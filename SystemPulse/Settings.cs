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
    [Url]
    private string _iPService = "https://api.ipify.org";

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0, 60f * 1000000f / 8f)]
    private float _downloadBandwidth = 50f * 1000000f / 8f;

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0, double.MaxValue)]
    private float _uploadBandwidth = 10f * 1000000f / 8f;

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0, int.MaxValue)]
    private int _targetScreen = 0;

    [ObservableProperty]
    private Side _side = Side.Right;
}