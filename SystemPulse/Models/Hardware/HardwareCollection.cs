using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;
using System.Collections.ObjectModel;
using System.Linq;

namespace SystemPulse.Models.Hardware;

public interface IHardwareItem {
    string Name { get; }
    SensorCollection Sensors { get; }
    void Update();
}

public class HardwareCollection : ObservableCollection<IHardwareItem> {
    public IHardwareItem this[string value] => this.First(h => h.Name == value);

    public void Update() {
        foreach (var hardware in this) {
            hardware.Update();
        }
    }
}

public partial class HardwareItem : ObservableObject, IHardwareItem {
    private static readonly string[] _hardwareTypeMapping = [
        "Motherboard",
        "SuperIO",
        "Cpu",
        "Memory",
        "Gpu",
        "Gpu",
        "Gpu",
        "Storage",
        "Network",
        "Cooler",
        "EmbeddedController",
        "Psu",
        "Battery",
    ];

    public IHardware Hardware;

    public string Name => _hardwareTypeMapping[(int)Hardware.HardwareType];

    public SensorCollection Sensors { get; } = [];

    public HardwareItem(IHardware hardware) {
        Hardware = hardware;
        foreach (var sensor in hardware.Sensors) {
            Sensors.Add(new SensorItem(sensor));
        }
    }

    public void Update() {
        Hardware.Update();
        foreach (var sensor in Sensors) {
            sensor.Update();
        }
    }
}

public partial class GenericHardwareItem(string name) : ObservableObject, IHardwareItem {
    public SensorCollection Sensors { get; } = [];

    public string Name => name;

    public void Update() {
        foreach (var sensor in Sensors) {
            sensor.Update();
        }
    }
}