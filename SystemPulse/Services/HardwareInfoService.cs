using CommunityToolkit.Mvvm.ComponentModel;
using Injectio.Attributes;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;
using LibreHardwareMonitor.Hardware.Storage;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using SystemPulse.Models.Hardware;

namespace SystemPulse.Services;

[RegisterSingleton]
public sealed partial class HardwareInfoService : ObservableObject, IDisposable, IUpdatable {
    private readonly Computer _computer;

    public HardwareCollection Hardwares { get; } = [];
    public ObservableCollection<PhysicalCore> PhysicalCores { get; } = [];
    public ObservableCollection<Drive> Drives { get; } = [];

    public HardwareInfoService(UpdateService updateService, Settings settings) {
        _computer = new Computer {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsNetworkEnabled = true,
            IsStorageEnabled = true,
        };
        _computer.Open();

        // =========================================================

        var cpu = (GenericCpu)_computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu).First();
        var cpuHardware = new HardwareItem(cpu);
        Hardwares.Add(cpuHardware);

        var cpuLoadSensors = cpuHardware.Sensors
            .Where(s => s.SensorType == SensorType.Load)
            .OfType<SensorItem>()
            .Where(s => s.Index >= 2) // 0 is total load, 1 is core max
            .ToArray();

        var coreSensors = cpuHardware.Sensors
            .OfType<SensorItem>()
            .Where(s => s.SensorType != SensorType.Load)
            .ToArray();

        if (cpu.CpuId.Any(p => p.Length > 1)) {
            for (int coreIdx = 0; coreIdx < cpu.CpuId.Length; coreIdx++) {
                var pcpu = cpu.CpuId[coreIdx];
                var physicalCore = CreatePhysicalCore(coreIdx, coreSensors);
                for (int threadIdx = 0; threadIdx < pcpu.Length; threadIdx++) {
                    var thread = pcpu[threadIdx];
                    if (thread.Thread < cpuLoadSensors.Length) {
                        physicalCore.LogicalCores.Add(new LogicalCore(thread.Thread, cpuLoadSensors[thread.Thread], physicalCore));
                    }
                }
                PhysicalCores.Add(physicalCore);
            }
        } else {
            // Workaround for LibreHardwareMonitor#1632
            int physicalCoreCount = coreSensors.Count(s =>
                s.SensorType == SensorType.Clock &&
                s.Name.StartsWith("Clock/Core#", StringComparison.Ordinal) &&
                !s.Name.EndsWith("(Effective)", StringComparison.Ordinal));

            int threadsPerCore = physicalCoreCount > 0 && cpu.CpuId.Length % physicalCoreCount == 0
                ? cpu.CpuId.Length / physicalCoreCount
                : 2;

            for (int coreIdx = 0; coreIdx < cpu.CpuId.Length; coreIdx += threadsPerCore) {
                int physicalIdx = coreIdx / threadsPerCore;
                var physicalCore = CreatePhysicalCore(physicalIdx, coreSensors);
                for (int threadIdx = 0; threadIdx < threadsPerCore && (coreIdx + threadIdx) < cpu.CpuId.Length; threadIdx++) {
                    var thread = cpu.CpuId[coreIdx + threadIdx][0];
                    if (thread.Thread < cpuLoadSensors.Length) {
                        physicalCore.LogicalCores.Add(new LogicalCore(thread.Thread, cpuLoadSensors[thread.Thread], physicalCore));
                    }
                }
                PhysicalCores.Add(physicalCore);
            }
        }

        // =========================================================

        var ram = _computer.Hardware.Where(h => h.HardwareType == HardwareType.Memory && h.Name.Equals("Total Memory", StringComparison.Ordinal)).First();
        Hardwares.Add(new HardwareItem(ram));

        // =========================================================

        var gpu = _computer.Hardware.Where(h => h.HardwareType is HardwareType.GpuAmd or HardwareType.GpuIntel or HardwareType.GpuNvidia)
            .OrderByDescending(h => h.Sensors.First(s => s.Name == "GPU Memory Total").Value) // Take the one with the most memory to filter out integrated Gpu
            .First();
        Hardwares.Add(new HardwareItem(gpu));

        // =========================================================

        var storage = _computer.Hardware
            .OfType<StorageDevice>()
            .ToArray();

        var drives = DriveInfo.GetDrives()
            .Where(d => d.IsReady && d.DriveType == DriveType.Fixed);

        foreach (var drive in drives) {
            char driveLetter = drive.Name[0];

            var hardware = storage.FirstOrDefault(h => h.Storage.Partitions
                .Any(p => char.ToUpperInvariant(p.DriveLetter ?? '\0') == char.ToUpperInvariant(driveLetter)));

            if (hardware is not null) {
                Drives.Add(new Drive(hardware, drive));
            }
        }

        // =========================================================

        var adapter = GetActiveNetworkAdapter();
        if (adapter is not null) {
            var net = _computer.Hardware.Where(h => h.HardwareType == HardwareType.Network && h.Name.Contains(adapter.Name, StringComparison.OrdinalIgnoreCase)).First();
            Hardwares.Add(new HardwareItem(net));
        }

        settings.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(Settings.RefreshInterval)) {
                updateService.Unregister(this);
                updateService.Register(this, Settings.Default!.RefreshInterval);
            }
        };

        updateService.Register(this, settings.RefreshInterval);
        Update();
    }

    private static PhysicalCore CreatePhysicalCore(int coreIdx, SensorItem[] coreSensors) {
        var core = new PhysicalCore(coreIdx);
        string token = $"Core#{coreIdx + 1}";

        foreach (var sensor in coreSensors) {
            int idx = sensor.Name.IndexOf(token, StringComparison.Ordinal);
            if (idx >= 0 && (idx + token.Length == sensor.Name.Length || !char.IsDigit(sensor.Name[idx + token.Length]))) {
                string normalizedName = sensor.Name.Remove(idx + "Core".Length, token.Length - "Core".Length);
                core.Sensors.Add(new SensorItem(sensor.Sensor, normalizedName));
            }
        }

        return core;
    }

    private static NetworkInterface? GetActiveNetworkAdapter() {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic =>
                nic.OperationalStatus == OperationalStatus.Up &&
                nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                nic.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork));

        var activeInterface = interfaces.FirstOrDefault();
        return activeInterface;
    }

    public void Update() {
        Hardwares.Update();

        foreach (var core in PhysicalCores) {
            core.Update();
        }

        foreach (var drive in Drives) {
            drive.Update();
        }
    }

    public void Dispose() {
        UpdateService.Default?.Unregister(this);
        _computer.Close();
    }
}