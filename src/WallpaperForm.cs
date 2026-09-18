using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CloudTrainWallpaper;

/// <summary>全屏无边框窗口，嵌入 WebView2 渲染壁纸页面，并沉降到桌面图标层后面。</summary>
class WallpaperForm : Form
{
    readonly Settings _settings;
    Microsoft.Web.WebView2.WinForms.WebView2? _web;

    public WallpaperForm(Settings settings)
    {
        _settings = settings;
        Text = "CloudTrainWallpaper";
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.Black;
        TopMost = false;

        Load += async (_, _) =>
        {
            _web = new Microsoft.Web.WebView2.WinForms.WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = Color.Black
            };
            Controls.Add(_web);
            var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(
                userDataFolder: SettingsData.UserDataDir);
            await _web.EnsureCoreWebView2Async(env);
            var core = _web.CoreWebView2;
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.IsZoomControlEnabled = false;
            core.Settings.IsStatusBarEnabled = false;
            var entry = Path.Combine(AppContext.BaseDirectory, "wallpaper", "index.html");
            core.NavigateToFileSystem(new Uri(entry).AbsoluteUri);

            // 参数实时下发：宿主把设置 JSON 推给页面
            var json = _settings.ToJson();
            core.AddScriptToExecuteOnDocumentCreatedAsync(
                "window.__WALLPAPER_SETTINGS__ = " + json + ";");
            core.NavigationCompleted += (_, _) =>
                core.ExecuteScriptAsync("window.__applyWallpaperSettings && window.__applyWallpaperSettings(" + _settings.ToJson() + ");");
        };
    }

    /// <summary>铺满指定屏幕并沉到桌面后层。</summary>
    public void AttachToDesktop()
    {
        var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        var hmon = Native.MonitorFromWindow(Handle, Native.MONITOR_DEFAULTTONEAREST);
        if (Native.GetMonitorInfo(hmon, ref mi))
            SetBounds(mi.rcMonitor.Left, mi.rcMonitor.Top,
                mi.rcMonitor.Right - mi.rcMonitor.Left, mi.rcMonitor.Bottom - mi.rcMonitor.Top);

        // 沉到 Progman/WorkerW 之后：找到 SHELLDLL_DefView 的父 WorkerW，把自己塞到它后面
        IntPtr worker = FindWorkerWBehindDefView();
        if (worker != IntPtr.Zero)
            Native.SetWindowPos(Handle, worker, 0, 0, 0, 0,
                Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
        else
            Native.SetWindowPos(Handle, (IntPtr)1 /*HWND_BOTTOM*/, 0, 0, 0, 0,
                Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
    }

    /// <summary>标准 Progman→WorkerW 手法：给 Progman 发 0x052C 让 explorer 生成 WorkerW 层。</summary>
    public static void SpawnWorkerW()
    {
        var progman = Native.FindWindow("Progman", null);
        if (progman == IntPtr.Zero) return;
        Native.SendMessageTimeout(progman, 0x052C, IntPtr.Zero, IntPtr.Zero, 0, 200, out _);
    }

    static IntPtr FindWorkerWBehindDefView()
    {
        IntPtr worker = IntPtr.Zero, shell = IntPtr.Zero;
        Native.EnumWindows((h, _) =>
        {
            var defView = Native.FindWindowEx(h, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (defView != IntPtr.Zero)
            {
                worker = Native.FindWindowEx(IntPtr.Zero, h, "WorkerW", null);
                shell = h;
                return false;
            }
            return true;
        }, IntPtr.Zero);
        return worker;
    }

    /// <summary>把参数实时推给壁纸页面（滑条拖动时调用）。</summary>
    public void ApplySettings(Settings s)
    {
        var core = _web?.CoreWebView2;
        if (core == null) return;
        core.ExecuteScriptAsync(
            "window.__applyWallpaperSettings && window.__applyWallpaperSettings(" + s.ToJson() + ");");
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_WINDOWPOSCHANGING = 0x0047;
        // 别让 explorer 的 z 序维护把我们带回去
        if (m.Msg == WM_WINDOWPOSCHANGING && Visible)
        {
            var wp = Marshal.PtrToStructure<WINDOWPOS>(m.LParam);
            wp.hwndInsertAfter = FindWorkerWBehindDefView();
            Marshal.StructureToPtr(wp, m.LParam, true);
        }
        base.WndProc(ref m);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WINDOWPOS { public IntPtr hwnd, hwndInsertAfter; public int x, y, cx, cy; public uint flags; }
}

public static class WebView2FileExt
{
    // WebView2 官方 API：允许 file:// 加载本地目录
    public static void NavigateToFileSystem(this Microsoft.Web.WebView2.Core.CoreWebView2 core, string url)
        => core.Navigate(url);
}

internal static class Native2 { }
