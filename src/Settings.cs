using System;
using System.IO;

namespace CloudTrainWallpaper;

/// <summary>壁纸参数。与页面 URL 参数一一对应，持久化到 %APPDATA%。</summary>
public sealed class Settings
{
    public double Rain { get; set; } = 0.6;
    public double Thunder { get; set; } = 0.4;
    public double Wind { get; set; } = 1.0;
    public double Speed { get; set; } = 0.26;
    public double Zoom { get; set; } = 0.71;
    public double Offset { get; set; } = -0.12;
    public double Amplitude { get; set; } = 0.6;
    public int Detail { get; set; } = 8;
    public double Exposure { get; set; } = 1.0;
    public double Saturation { get; set; } = 1.0;
    public int Hue { get; set; } = 0;
    public double Temperature { get; set; } = 0.0;
    public int Tod { get; set; } = 25;   // 25 = 自动跟随系统时间；0-24 = 固定时刻
    public bool AutoStart { get; set; } = false;

    public static string Dir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CloudTrainWallpaper");
    public static string FilePath => Path.Combine(Dir, "settings.json");
    public static string UserDataDir => Path.Combine(Dir, "webview");

    public string ToJson()
    {
        // 页面参数是小写 key；tod: 25=自动跟随系统时间
        bool todAuto = Tod >= 25;
        double tod = todAuto ? 0 : Tod;
        return "{\"rain\":" + Rain + ",\"thunder\":" + Thunder + ",\"wind\":" + Wind +
               ",\"speed\":" + Speed + ",\"zoom\":" + Zoom + ",\"offset\":" + Offset +
               ",\"amplitude\":" + Amplitude + ",\"detail\":" + Detail + ",\"exposure\":" + Exposure +
               ",\"saturation\":" + Saturation + ",\"hue\":" + Hue + ",\"temperature\":" + Temperature +
               ",\"tod\":" + tod + ",\"todAuto\":" + (todAuto ? "true" : "false") + "}";
    }

    public static Settings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var s = new Settings();
                var json = File.ReadAllText(FilePath);
                double D(string k, double dft)
                {
                    var m = System.Text.RegularExpressions.Regex.Match(json, "\"" + k + "\"\\s*:\\s*(-?[0-9.]+)");
                    return m.Success ? double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : dft;
                }
                bool B(string k, bool dft)
                {
                    var m = System.Text.RegularExpressions.Regex.Match(json, "\"" + k + "\"\\s*:\\s*(true|false)");
                    return m.Success ? m.Groups[1].Value == "true" : dft;
                }
                s.Rain = D("rain", s.Rain); s.Thunder = D("thunder", s.Thunder); s.Wind = D("wind", s.Wind);
                s.Speed = D("speed", s.Speed); s.Zoom = D("zoom", s.Zoom); s.Offset = D("offset", s.Offset);
                s.Amplitude = D("amplitude", s.Amplitude); s.Detail = (int)D("detail", s.Detail);
                s.Exposure = D("exposure", s.Exposure); s.Saturation = D("saturation", s.Saturation);
                s.Hue = (int)D("hue", s.Hue); s.Temperature = D("temperature", s.Temperature);
                s.Tod = (int)D("tod", s.Tod); s.AutoStart = B("autoStart", s.AutoStart);
                if (!File.ReadAllText(FilePath).Contains("\"tod\"") )
                    s.Tod = 25;
                return s;
            }
        }
        catch { }
        return new Settings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Dir);
        File.WriteAllText(FilePath, ToJsonPretty());
    }

    string ToJsonPretty()
    {
        bool todAuto = Tod >= 25;
        double tod = todAuto ? 0 : Tod;
        return "{\n  \"rain\": " + Rain + ",\n  \"thunder\": " + Thunder + ",\n  \"wind\": " + Wind +
               ",\n  \"speed\": " + Speed + ",\n  \"zoom\": " + Zoom + ",\n  \"offset\": " + Offset +
               ",\n  \"amplitude\": " + Amplitude + ",\n  \"detail\": " + Detail + ",\n  \"exposure\": " + Exposure +
               ",\n  \"saturation\": " + Saturation + ",\n  \"hue\": " + Hue + ",\n  \"temperature\": " + Temperature +
               ",\n  \"tod\": " + tod + ",\n  \"autoStart\": " + (AutoStart ? "true" : "false").ToLower() + "\n}";
    }
}

public static class SettingsData
{
    public static string UserDataDir => Settings.UserDataDir;
}
