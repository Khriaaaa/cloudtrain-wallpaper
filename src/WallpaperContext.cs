using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CloudTrainWallpaper;

/// <summary>ApplicationContext：壁纸窗口 + 托盘。壁纸窗口挂在桌面 WorkerW 后面，托盘提供参数面板/退出。</summary>
class WallpaperContext : ApplicationContext
{
    readonly WallpaperForm _wallpaper;
    readonly TrayService _tray;
    readonly Settings _settings;

    public WallpaperContext()
    {
        _settings = Settings.Load();

        _wallpaper = new WallpaperForm(_settings);
        _wallpaper.Show();

        _tray = new TrayService(_settings, _wallpaper, () =>
        {
            _tray.Dispose();
            Application.Exit();
        });

        // 尝试把自己放到原壁纸窗口后面（Progman/WorkerW 手法）
        WallpaperForm.SpawnWorkerW();
        _wallpaper.AttachToDesktop();
    }
}

internal static class Native
{
    [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr result);
    [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr FindWindow(string? cls, string? title);
    [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string? cls, string? title);
    [DllImport("user32.dll", SetLastError = true)] public static extern bool EnumWindows(EnumWindowsProc cb, IntPtr lp);
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")] public static extern IntPtr SHAppBarMessage(uint msg, ref APPBARDATA data);
    [DllImport("user32.dll")] public static extern uint GetWindowLong(IntPtr hWnd, int index);
    [DllImport("user32.dll")] public static extern int SetWindowLong(IntPtr hWnd, int index, uint value);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int w, int h, uint flags);
    [DllImport("user32.dll")] public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);
    [DllImport("user32.dll")] public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO info);

    public const uint SPI_GETWORKAREA = 0x0048;
    public const int GWL_EXSTYLE = -20;
    public const uint WS_EX_TOOLWINDOW = 0x00000080;
    public const uint SWP_NOSIZE = 0x0001, SWP_NOMOVE = 0x0002, SWP_NOACTIVATE = 0x0010;
    public const uint MONITOR_DEFAULTTONEAREST = 2;
}

[StructLayout(LayoutKind.Sequential)]
struct APPBARDATA { public int cbSize; public IntPtr hWnd; public uint uCallbackMessage; public uint uEdge; public RECT rc; public int lParam; }

[StructLayout(LayoutKind.Sequential)]
public struct RECT { public int Left, Top, Right, Bottom; }

[StructLayout(LayoutKind.Sequential)]
struct MONITORINFO { public int cbSize; public RECT rcMonitor; public RECT rcWork; public uint dwFlags; }
