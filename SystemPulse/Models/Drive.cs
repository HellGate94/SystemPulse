using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace SystemPulse.Models;
[SupportedOSPlatform("windows")]
public sealed partial class Drive(string name, PerformanceCounter load/*, PerformanceCounter free*/) : ObservableObject, IDisposable {
    [ObservableProperty]
    string _name = name;

    [ObservableProperty]
    PerMonValue _load = new(load, static (v) => 100f - v);

    //[ObservableProperty]
    //PerMonValue _free = new(free, static (v) => v / 1024f);

    public void Dispose() {
        Load.Dispose();
        //Free.Dispose();
    }

    public void UpdateSensorValue() {
        Load.UpdateSensorValue();
        //Free.UpdateSensorValue();
    }
}