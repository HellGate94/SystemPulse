using Avalonia;
using Avalonia.Platform;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using Windows.Win32.UI.Shell;

namespace SystemPulse;

public static class Native {
    private const uint ABM_NEW = 0;
    private const uint ABM_REMOVE = 1;
    private const uint ABM_QUERYPOS = 2;
    private const uint ABM_SETPOS = 3;
    private const uint ABM_GETSTATE = 4;
    private const uint ABM_GETTASKBARPOS = 5;
    private const uint ABM_ACTIVATE = 6;
    private const uint ABM_GETAUTOHIDEBAR = 7;
    private const uint ABM_SETAUTOHIDEBAR = 8;
    private const uint ABM_WINDOWPOSCHANGED = 9;
    private const uint ABM_SETSTATE = 10;
    private const uint ABM_GETAUTOHIDEBAREX = 11;
    private const uint ABM_SETAUTOHIDEBAREX = 12;

    public static bool CreateAppBar(nint appBarHandle) {
        var appBarData = new APPBARDATA {
            cbSize = (uint)Marshal.SizeOf<APPBARDATA>(),
            hWnd = (HWND)appBarHandle,
        };

        return PInvoke.SHAppBarMessage(ABM_NEW, ref appBarData) != nuint.Zero;
    }

    public static bool RemoveAppBar(nint appBarHandle) {
        var appBarData = new APPBARDATA {
            cbSize = (uint)Marshal.SizeOf<APPBARDATA>(),
            hWnd = (HWND)appBarHandle,
        };

        return PInvoke.SHAppBarMessage(ABM_REMOVE, ref appBarData) != nuint.Zero;
    }

    public static PixelRect SetAppBarPosition(nint appBarHandle, Screen targetScreen, Size size, Side side) {
        var appBarData = new APPBARDATA {
            cbSize = (uint)Marshal.SizeOf<APPBARDATA>(),
            hWnd = (HWND)appBarHandle,
        };

        appBarData.uEdge = side switch {
            Side.Left => 0,
            Side.Right or _ => 2,
        };

        var bounds = targetScreen.Bounds;
        PixelPoint lr = side switch {
            Side.Left => new(bounds.X, bounds.X + (int)(size.Width * targetScreen.Scaling)),
            Side.Right or _ => new(bounds.Right - (int)(size.Width * targetScreen.Scaling), bounds.Right),
        };
        appBarData.rc = new RECT(lr.X, bounds.Y, lr.Y, bounds.Bottom);

        //PInvoke.SHAppBarMessage(ABM_QUERYPOS, ref appBarData);
        PInvoke.SHAppBarMessage(ABM_SETPOS, ref appBarData);

        return new PixelRect(appBarData.rc.left, appBarData.rc.top, appBarData.rc.Width, appBarData.rc.Height);
    }

    // ===========================================================

    private const uint PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;
    private const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 1;

    public static unsafe void EnablePowerThrottling() {
        using Process process = Process.GetCurrentProcess();
        using SafeFileHandle hProcess = new(process.Handle, false);

        PInvoke.SetPriorityClass(hProcess, PROCESS_CREATION_FLAGS.IDLE_PRIORITY_CLASS);

        PROCESS_POWER_THROTTLING_STATE state = new() {
            Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
            ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
            StateMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED
        };

        PInvoke.SetProcessInformation(
            hProcess,
            PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
            &state,
            (uint)Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>()
        );
    }
}
