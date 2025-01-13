using CommunityToolkit.Mvvm.ComponentModel;
using Injectio.Attributes;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Timers;
using SystemPulse.Models;
using SystemPulse.Services;
using SValue = SystemPulse.Models.SensorValue;

namespace SystemPulse.ViewModels;

[RegisterTransient]
[SupportedOSPlatform("windows")]
public partial class MainViewModel : ViewModelBase {
    private readonly string _ipServiceUrl = "https://api.ipify.org";

    private readonly HardwareMonitorService _hwMonitor;

    [ObservableProperty]
    [NotifyPropertyChangedFor("HourRotation", "MinuteRotation")]
    private DateTime _now;
    public float HourRotation => Now.Hour / 12f * 360f;
    public float MinuteRotation => Now.Minute / 60f * 360f;

    [ObservableProperty]
    ObservableCollection<PhysicalCores> _physicalCores = [];

    [ObservableProperty]
    private SValue _ramUsage;

    [ObservableProperty]
    private SValue _gpuUsage;
    [ObservableProperty]
    private SValue _vramUsage;

    [ObservableProperty]
    private ObservableCollection<Drive> _drives = [];

    [ObservableProperty]
    private string _ipAddress = "";

    [ObservableProperty]
    private SValue? _networkUsage;
    [ObservableProperty]
    private float _networkMax;

    private readonly GenericCpu _cpu;
    private readonly IHardware _ram;
    private readonly IHardware _gpu;
    private IHardware? _net;

    public MainViewModel(HardwareMonitorService hwMonitor) {
        _hwMonitor = hwMonitor;
        _cpu = hwMonitor.Computer.Hardware
            .Where(h => h.HardwareType == HardwareType.Cpu).
            First() as GenericCpu ?? throw new Exception();
        _cpu.Update();

        var cpuLoadSensors = _cpu.Sensors
            .Where(s => s.SensorType == SensorType.Load)
            .Where(s => s.Index >= 2) // 0 is total load, 1 is core max
            .ToArray();

        for (int coreIdx = 0; coreIdx < _cpu.CpuId.Length; coreIdx++) {
            var pcpu = _cpu.CpuId[coreIdx];
            PhysicalCores physicalCore = new(coreIdx);
            for (int threadIdx = 0; threadIdx < pcpu.Length; threadIdx++) {
                var thread = _cpu.CpuId[coreIdx][threadIdx];
                LogicalCore logicalCore = new(thread.Thread, cpuLoadSensors[thread.Thread]);
                physicalCore.LogicalCores.Add(logicalCore);
            }
            PhysicalCores.Add(physicalCore);
        }

        // =========================================================

        _ram = hwMonitor.Computer.Hardware.Where(h => h.HardwareType == HardwareType.Memory).First();
        _ram.Update();

        RamUsage = new(_ram.Sensors.First(s => s.Name == "Memory"));

        // =========================================================

        _gpu = hwMonitor.Computer.Hardware.Where(h => h.HardwareType is HardwareType.GpuAmd or HardwareType.GpuIntel or HardwareType.GpuNvidia).First();
        _gpu.Update();

        _gpuUsage = new(_gpu.Sensors.First(s => s.SensorType == SensorType.Load && s.Name == "GPU Core"));
        _vramUsage = new(_gpu.Sensors.First(s => s.SensorType == SensorType.Load && s.Name == "GPU Memory"));

        // =========================================================

        var driveCounters = new PerformanceCounterCategory("LogicalDisk");
        var drives = driveCounters.GetInstanceNames().Where(n => n.Length == 2 && n.EndsWith(':')).OrderBy(d => d[0]);
        foreach (var drive in drives) {
            var usedSpaceCounter = new PerformanceCounter("LogicalDisk", "% Free Space", drive);
            //var freeSpaceCounter = new PerformanceCounter("LogicalDisk", "Free Megabytes", drive);

            _drives.Add(new Drive(drive, usedSpaceCounter));
        }

        // =========================================================

        NetworkChange.NetworkAddressChanged += async (sender, args) => await GetExternalIpAsync();
        _ = GetExternalIpAsync();

        SetupNetworkMonitoring();

        // =========================================================

        var chkDate = new Timer {
            Interval = 1000,
            AutoReset = true,
            Enabled = true
        };
        chkDate.Elapsed += UpdateSensors;
        UpdateSensors(null, null);
    }

    private void SetupNetworkMonitoring() {
        var adapter = GetActiveNetworkAdapter();
        if (adapter is not null) {
            _net = _hwMonitor.Computer.Hardware.Where(h => h.HardwareType == HardwareType.Network && h.Name.Contains(adapter.Name, StringComparison.OrdinalIgnoreCase)).First();
            _net.Update();

            NetworkUsage = new(_net.Sensors.Where(s => s.SensorType == SensorType.Throughput && s.Name == "Download Speed").First());
        }
    }

    static NetworkInterface? GetActiveNetworkAdapter() {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic =>
                nic.OperationalStatus == OperationalStatus.Up &&
                nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                nic.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork));

        // Return the name of the first interface with a valid IPv4 gateway.
        var activeInterface = interfaces.FirstOrDefault();
        return activeInterface;
    }

    public void UpdateSensors(object? sender, ElapsedEventArgs e) {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => {
            Now = DateTime.Now;

            _cpu.Update();
            foreach (var pcore in PhysicalCores) {
                foreach (var lcore in pcore.LogicalCores) {
                    lcore.UpdateSensorValue();
                }
            }

            _ram.Update();
            RamUsage.UpdateSensorValue();

            _gpu.Update();
            GpuUsage.UpdateSensorValue();
            VramUsage.UpdateSensorValue();

            foreach (var drive in Drives) {
                drive.UpdateSensorValue();
            }

            if (_net is not null) {
                _net.Update();
                NetworkUsage.UpdateSensorValue();
                NetworkMax = MathF.Max(NetworkMax, NetworkUsage.Value);
            }
        }, Avalonia.Threading.DispatcherPriority.Background);
    }

    private async Task GetExternalIpAsync() {
        using var httpClient = new HttpClient();
        try {
            IpAddress = await httpClient.GetStringAsync(_ipServiceUrl);
        } catch (Exception) {
            IpAddress = "";
        }
    }
}
