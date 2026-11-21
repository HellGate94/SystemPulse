using System.Collections.ObjectModel;

namespace SystemPulse.Models.Hardware;

public record LogicalCore(int CoreId, SensorItem LoadValue, PhysicalCore? PhysicalCore = null);

public record PhysicalCore(int CoreId) {
    public int DisplayNumber => CoreId + 1;
    public ObservableCollection<LogicalCore> LogicalCores { get; } = [];
    public SensorCollection Sensors { get; } = [];

    public void Update() {
        foreach (var sensor in Sensors) {
            sensor.Update();
        }
    }
}