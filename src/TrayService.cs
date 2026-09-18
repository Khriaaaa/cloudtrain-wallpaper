using System;
using System.Drawing;
using System.Windows.Forms;

namespace CloudTrainWallpaper;

/// <summary>托盘：双击/右键打开参数面板，退出菜单。</summary>
class TrayService : IDisposable
{
    readonly NotifyIcon _icon;
    readonly Settings _settings;
    readonly WallpaperForm _wallpaper;
    readonly Action _exit;

    public TrayService(Settings settings, WallpaperForm wallpaper, Action exit)
    {
        _settings = settings; _wallpaper = wallpaper; _exit = exit;
        _icon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "云间列车壁纸 (右键调参数)",
            Visible = true
        };

        var menu = new ContextMenuStrip();
        var miPanel = new ToolStripMenuItem("参数设置…");
        miPanel.Font = new Font(miPanel.Font, FontStyle.Bold);
        var miAttach = new ToolStripMenuItem("重新贴合桌面");
        var miExit = new ToolStripMenuItem("退出壁纸");
        menu.Items.AddRange(new ToolStripItem[] { miPanel, miAttach, new ToolStripSeparator(), miExit });

        miPanel.Click += (_, _) => SettingsPanel.ShowOnce(_settings, _wallpaper);
        miAttach.Click += (_, _) => { WallpaperForm.SpawnWorkerW(); _wallpaper.AttachToDesktop(); };
        miExit.Click += (_, _) => _exit();
        _icon.ContextMenuStrip = menu;
        _icon.DoubleClick += (_, _) => SettingsPanel.ShowOnce(_settings, _wallpaper);
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
