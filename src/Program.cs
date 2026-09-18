using System;
using System.IO;
using System.Windows.Forms;

namespace CloudTrainWallpaper;

static class Program
{
    [STAThread]
    static void Main()
    {
        // 单实例：壁纸进程只允许一个
        using var mutex = new Mutex(true, "CloudTrainWallpaper_SingleInstance", out var createdNew);
        if (!createdNew) return;

        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new WallpaperContext());
    }
}
