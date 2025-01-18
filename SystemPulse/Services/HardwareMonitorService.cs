using Injectio.Attributes;
using LibreHardwareMonitor.Hardware;
using System;

namespace SystemPulse.Services;

[RegisterSingleton]
public sealed class HardwareMonitorService : IDisposable {
    public Computer Computer { get; init; }

    public HardwareMonitorService() {
        Computer = new Computer {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsNetworkEnabled = true,
        };

        Computer.Open();
    }

    public void Dispose() {
        Computer.Close();
    }
}
