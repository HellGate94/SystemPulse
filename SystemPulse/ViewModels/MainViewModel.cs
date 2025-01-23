using CommunityToolkit.Mvvm.ComponentModel;
using Injectio.Attributes;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Timers;
using SystemPulse.Models.Hardware;
using SystemPulse.Services;

namespace SystemPulse.ViewModels;

[RegisterTransient]
[SupportedOSPlatform("windows")]
public partial class MainViewModel : ViewModelBase {
    private readonly HardwareInfoService _hwInfoService;
    private readonly Settings _settings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HourRotation), nameof(MinuteRotation))]
    private DateTime _now;
    public float HourRotation => Now.Hour / 12f * 360f;
    public float MinuteRotation => Now.Minute / 60f * 360f;

    public HardwareCollection Hardwares => _hwInfoService.Hardwares;
    public ObservableCollection<PhysicalCore> PhysicalCores => _hwInfoService.PhysicalCores;
    public ObservableCollection<Drive> Drives => _hwInfoService.Drives;

    [ObservableProperty]
    private string _ipAddress = "";

    public MainViewModel(HardwareInfoService hwInfoService, Settings settings) {
        _hwInfoService = hwInfoService;
        _settings = settings;

        NetworkChange.NetworkAddressChanged += async (sender, args) => await GetExternalIpAsync();
        _ = GetExternalIpAsync();

        var chkDate = new Timer {
            Interval = 1000,
            AutoReset = true,
            Enabled = true
        };
        chkDate.Elapsed += UpdateSensors;
        UpdateSensors(null, null);
    }

    private async Task GetExternalIpAsync() {
        using var httpClient = new HttpClient();
        try {
            IpAddress = await httpClient.GetStringAsync(_settings.IPService);
        } catch (Exception) {
            IpAddress = "";
        }
    }

    public void UpdateSensors(object? sender, ElapsedEventArgs e) {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => {
            Now = DateTime.Now;
            _hwInfoService.Update();
        }, Avalonia.Threading.DispatcherPriority.Background);
    }
}
