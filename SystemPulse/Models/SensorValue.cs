using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace SystemPulse.Models;
public partial class SensorValue(ISensor sensor) : ObservableObject {
    [ObservableProperty]
    private float _value = 0f;

    [ObservableProperty]
    private bool _isAlert = false;

    public void UpdateSensorValue() {
        Value = sensor.Value ?? 0f;
        IsAlert = Value > 95f;
    }
}

[SupportedOSPlatform("windows")]
public sealed partial class PerMonValue(PerformanceCounter sensor, Func<float, float> converter) : ObservableObject, IDisposable {
    [ObservableProperty]
    private float _value = 0f;

    [ObservableProperty]
    private bool _isAlert = false;

    public void Dispose() {
        sensor.Dispose();
    }

    public void UpdateSensorValue() {
        Value = converter.Invoke(sensor.NextValue());
        IsAlert = Value > 95f;
    }
}