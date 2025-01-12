using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;

namespace SystemPulse.Models;
public partial class LogicalCore(int coreId, ISensor load) : ObservableObject {
    [ObservableProperty]
    private int _coreId = coreId;

    [ObservableProperty]
    private SensorValue _loadValue = new(load);
    public void UpdateSensorValue() {
        LoadValue.UpdateSensorValue();
    }
}
