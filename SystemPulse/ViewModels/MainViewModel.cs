using CommunityToolkit.Mvvm.ComponentModel;
using Injectio.Attributes;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Timers;
using SystemPulse.Models.Hardware;
using SystemPulse.Services;

namespace SystemPulse.ViewModels;

[RegisterTransient]
[SupportedOSPlatform("windows")]
public partial class MainViewModel : ViewModelBase {
    private readonly string _ipServiceUrl = "https://api.ipify.org";

    private readonly HardwareMonitorService _hwMonitor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HourRotation), nameof(MinuteRotation))]
    private DateTime _now;
    public float HourRotation => Now.Hour / 12f * 360f;
    public float MinuteRotation => Now.Minute / 60f * 360f;

    public HardwareCollection Hardwares { get; } = [];
    public ObservableCollection<PhysicalCore> PhysicalCores { get; init; }

    [ObservableProperty]
    private ObservableCollection<Drive> _drives = [];

    [ObservableProperty]
    private string _ipAddress = "";

    public MainViewModel(HardwareMonitorService hwMonitor) {
        _hwMonitor = hwMonitor;

        var cpu = hwMonitor.Computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu).First();
        var cpuHardware = new CpuHardwareItem(cpu);
        Hardwares.Add(cpuHardware);
        PhysicalCores = cpuHardware.PhysicalCores;

        // =========================================================

        var ram = hwMonitor.Computer.Hardware.Where(h => h.HardwareType == HardwareType.Memory).First();
        Hardwares.Add(new HardwareItem(ram));

        // =========================================================

        var gpu = hwMonitor.Computer.Hardware.Where(h => h.HardwareType is HardwareType.GpuAmd or HardwareType.GpuIntel or HardwareType.GpuNvidia).First();
        gpu.Update();
        Hardwares.Add(new HardwareItem(gpu));

        // =========================================================

        var drives = DriveInfo.GetDrives();
        foreach (var drive in drives) {
            _drives.Add(new Drive(drive));
        }

        // =========================================================

        SetupNetworkMonitoring();

        NetworkChange.NetworkAddressChanged += async (sender, args) => await GetExternalIpAsync();
        _ = GetExternalIpAsync();

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
            var _net = _hwMonitor.Computer.Hardware.Where(h => h.HardwareType == HardwareType.Network && h.Name.Contains(adapter.Name, StringComparison.OrdinalIgnoreCase)).First();
            Hardwares.Add(new HardwareItem(_net));
        }
    }

    static NetworkInterface? GetActiveNetworkAdapter() {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic =>
                nic.OperationalStatus == OperationalStatus.Up &&
                nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                nic.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork));

        var activeInterface = interfaces.FirstOrDefault();
        return activeInterface;
    }

    public void UpdateSensors(object? sender, ElapsedEventArgs e) {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => {
            Now = DateTime.Now;

            Hardwares.Update();

            foreach (var drive in Drives) {
                drive.Update();
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
