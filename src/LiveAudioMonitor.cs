using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Text.RegularExpressions;

namespace SwitchCaptureViewer;

// Audio uses the capture endpoint's shared WASAPI stream instead of DirectShow's
// ~100 ms audio blocks. This keeps the video clock independent of audio buffering.
internal sealed class LiveAudioMonitor : IDisposable
{
    readonly MMDevice device;
    readonly WasapiCapture capture;
    readonly WasapiOut output;
    readonly BufferedWaveProvider buffer;
    readonly VolumeSampleProvider level;
    readonly CountingAudioSource source;
    readonly ManualResetEventSlim firstData = new(false);
    readonly object gate = new();
    bool disposed;
    public double Peak { get; private set; }
    public long ReceivedBytes { get; private set; }
    public long UnderrunBytes => source.UnderrunBytes;
    public long PlayedBytes => source.PlayedBytes;
    public long DiscardedBytes { get; private set; }
    public event Action<string>? Failed;

    public LiveAudioMonitor(CaptureDevice selected)
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        var waveGuid = Regex.Match(selected.Id, @"wave:(\{[^}]+\})", RegexOptions.IgnoreCase).Groups[1].Value;
        var matches = devices.Where(d => waveGuid.Length > 0 && d.ID.Contains(waveGuid, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count == 0) matches = devices.Where(d => string.Equals(d.FriendlyName, selected.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count != 1)
        {
            foreach (var item in devices) item.Dispose();
            throw new InvalidOperationException("ระบุอุปกรณ์เสียง WASAPI ไม่ได้แน่ชัด — เลือกอุปกรณ์เสียงใหม่");
        }
        device = matches[0];
        foreach (var item in devices) if (item != device) item.Dispose();
        capture = new WasapiCapture(device, false, 20);
        buffer = new BufferedWaveProvider(capture.WaveFormat)
        {
            BufferDuration = TimeSpan.FromMilliseconds(80), ReadFully = false, DiscardOnBufferOverflow = true
        };
        source = new CountingAudioSource(buffer);
        level = new VolumeSampleProvider(source.ToSampleProvider());
        output = new WasapiOut(AudioClientShareMode.Shared, true, 20);
        try { output.Init(level); }
        catch { output.Dispose(); capture.Dispose(); device.Dispose(); throw; }
        capture.DataAvailable += (_, e) =>
        {
            lock (gate)
            {
                if (disposed) return;
                // If the consumer falls behind, discard old audio, never build a long queue.
                if (buffer.BufferedDuration.TotalMilliseconds > 40) { DiscardedBytes += buffer.BufferedBytes; buffer.ClearBuffer(); }
                buffer.AddSamples(e.Buffer, 0, e.BytesRecorded); ReceivedBytes += e.BytesRecorded;
                firstData.Set();
                if (capture.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat ||
                    capture.WaveFormat is WaveFormatExtensible ext && ext.SubFormat == new Guid("00000003-0000-0010-8000-00aa00389b71"))
                {
                    double peak = 0;
                    for (int i = 0; i + 3 < e.BytesRecorded; i += 4) peak = Math.Max(peak, Math.Abs(BitConverter.ToSingle(e.Buffer, i)));
                    Peak = peak;
                }
            }
        };
        capture.RecordingStopped += (_, e) => { if (!disposed && e.Exception is not null) Failed?.Invoke(e.Exception.Message); };
        output.PlaybackStopped += (_, e) => { if (!disposed && e.Exception is not null) Failed?.Invoke(e.Exception.Message); };
    }
    public void SetVolume(int volume, bool muted) => level.Volume = muted ? 0f : Math.Clamp(volume / 100f, 0, 1);
    public void Start()
    {
        capture.StartRecording();
        if (!firstData.Wait(TimeSpan.FromSeconds(1))) throw new InvalidOperationException("อุปกรณ์เสียงยังไม่ส่งข้อมูล");
        output.Play();
    }
    public void Dispose()
    {
        lock (gate) { if (disposed) return; disposed = true; }
        capture.StopRecording(); output.Stop(); capture.Dispose(); output.Dispose(); device.Dispose(); firstData.Dispose();
    }
}

internal sealed class CountingAudioSource(BufferedWaveProvider buffer) : IWaveProvider
{
    public WaveFormat WaveFormat => buffer.WaveFormat;
    public long UnderrunBytes;
    public long PlayedBytes;
    public int Read(byte[] data, int offset, int count)
    {
        int read = buffer.Read(data, offset, count);
        Interlocked.Add(ref PlayedBytes, read);
        if (read < count) { Array.Clear(data, offset + read, count - read); Interlocked.Add(ref UnderrunBytes, count - read); }
        return count;
    }
}
