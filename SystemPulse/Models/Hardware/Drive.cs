using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;
using System.IO;

namespace SystemPulse.Models.Hardware;

public partial class Drive : HardwareItem {
    private readonly DriveInfo _driveInfo;

    public string VolumeLabel => _driveInfo.VolumeLabel;
    public string DriveFormat => _driveInfo.DriveFormat;

    [ObservableProperty]
    private double _usedSpace;

    public Drive(IHardware hardware, DriveInfo driveInfo) : base(hardware) {
        _driveInfo = driveInfo;
        UpdateUsedSpace();
    }

    public override void Update() {
        base.Update();
        UpdateUsedSpace();
    }

    private void UpdateUsedSpace() {
        UsedSpace = (_driveInfo.TotalSize - _driveInfo.AvailableFreeSpace) / (1024d * 1024d * 1024d);
    }
}