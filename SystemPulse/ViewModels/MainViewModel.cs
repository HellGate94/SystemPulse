using CommunityToolkit.Mvvm.ComponentModel;
using Injectio.Attributes;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using SystemPulse.Models.Hardware;
using SystemPulse.Services;

namespace SystemPulse.ViewModels;

[RegisterTransient]
[SupportedOSPlatform("windows")]
public sealed partial class MainViewModel : ViewModelBase, IDisposable, IUpdatable {
    private readonly HardwareInfoService _hwInfoService;
    private readonly Settings _settings;

    [ObservableProperty]
    private DateTime _now;

    public HardwareCollection Hardwares => _hwInfoService.Hardwares;
    public ObservableCollection<PhysicalCore> PhysicalCores => _hwInfoService.PhysicalCores;
    public ObservableCollection<Drive> Drives => _hwInfoService.Drives;

    [ObservableProperty]
    private string _ipAddress = "";

    public MainViewModel(HardwareInfoService hwInfoService, Settings settings, UpdateService updateService) {
        _hwInfoService = hwInfoService;
        _settings = settings;

        NetworkChange.NetworkAddressChanged += async (sender, args) => await GetExternalIpAsync();
        _ = GetExternalIpAsync();

        updateService.Register(this, 1000);
        Update();
    }
    public void Update() {
        Now = DateTime.Now;
    }

    private async Task GetExternalIpAsync() {
        using var httpClient = new HttpClient();
        try {
            IpAddress = await httpClient.GetStringAsync(_settings.IPService);
        } catch (Exception) {
            IpAddress = "";
        }
    }

    public void Dispose() {
        UpdateService.Default!.Unregister(this);
    }
}
