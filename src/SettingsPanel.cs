using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CloudTrainWallpaper;

/// <summary>参数面板：与网页滑条一一对应，改动即时下发到壁纸页面并持久化。</summary>
class SettingsPanel : Form
{
    static SettingsPanel? _instance;

    readonly Settings _settings;
    readonly WallpaperForm _wallpaper;
    readonly Dictionary<string, TrackBar> _bars = new();
    readonly Dictionary<string, System.Windows.Forms.Label> _vals = new();
    bool _updating;

    // (key, 标签, min, max, step, 是否整数)
    static readonly (string, string, double, double, double, bool)[] Defs =
    {
        ("rain",       "降雨强度", 0, 1,   0.01, false),
        ("thunder",    "打雷强度", 0, 1,   0.01, false),
        ("wind",       "风向强度", -2, 2, 0.05, false),
        ("speed",      "行进速度", 0, 5,   0.01, false),
        ("zoom",       "视角缩放", 0.5, 2, 0.01, false),
        ("offset",     "垂直位置", -0.5, 0.5, 0.01, false),
        ("amplitude",  "云层起伏", 0, 2,   0.01, false),
        ("detail",     "噪声细节", 1, 8,   1,    true),
        ("exposure",   "曝光亮度", 0.2, 2, 0.01, false),
        ("saturation", "色彩饱和度", 0, 2, 0.01, false),
        ("hue",        "整体色相", -180, 180, 1, true),
        ("temperature","冷暖色温", -1, 1,  0.01, false),
        ("tod",        "时刻(25=自动)", 0, 25, 1, true),
    };

    SettingsPanel(Settings s, WallpaperForm w)
    {
        _settings = s; _wallpaper = w;
        Text = "云间列车壁纸 — 参数";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new System.Drawing.Size(360, Defs.Length * 46 + 70);
        FormClosing += (_, e) => { _instance = null; e.Cancel = true; Hide(); };

        for (int i = 0; i < Defs.Length; i++)
        {
            var (key, label, min, max, step, isInt) = Defs[i];
            var lbl = new System.Windows.Forms.Label { Text = label, Location = new System.Drawing.Point(12, 14 + i * 46), AutoSize = true };
            var val = new System.Windows.Forms.Label { Location = new System.Drawing.Point(296, 14 + i * 46), AutoSize = true };
            var bar = new TrackBar
            {
                Minimum = ToInt(min, step, isInt),
                Maximum = ToInt(max, step, isInt),
                TickFrequency = 1,
                SmallChange = 1,
                LargeChange = 5,
                Location = new System.Drawing.Point(90, 10 + i * 46),
                Width = 200
            };
            bar.Value = ClampInt(GetVal(key), min, max, step, isInt);
            val.Text = FormatVal(key, bar.Value, step, isInt);
            bar.ValueChanged += (_, _) =>
            {
                if (_updating) return;
                SetVal(key, bar.Value, step, isInt);
                val.Text = FormatVal(key, bar.Value, step, isInt);
                PushToPage();
            };
            Controls.Add(lbl); Controls.Add(bar); Controls.Add(val);
            _bars[key] = bar; _vals[key] = val;
        }

        var btnSave = new Button { Text = "保存设置", Location = new System.Drawing.Point(130, Defs.Length * 46 + 22), Width = 100 };
        btnSave.Click += (_, _) => _settings.Save();
        Controls.Add(btnSave);
        var tip = new System.Windows.Forms.Label { Text = "改动即时生效，点「保存设置」才会记住", Location = new System.Drawing.Point(12, Defs.Length * 46 + 27), AutoSize = true };
        Controls.Add(tip);
    }

    public static void ShowOnce(Settings s, WallpaperForm w)
    {
        if (_instance == null || _instance.IsDisposed) _instance = new SettingsPanel(s, w);
        _instance.Show();
        _instance.BringToFront();
        _instance.Activate();
    }

    double GetVal(string key) => key switch
    {
        "rain" => _settings.Rain, "thunder" => _settings.Thunder, "wind" => _settings.Wind,
        "speed" => _settings.Speed, "zoom" => _settings.Zoom, "offset" => _settings.Offset,
        "amplitude" => _settings.Amplitude, "detail" => _settings.Detail, "exposure" => _settings.Exposure,
        "saturation" => _settings.Saturation, "hue" => _settings.Hue,
        "temperature" => _settings.Temperature, "tod" => _settings.Tod,
        _ => 0
    };

    void SetVal(string key, int raw, double step, bool isInt)
    {
        double v = raw * step;
        switch (key)
        {
            case "rain": _settings.Rain = v; break;
            case "thunder": _settings.Thunder = v; break;
            case "wind": _settings.Wind = v; break;
            case "speed": _settings.Speed = v; break;
            case "zoom": _settings.Zoom = v; break;
            case "offset": _settings.Offset = v; break;
            case "amplitude": _settings.Amplitude = v; break;
            case "detail": _settings.Detail = (int)v; break;
            case "exposure": _settings.Exposure = v; break;
            case "saturation": _settings.Saturation = v; break;
            case "hue": _settings.Hue = (int)v; break;
            case "temperature": _settings.Temperature = v; break;
            case "tod": _settings.Tod = (int)v; break;
        }
    }

    static int ToInt(double v, double step, bool isInt) => (int)Math.Round(v / step);
    static int ClampInt(double v, double min, double max, double step, bool isInt)
    {
        int x = (int)Math.Round(v / step);
        int lo = (int)Math.Round(min / step), hi = (int)Math.Round(max / step);
        return Math.Clamp(x, lo, hi);
    }
    string FormatVal(string key, int raw, double step, bool isInt)
    {
        double v = raw * step;
        return isInt ? ((int)v).ToString() : v.ToString("0.##");
    }

    void PushToPage()
    {
        _wallpaper.ApplySettings(_settings);
    }
}
