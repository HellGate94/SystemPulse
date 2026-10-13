using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace SystemPulse;

public enum Side {
    Left,
    Right,
}

public partial class Settings : ObservableValidator {
    public static Settings? Default => Ioc.Default.GetService<Settings>();

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(500, 10000)]
    private int _refreshInterval = 1000;

    [ObservableProperty, NotifyDataErrorInfo]
    [Url]
    private string _iPService = "https://api.ipify.org";

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0, double.MaxValue)]
    private double _downloadBandwidth = 50.0 * 1000000.0 / 8.0;

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0, double.MaxValue)]
    private double _uploadBandwidth = 10.0 * 1000000.0 / 8.0;

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0, int.MaxValue)]
    private int _targetScreen = 0;

    [ObservableProperty]
    private Side _side = Side.Right;
}