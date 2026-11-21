using System.Text.Json;

namespace SwitchCaptureViewer;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var test = args.Contains("--self-test");
        string? clip = Value(args, "--test-clip");
        var root = Value(args, "--data-dir") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SwitchCaptureViewer");
        var store = new SettingsStore(root);
        Application.ThreadException += (_, e) => ShowError(store, e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => store.Log(e.ExceptionObject.ToString() ?? "Unhandled error");
        try
        {
            string? inspect = Value(args, "--inspect-device");
            if (inspect is not null)
            {
                var device = Devices.List(true).Single(d => d.Name == inspect);
                var modes = Devices.ReadModes(device.Id);
                Directory.CreateDirectory(root);
                File.WriteAllText(Path.Combine(root, "capabilities.json"), JsonSerializer.Serialize(new { Device = device, Video = Devices.List(true), Audio = Devices.List(false), Modes = modes }, new JsonSerializerOptions { WriteIndented = true }));
                return;
            }
            using var form = new MainForm(store, clip, test);
            Application.Run(form);
        }
        catch (Exception e) { ShowError(store, e); Environment.ExitCode = 1; }
    }

    static string? Value(string[] args, string key)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    static void ShowError(SettingsStore store, Exception e)
    {
        store.Log(e.ToString());
        MessageBox.Show("เปิดโปรแกรมไม่สำเร็จ\nกรุณาเก็บไฟล์ทั้งโฟลเดอร์ไว้ด้วยกัน แล้วลองเปิดใหม่\n\n" + e.Message + "\n\nบันทึกข้อผิดพลาด: " + store.LogPath,
            "Switch Capture Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

internal sealed record CaptureMode(int Width, int Height, double Fps, string Chroma)
{
    public string Resolution => $"{Width} × {Height}";
    public string Rate => Fps.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
    public override string ToString() => $"{Resolution} · {Rate} FPS · {Chroma}";
}

internal sealed class AppSettings
{
    public string? VideoId { get; set; }
    public string? AudioId { get; set; }
    public CaptureMode? Mode { get; set; }
    public int CacheMs { get; set; } = 50;
    public int Volume { get; set; } = 70;
    public bool Muted { get; set; }
    public bool MenuCollapsed { get; set; }
    public bool AudioConfigured { get; set; }
}

internal sealed class SettingsStore(string root)
{
    public string Root { get; } = root;
    public string LogPath => Path.Combine(Root, "viewer.log");
    public string SettingsPath => Path.Combine(Root, "settings.json");
    public AppSettings Load()
    {
        try
        {
            var result = File.Exists(SettingsPath) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath)) ?? new() : new();
            result.Volume = Math.Clamp(result.Volume, 0, 100);
            if (result.CacheMs is not (50 or 100 or 200)) result.CacheMs = 50;
            return result;
        }
        catch (Exception e) { Log("Settings reset: " + e.Message); return new(); }
    }
    public bool Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Root);
            string temp = SettingsPath + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temp, SettingsPath, true);
            return true;
        }
        catch (Exception e) { Log("Save settings: " + e.Message); return false; }
    }
    public void Log(string message)
    {
        try
        {
            lock (this)
            {
                Directory.CreateDirectory(Root);
                if (File.Exists(LogPath) && new FileInfo(LogPath).Length > 1_000_000) File.Move(LogPath, LogPath + ".old", true);
                File.AppendAllText(LogPath, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
            }
        }
        catch { /* Logging must not crash the viewer. */ }
    }
}
