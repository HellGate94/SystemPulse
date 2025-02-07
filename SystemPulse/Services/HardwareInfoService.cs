using CommunityToolkit.Mvvm.ComponentModel;
using Injectio.Attributes;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;
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

    public HardwareInfoService(UpdateService updateService) {
        _computer = new Computer {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsNetworkEnabled = true,
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

        for (int coreIdx = 0; coreIdx < cpu.CpuId.Length; coreIdx++) {
            var pcpu = cpu.CpuId[coreIdx];
            PhysicalCore physicalCore = new(coreIdx);
            for (int threadIdx = 0; threadIdx < pcpu.Length; threadIdx++) {
                var thread = cpu.CpuId[coreIdx][threadIdx];
                LogicalCore logicalCore = new(thread.Thread, cpuLoadSensors[thread.Thread]);
                physicalCore.LogicalCores.Add(logicalCore);
            }
            PhysicalCores.Add(physicalCore);
        }

        // =========================================================

        var ram = _computer.Hardware.Where(h => h.HardwareType == HardwareType.Memory).First();
        Hardwares.Add(new HardwareItem(ram));

        // =========================================================

        var gpu = _computer.Hardware.Where(h => h.HardwareType is HardwareType.GpuAmd or HardwareType.GpuIntel or HardwareType.GpuNvidia).First();
        gpu.Update();
        Hardwares.Add(new HardwareItem(gpu));

        // =========================================================

        var drives = DriveInfo.GetDrives();
        foreach (var drive in drives) {
            Drives.Add(new Drive(drive));
        }

        // =========================================================

        var adapter = GetActiveNetworkAdapter();
        if (adapter is not null) {
            var net = _computer.Hardware.Where(h => h.HardwareType == HardwareType.Network && h.Name.Contains(adapter.Name, StringComparison.OrdinalIgnoreCase)).First();
            Hardwares.Add(new HardwareItem(net));
        }

        updateService.Register(this, 1000);
        Update();
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

        foreach (var drive in Drives) {
            drive.Update();
        }
    }

    public void Dispose() {
        UpdateService.Default!.Unregister(this);
        _computer.Close();
    }
}
