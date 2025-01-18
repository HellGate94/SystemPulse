using CommunityToolkit.Mvvm.DependencyInjection;
using Config.Net;
using System.ComponentModel;

namespace SystemPulse;

public enum Side {
    Left,
    Right,
}
public interface ISettings : INotifyPropertyChanged {
    [Option(DefaultValue = 50f * 1000000f / 8f)]
    float DownloadBandwidth { get; set; }

    [Option(DefaultValue = 10f * 1000000f / 8f)]
    float UploadBandwidth { get; set; }

    [Option(DefaultValue = 0)]
    int TargetScreen { get; set; }

    [Option(DefaultValue = Side.Right)]
    Side Side { get; set; }
}

public static class Settings {
    public static ISettings? Default => Ioc.Default.GetService<ISettings>();
}