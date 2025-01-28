using Avalonia.Controls;
using CircularBuffer;
using CommunityToolkit.Mvvm.ComponentModel;
using Dia2Lib;
using Injectio.Attributes;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using SystemPulse.Models.Hardware;
using SystemPulse.Services;

namespace SystemPulse.ViewModels;

[RegisterTransient]
[SupportedOSPlatform("windows")]
public partial class OverlayViewModel : ViewModelBase, IDisposable, IUpdatable {
    private readonly HardwareInfoService _hwInfoService;

    public HardwareCollection Hardwares => _hwInfoService.Hardwares;

    //event codes (https://github.com/GameTechDev/PresentMon/blob/40ee99f437bc1061a27a2fc16a8993ee8ce4ebb5/PresentData/PresentMonTraceConsumer.cpp)
    public const ushort EventID_DxgiPresentStart = 42;
    private const double maxTimeDelta = 1000d;
    private const int maxBufferSize = 1000;

    //ETW provider codes
    public static readonly Guid DXGI_provider = Guid.Parse("{CA11C036-0102-4A2D-A6AD-F03CFED5D3C9}");

    TraceEventSession session;
    int targetPid;
    object sync = new object();

    CircularBuffer<double> frametimes = new(maxBufferSize);
    double[] frametimeBuffer = new double[maxBufferSize];
    double lastTime = 0d;

    [ObservableProperty]
    private double _fps = 0;

    [ObservableProperty]
    private double _fpsLow = 0;

    public OverlayViewModel(HardwareInfoService hwInfoService, UpdateService updateService) {
        _hwInfoService = hwInfoService;
        if (Design.IsDesignMode) return;

        var targetWindow = Native.GetForegroundWindow();
        Native.GetWindowThreadProcessId(targetWindow, out targetPid);

        session = new TraceEventSession("SystemPulse") {
            StopOnDispose = true,
        };
        session.EnableProvider("Microsoft-Windows-DXGI");

        session.Source.AllEvents += data => {
            if (data.ProcessID == targetPid) {
                if (data.ProviderGuid == DXGI_provider && (ushort)data.ID == EventID_DxgiPresentStart) {
                    var delta = data.TimeStampRelativeMSec - lastTime;
                    frametimes.PushFront(delta);
                    lastTime = data.TimeStampRelativeMSec;
                }
            }
        };

        Thread etwThread = new Thread(() => session.Source.Process()) {
            IsBackground = true,
        };
        etwThread.Start();

        updateService.Register(this, 1000);
    }

    public void Update() {
        double sum = 0d;
        double avg = 0d;
        int i;
        for (i = 0; i < frametimes.Size; i++) {
            sum += frametimes[i];
            frametimeBuffer[i] = frametimes[i];
            if (sum > maxTimeDelta) {
                break;
            }
        }
        avg = sum / (i + 1d);

        var high = PercentileFinder.GetPercentile(frametimeBuffer[0..i], 99d);

        Fps = 1000d / avg;
        FpsLow = 1000d / high;
    }

    public void Dispose() {
        UpdateService.Default.Unregister(this);
        session.Stop();
        session.Dispose();
    }
}

public static class PercentileFinder {
    public static double GetPercentile(Span<double> span, double percentile) {
        if (span.IsEmpty)
            throw new ArgumentException("Span cannot be empty.");

        if (percentile < 0 || percentile > 100)
            throw new ArgumentOutOfRangeException(nameof(percentile), "Percentile must be between 0 and 100.");

        int n = span.Length;
        int k = (int)(percentile / 100.0 * (n - 1));

        // Use Quickselect to find the k-th smallest element (which is our percentile)
        return QuickSelect(span, 0, n - 1, k);
    }

    private static double QuickSelect(Span<double> span, int left, int right, int k) {
        if (left == right)
            return span[left];

        int pivotIndex = Partition(span, left, right);
        if (k == pivotIndex)
            return span[k];
        else if (k < pivotIndex)
            return QuickSelect(span, left, pivotIndex - 1, k);
        else
            return QuickSelect(span, pivotIndex + 1, right, k);
    }

    private static int Partition(Span<double> span, int left, int right) {
        double pivot = span[right];
        int i = left;

        for (int j = left; j < right; j++) {
            if (span[j] <= pivot) {
                Swap(span, i, j);
                i++;
            }
        }

        Swap(span, i, right);
        return i;
    }

    private static void Swap(Span<double> span, int i, int j) {
        double temp = span[i];
        span[i] = span[j];
        span[j] = temp;
    }
}