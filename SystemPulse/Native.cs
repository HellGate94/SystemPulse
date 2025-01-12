using Avalonia.Platform;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SystemPulse;
public static partial class Native {
    public const int ABM_NEW = 0;
    public const int ABM_REMOVE = 1;
    public const int ABM_QUERYPOS = 2;
    public const int ABM_SETPOS = 3;

    public enum ABEdge {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA {
        public int cbSize;
        public nint hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public int lParam;
    }

    [LibraryImport("shell32.dll", SetLastError = true)]
    public static partial nint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    public static bool CreateAppBar(nint appBarHandle) {
        var appBarData = new APPBARDATA {
            cbSize = Marshal.SizeOf<APPBARDATA>(),
            hWnd = appBarHandle,
        };

        return SHAppBarMessage(ABM_NEW, ref appBarData) != nint.Zero;
    }

    public static bool RemoveAppBar(nint appBarHandle) {
        var appBarData = new APPBARDATA {
            cbSize = Marshal.SizeOf<APPBARDATA>(),
            hWnd = appBarHandle,
        };

        return SHAppBarMessage(ABM_REMOVE, ref appBarData) != nint.Zero;
    }

    public static PixelRect SetAppBarPosition(nint appBarHandle, Screen targetScreen, Size size) {
        var bounds = targetScreen.Bounds;

        var appBarData = new APPBARDATA {
            cbSize = Marshal.SizeOf<APPBARDATA>(),
            hWnd = appBarHandle,
            uEdge = (int)ABEdge.Right,
            rc = new RECT {
                Left = bounds.Right - (int)size.Width,
                Top = bounds.TopLeft.Y,
                Right = bounds.Right,
                Bottom = bounds.Bottom
            }
        };

        SHAppBarMessage(ABM_QUERYPOS, ref appBarData);
        SHAppBarMessage(ABM_SETPOS, ref appBarData);

        return new PixelRect(appBarData.rc.Left, appBarData.rc.Top, appBarData.rc.Right - appBarData.rc.Left, appBarData.rc.Bottom - appBarData.rc.Top);

        // Set the window's position and size to match the app bar
        //Position = new PixelPoint(appBarData.rc.Left, appBarData.rc.Top);
        //Width = appBarData.rc.Right - appBarData.rc.Left;
        //Height = appBarData.rc.Bottom - appBarData.rc.Top;
    }
}
