using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;
using System.IO;
using System.Runtime.Versioning;

namespace SystemPulse.Models.Hardware;
[SupportedOSPlatform("windows")]
public sealed partial class Drive : ObservableObject, IHardwareItem {
    private readonly DriveInfo _driveInfo;
    public string Name => _driveInfo.Name.Replace("\\", "");

    public SensorCollection Sensors { get; } = [];

    public Drive(DriveInfo driveInfo) {
        _driveInfo = driveInfo;
        Sensors.Add(new CustomSensorItem("Load/FreeSpace", () => driveInfo.AvailableFreeSpace, SensorType.Load));
        Sensors.Add(new CustomSensorItem("Load/TotalSpace", () => driveInfo.TotalSize, SensorType.Load));
        Sensors.Add(new CustomSensorItem("Load/UsedSpace", () => driveInfo.TotalSize - driveInfo.AvailableFreeSpace, SensorType.Load));
        Sensors.Add(new CustomSensorItem("Load", () => (1f - (driveInfo.AvailableFreeSpace / (float)driveInfo.TotalSize)) * 100f, SensorType.Load));
    }

    public void Update() {
        foreach (var sensor in Sensors) {
            sensor.Update();
        }
    }
}