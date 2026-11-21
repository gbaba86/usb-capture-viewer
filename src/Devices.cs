using DirectShowLib;
using System.Runtime.InteropServices;

namespace SwitchCaptureViewer;

internal sealed record CaptureDevice(string Id, string Name)
{
    public override string ToString() => Name;
}

internal static class Devices
{
    // First-run auto-connect requires the known physical USB identity, not just a display name.
    public static bool IsRecognizedCapture(CaptureDevice device) =>
        device.Name == "Live Gamer MINI" && device.Id.Contains("vid_07ca&pid_1311", StringComparison.OrdinalIgnoreCase);

    public static List<CaptureDevice> List(bool video)
    {
        var devices = DsDevice.GetDevicesOfCat(video ? FilterCategory.VideoInputDevice : FilterCategory.AudioInputDevice);
        try { return devices.Select(d => new CaptureDevice(d.DevicePath, d.Name)).ToList(); }
        finally { foreach (var d in devices) d.Dispose(); }
    }

    // This only inspects the selected driver's advertised stream types; the graph is never run.
    public static List<CaptureMode> ReadModes(string id)
    {
        var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        object? source = null, configObject = null;
        IFilterGraph2? graph = null;
        ICaptureGraphBuilder2? builder = null;
        IntPtr caps = IntPtr.Zero;
        try
        {
            var device = devices.FirstOrDefault(d => d.DevicePath == id) ?? throw new InvalidOperationException("ไม่พบอุปกรณ์วิดีโอที่เลือก");
            var filterId = typeof(IBaseFilter).GUID;
            device.Mon.BindToObject(null!, null, ref filterId, out source);
            var filter = (IBaseFilter)source;
            graph = (IFilterGraph2)new FilterGraph();
            builder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
            DsError.ThrowExceptionForHR(builder.SetFiltergraph(graph));
            DsError.ThrowExceptionForHR(graph.AddFilter(filter, "Selected capture device"));
            var hr = builder.FindInterface(PinCategory.Capture, MediaType.Video, filter, typeof(IAMStreamConfig).GUID, out configObject);
            if (hr < 0) hr = builder.FindInterface(PinCategory.Preview, MediaType.Video, filter, typeof(IAMStreamConfig).GUID, out configObject);
            DsError.ThrowExceptionForHR(hr);
            var config = (IAMStreamConfig)configObject;
            DsError.ThrowExceptionForHR(config.GetNumberOfCapabilities(out int count, out int size));
            if (size <= 0 || size > 1024 * 1024 || count < 0 || count > 10000) throw new InvalidOperationException("ข้อมูลโหมดจากไดรเวอร์ไม่ถูกต้อง");
            caps = Marshal.AllocCoTaskMem(size);
            var modes = new List<CaptureMode>();
            for (int i = 0; i < count; i++)
            {
                AMMediaType? media = null;
                try
                {
                    if (config.GetStreamCaps(i, out media, caps) < 0 || media is null) continue;
                    int width, height; long frameTime;
                    if (media.formatType == FormatType.VideoInfo && media.formatSize >= Marshal.SizeOf<VideoInfoHeader>())
                    {
                        var header = Marshal.PtrToStructure<VideoInfoHeader>(media.formatPtr)!;
                        width = header.BmiHeader.Width; height = Math.Abs(header.BmiHeader.Height); frameTime = header.AvgTimePerFrame;
                    }
                    else if (media.formatType == FormatType.VideoInfo2 && media.formatSize >= Marshal.SizeOf<VideoInfoHeader2>())
                    {
                        var header = Marshal.PtrToStructure<VideoInfoHeader2>(media.formatPtr)!;
                        width = header.BmiHeader.Width; height = Math.Abs(header.BmiHeader.Height); frameTime = header.AvgTimePerFrame;
                    }
                    else continue;
                    var chroma = Chroma(media.subType);
                    // This app deliberately offers common SDR modes, up to 1080p60.
                    if (chroma is null || width <= 0 || height <= 0 || width > 1920 || height > 1080 || frameTime <= 0) continue;
                    double fps = Math.Round(10_000_000d / frameTime, 3);
                    if (fps > 60.01) continue;
                    modes.Add(new(width, height, fps, chroma));
                    // Some drivers describe a frame-rate range rather than a separate type per rate.
                    // Only endpoints are added; arbitrary rates inside the range are not invented.
                    if (size == Marshal.SizeOf<VideoStreamConfigCaps>())
                    {
                        var range = Marshal.PtrToStructure<VideoStreamConfigCaps>(caps)!;
                        foreach (long interval in new[] { range.MinFrameInterval, range.MaxFrameInterval })
                        {
                            if (interval <= 0) continue;
                            double endpoint = Math.Round(10_000_000d / interval, 3);
                            if (endpoint >= 1 && endpoint <= 60.01) modes.Add(new(width, height, endpoint, chroma));
                        }
                    }
                }
                finally { if (media is not null) DsUtils.FreeAMMediaType(media); }
            }
            return modes.Distinct().OrderByDescending(m => m.Width * m.Height).ThenByDescending(m => m.Fps).ThenBy(m => m.Chroma).ToList();
        }
        finally
        {
            if (caps != IntPtr.Zero) Marshal.FreeCoTaskMem(caps);
            Release(configObject); Release(builder); Release(graph); Release(source);
            foreach (var d in devices) d.Dispose();
        }
    }

    static void Release(object? obj) { if (obj is not null && Marshal.IsComObject(obj)) Marshal.ReleaseComObject(obj); }
    static string? Chroma(Guid subtype)
    {
        if (subtype == MediaSubType.RGB24) return "RV24";
        if (subtype == MediaSubType.RGB32) return "RV32";
        byte[] b = subtype.ToByteArray();
        string fourcc = System.Text.Encoding.ASCII.GetString(b, 0, 4);
        return fourcc is "MJPG" or "H264" or "YUY2" or "UYVY" or "NV12" or "I420" or "YV12" ? fourcc : null;
    }

    public static Task<List<CaptureMode>> ReadModesAsync(string id)
    {
        var completion = new TaskCompletionSource<List<CaptureMode>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try { completion.SetResult(ReadModes(id)); }
            catch (Exception e) { completion.SetException(e); }
        }) { IsBackground = true, Name = "Capture capability reader" };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }
}
