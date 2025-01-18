using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SystemPulse.Models.Hardware;

public interface ISensorItem {
    string Name { get; }
    float Value { get; }
    bool IsAlert { get; }
    SensorType SensorType { get; }
    void Update();
}
public class SensorCollection : ObservableCollection<ISensorItem> {
    public ISensorItem this[string value] => this.First(h => h.Name == value);
}
public partial class SensorItem : ObservableObject, ISensorItem {
    [ObservableProperty]
    private ISensor _sensor;

    public string Name { get; init; }

    public SensorType SensorType => Sensor.SensorType;
    public int Index => Sensor.Index;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAlert))]
    private float _value = 0f;

    public bool IsAlert => Value > 95f;

    public SensorItem(ISensor sensor) {
        Sensor = sensor;
        Name = $"{sensor.SensorType}/{sensor.Name.Replace(" ", "")}";
        Update();
    }

    public void Update() {
        Value = Sensor.Value ?? 0f;
    }
}

public sealed partial class CustomSensorItem(string name, Func<float> valueFunc, SensorType sensorType) : ObservableObject, ISensorItem {
    public string Name => name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAlert))]
    private float _value = 0f;

    public SensorType SensorType => sensorType;

    public bool IsAlert => Value > 95f;

    public void Update() {
        Value = valueFunc();
    }
}