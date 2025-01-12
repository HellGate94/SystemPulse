using Injectio.Attributes;
using LibreHardwareMonitor.Hardware;
using System;
using System.Diagnostics;
using System.Linq;

namespace SystemPulse.Services;

[RegisterSingleton]
public class HardwareMonitorService : IDisposable {
    private bool _disposed;
    public Computer Computer { get; private set; }

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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                Computer.Close();

                Computer = null!;
            }

            _disposed = true;
        }
    }

    ~HardwareMonitorService() {
        Dispose(false);
    }
}
