using CommunityToolkit.Mvvm.ComponentModel;

namespace SystemPulse;
public partial class Settings : ObservableObject {
    public static Settings Default { get; } = new Settings();

    [ObservableProperty]
    private float _bandwidth = (50 * 1000000) / 8;

    [ObservableProperty]
    private int _targetScreen = 1;
}
