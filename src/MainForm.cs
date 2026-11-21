using LibVLCSharp.Shared;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace SwitchCaptureViewer;

internal sealed class MainForm : Form
{
    readonly SettingsStore store;
    readonly AppSettings settings;
    readonly string? testClip;
    readonly bool selfTest;
    LibVLC? vlc;
    Task<LibVLC>? initialization;
    readonly ComboBox video = Theme.Combo(), audio = Theme.Combo();
    readonly Button start = Theme.Button("เริ่ม", true), stop = Theme.Button("หยุด"), refresh = Theme.Button("ค้นหาอุปกรณ์ใหม่"), configure = Theme.Button("ตั้งค่าภาพ / เสียง"), full = Theme.Button("เต็มจอ  F11");
    readonly Button menu = Theme.Button("ซ่อนเมนู  Tab");
    readonly CheckBox mute = new() { Text = "ปิดเสียง  M", AutoSize = true, ForeColor = Theme.Text, Margin = new Padding(0, 12, 0, 4) };
    readonly TrackBar volume = new() { Minimum = 0, Maximum = 100, TickFrequency = 10, Dock = DockStyle.Top, Height = 42, SmallChange = 5, LargeChange = 10 };
    readonly Label status = Theme.Label("พร้อม — เลือกการ์ดแล้วกดเริ่ม"), modeInfo = Theme.Label("Auto"), volumeLabel = Theme.Label("ระดับเสียง"), detail = Theme.Label("ยังไม่รับภาพ");
    readonly Panel videoHost = new() { BackColor = Color.Black, Dock = DockStyle.Fill, TabStop = true };
    readonly Panel header = new() { Dock = DockStyle.Top, Height = 76, Padding = new Padding(24, 12, 24, 8) };
    readonly Panel sidebar = new() { Dock = DockStyle.Left, Width = 325, Padding = new Padding(22, 8, 22, 12), BackColor = Theme.Surface };
    readonly Panel footer = new() { Dock = DockStyle.Bottom, Height = 61, Padding = new Padding(20, 8, 20, 6) };
    readonly Label placeholder = new() { Text = "กำลังรอสัญญาณจากการ์ดแคปเจอร์\n\nเครื่องเกม → HDMI IN ของการ์ด → USB ของคอม\nโปรแกรมจะเชื่อมต่อการ์ดที่จำไว้โดยอัตโนมัติ\n\nF11 เต็มจอ  •  Esc กลับหน้าต่าง  •  Tab พับเมนู", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, ForeColor = Theme.Subtle, BackColor = Color.Black, Font = new Font("Leelawadee UI", 13) };
    readonly SemaphoreSlim operation = new(1, 1);
    readonly System.Windows.Forms.Timer monitor = new() { Interval = 1000 };
    readonly System.Windows.Forms.Timer reconnect = new() { Interval = 3000 };
    MediaPlayer? player;
    Media? media;
    LiveAudioMonitor? liveAudio;
    string? audioWarning;
    bool active, transitioning, closing, closeReady, fullscreen, populating, configuring, retryBusy;
    DateTime retryAt = DateTime.MinValue;
    int failures;
    Rectangle savedBounds;
    FormWindowState savedWindowState;
    DateTime startedAt;
    bool decoded;
    int lastDecoded;
    int stalledSeconds;
    bool statsLogged;
    DateTime nextDiagnosticAt;
    List<CaptureDevice> videoDevices = [], audioDevices = [];

    public MainForm(SettingsStore store, string? testClip, bool selfTest)
    {
        this.store = store; this.testClip = testClip; this.selfTest = selfTest; settings = store.Load();
        Text = "USB Capture Viewer"; BackColor = Theme.Background; ForeColor = Theme.Text;
        using (var iconStream = typeof(MainForm).Assembly.GetManifestResourceStream("SwitchCaptureViewer.AppIcon")!)
        using (var appIcon = new Icon(iconStream)) Icon = (Icon)appIcon.Clone();
        Font = new Font("Leelawadee UI", 10); Size = new Size(1280, 820); MinimumSize = new Size(1050, 720);
        StartPosition = FormStartPosition.CenterScreen; AutoScaleMode = AutoScaleMode.Dpi; KeyPreview = true;
        var title = Theme.Label("USB CAPTURE", 19, true); title.Location = new Point(24, 8);
        var subtitle = Theme.Label("ดูภาพและฟังเสียงจากการ์ด USB", 10); subtitle.ForeColor = Theme.Subtle; subtitle.Location = new Point(24, 43);
        header.Controls.Add(title); header.Controls.Add(subtitle);
        menu.Size = new Size(165, 42); header.Controls.Add(menu); menu.BringToFront();
        header.Resize += (_, _) => menu.Location = new Point(header.ClientSize.Width - 24 - menu.Width, 14);
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, AutoScroll = true, Padding = new Padding(0, 2, 0, 0) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); sidebar.Controls.Add(fields);
        fields.Controls.Add(Theme.Label("อุปกรณ์วิดีโอ", 10, true)); fields.Controls.Add(video);
        fields.Controls.Add(Theme.Label("อุปกรณ์เสียงเข้า", 10, true)); fields.Controls.Add(audio);
        refresh.Text = "เชื่อมต่อใหม่"; refresh.Dock = DockStyle.Top; configure.Dock = DockStyle.Top;
        fields.Controls.Add(refresh); fields.Controls.Add(configure);
        modeInfo.MaximumSize = new Size(280, 0); modeInfo.ForeColor = Theme.Subtle; fields.Controls.Add(modeInfo);
        var transport = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 14, 0, 8) };
        transport.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); transport.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        start.Dock = DockStyle.Fill; stop.Dock = DockStyle.Fill; transport.Controls.Add(start); transport.Controls.Add(stop);
        // Start/stop remain internal lifecycle controls for tests. Normal use auto-connects.
        fields.Controls.Add(volumeLabel); fields.Controls.Add(volume); fields.Controls.Add(mute);
        full.Dock = DockStyle.Top; fields.Controls.Add(full);
        var audioHint = Theme.Label("เสียงออกลำโพง/หูฟังเริ่มต้นของ Windows\nเลือกเสียงจากการ์ด ไม่ใช่ไมโครโฟน\n\nเปิดแอปเพื่อเชื่อมต่ออัตโนมัติ\nปิดแอปเพื่อคืนอุปกรณ์\nการ์ดอื่น: เลือกภาพ/เสียงครั้งแรกแล้วจำค่า");
        audioHint.MaximumSize = new Size(280, 0); audioHint.ForeColor = Theme.Subtle; fields.Controls.Add(audioHint);
        status.Dock = DockStyle.Top; status.AutoSize = false; status.Height = 27;
        detail.Dock = DockStyle.Bottom; detail.ForeColor = Theme.Subtle; detail.AutoSize = false; detail.Height = 21;
        footer.Controls.Add(status); footer.Controls.Add(detail);
        videoHost.Controls.Add(placeholder);
        Controls.Add(videoHost); Controls.Add(sidebar); Controls.Add(footer); Controls.Add(header);
        video.AccessibleName = "อุปกรณ์วิดีโอ"; audio.AccessibleName = "อุปกรณ์เสียงเข้า"; volume.AccessibleName = "ระดับเสียง";
        volume.Value = settings.Volume; mute.Checked = settings.Muted;
        UpdateVolumeText(); UpdateModeText();
        start.Click += async (_, _) => await StartAsync();
        stop.Click += async (_, _) => await StopAsync("หยุดแล้ว — อุปกรณ์พร้อมให้โปรแกรมอื่นใช้งาน");
        refresh.Click += async (_, _) =>
        {
            configuring = true;
            try { await StopAsync("กำลังเชื่อมต่อใหม่..."); RefreshDevices(); await StartAsync(); }
            finally { configuring = false; UpdateControls(); }
        };
        configure.Click += async (_, _) =>
        {
            configuring = true;
            await StopAsync("พักการรับภาพเพื่อปรับตั้งค่า...");
            using var dialog = new CaptureSettingsForm(SelectedVideo, settings);
            if (dialog.ShowDialog(this) == DialogResult.OK) { settings.Mode = dialog.SelectedMode; settings.CacheMs = dialog.CacheMs; SaveSettings(); UpdateModeText(); }
            configuring = false; await StartAsync();
        };
        full.Click += (_, _) => ToggleFullscreen();
        menu.Click += (_, _) => ToggleMenu();
        volume.ValueChanged += (_, _) => { settings.Volume = volume.Value; ApplyVolume(); SaveSettings(); UpdateVolumeText(); };
        mute.CheckedChanged += (_, _) => { settings.Muted = mute.Checked; ApplyVolume(); SaveSettings(); };
        video.SelectedIndexChanged += async (_, _) =>
        {
            if (populating) return;
            settings.VideoId = SelectedVideo?.Id; settings.Mode = null; retryAt = DateTime.MinValue;
            UpdateModeText(); SaveSettings(); UpdateControls();
            if (active) await StopAsync("กำลังเปลี่ยนอุปกรณ์วิดีโอ...");
            retryAt = DateTime.UtcNow.AddSeconds(3);
        };
        audio.SelectedIndexChanged += async (_, _) =>
        {
            if (populating) return;
            settings.AudioId = SelectedAudio?.Id; settings.AudioConfigured = true; SaveSettings();
            if (active) await StopAsync("กำลังเปลี่ยนอุปกรณ์เสียง...");
            retryAt = DateTime.UtcNow.AddSeconds(3);
        };
        monitor.Tick += async (_, _) => await CheckFramesAsync();
        reconnect.Tick += async (_, _) => await AutoConnectAsync();
        FormClosing += OnClosing;
        FormClosed += (_, _) => { monitor.Dispose(); reconnect.Dispose(); vlc?.Dispose(); operation.Dispose(); Icon?.Dispose(); };
        Shown += async (_, _) =>
        {
            RefreshDevices();
            if (settings.MenuCollapsed) { sidebar.Visible = false; menu.Text = "แสดงเมนู  Tab"; }
            status.Text = "กำลังเตรียมเอนจินวิดีโอครั้งแรก กรุณารอสักครู่...";
            initialization = Task.Run(() =>
            {
                Core.Initialize(Path.Combine(AppContext.BaseDirectory, "libvlc", "win-x64"));
                var engine = new LibVLC("--no-video-title-show", "--no-osd", "--no-snapshot-preview", "--drop-late-frames", "--no-video-deco", "--quiet");
                engine.Log += (_, e) => { if (e.Level == LogLevel.Error) store.Log($"VLC [{e.Module}] {e.Message}"); };
                return engine;
            });
            try { vlc = await initialization; }
            catch (Exception e) { store.Log(e.ToString()); status.Text = "เตรียมเอนจินไม่สำเร็จ — กรุณาเก็บไฟล์ทั้งโฟลเดอร์ไว้ด้วยกัน"; return; }
            if (closing) return;
            status.Text = "พร้อม — กดเริ่มเพื่อรับภาพจากอุปกรณ์ที่เลือก"; UpdateControls();
            if (!selfTest && settings.VideoId is null && SelectedVideo is { } initialVideo && Devices.IsRecognizedCapture(initialVideo))
            {
                var id = SelectedVideo.Id;
                transitioning = true; UpdateControls(); status.Text = "กำลังอ่านโหมดเริ่มต้นของ Live Gamer MINI...";
                try
                {
                    var modes = await Devices.ReadModesAsync(id).WaitAsync(TimeSpan.FromSeconds(10));
                    settings.Mode = modes.FirstOrDefault(m => m.Width == 1920 && m.Height == 1080 && m.Fps >= 59 && m.Fps <= 60.01 && m.Chroma == "MJPG");
                    settings.VideoId = id; settings.AudioId = SelectedAudio?.Id; settings.AudioConfigured = true; SaveSettings(); UpdateModeText();
                    status.Text = "กำลังเชื่อมต่อ Live Gamer MINI...";
                }
                catch (Exception e) { store.Log("Initial mode query: " + e.Message); status.Text = "อ่านโหมดไม่ได้ — พร้อมเริ่มด้วย Auto"; }
                finally { transitioning = false; UpdateControls(); }
            }
            if (selfTest) await RunSelfTestAsync();
            else { reconnect.Start(); await AutoConnectAsync(); }
        };
    }

    CaptureDevice? SelectedVideo => video.SelectedItem as CaptureDevice;
    CaptureDevice? SelectedAudio => audio.SelectedItem as CaptureDevice;

    void RefreshDevices()
    {
        if (active || transitioning) return;
        populating = true;
        try
        {
            videoDevices = Devices.List(true); audioDevices = Devices.List(false);
            video.Items.Clear(); audio.Items.Clear();
            video.Items.Add("— เลือกอุปกรณ์วิดีโอ —"); audio.Items.Add("ไม่รับเสียง");
            video.Items.AddRange(videoDevices.Cast<object>().ToArray()); audio.Items.AddRange(audioDevices.Cast<object>().ToArray());
            var v = videoDevices.FirstOrDefault(d => d.Id == settings.VideoId);
            var a = audioDevices.FirstOrDefault(d => d.Id == settings.AudioId);
            // Only the user's identified target may be suggested. Never choose a webcam/microphone by position.
            if (settings.VideoId is null) v = videoDevices.FirstOrDefault(Devices.IsRecognizedCapture);
            if (!settings.AudioConfigured && settings.AudioId is null && v is not null && Devices.IsRecognizedCapture(v)) a = audioDevices.FirstOrDefault(d => d.Name == "Digital Audio Interface (Live Gamer MINI)");
            video.SelectedIndex = v is null ? 0 : videoDevices.IndexOf(v) + 1;
            audio.SelectedIndex = a is null ? 0 : audioDevices.IndexOf(a) + 1;
            status.Text = v is null ? "ยังไม่พบการ์ดที่จำไว้ — เสียบ USB แล้วโปรแกรมจะเชื่อมต่อเอง" : "กำลังเชื่อมต่ออุปกรณ์ที่เลือก...";
            if (settings.AudioId is not null && a is null) status.Text = "อุปกรณ์เสียงที่บันทึกไว้ไม่เชื่อมต่อ — เลือกเสียงใหม่ก่อนเริ่ม";
            detail.Text = "เชื่อมต่ออัตโนมัติเฉพาะการ์ดที่จำไว้ หรือ Live Gamer MINI • F11 เต็มจอ • Tab พับเมนู";
        }
        catch (Exception e) { status.Text = "ค้นหาอุปกรณ์ไม่สำเร็จ: " + e.Message; store.Log(e.ToString()); }
        finally { populating = false; UpdateControls(); }
    }

    async Task StartAsync()
    {
        if (active || transitioning || closing || vlc is null) return;
        if (testClip is null && SelectedVideo is null) { status.Text = "กรุณาเลือกอุปกรณ์วิดีโอก่อน"; return; }
        if (testClip is null && settings.AudioId is not null && SelectedAudio is null) { status.Text = "ไม่พบเสียงที่บันทึกไว้ — เลือกอุปกรณ์เสียงหรือเลือกไม่รับเสียง"; return; }
        await operation.WaitAsync();
        try
        {
            if (active || closing) return;
            transitioning = true; UpdateControls();
            status.Text = "กำลังเปิดอุปกรณ์...";
            if (testClip is null)
            {
                // Re-enumeration checks that the saved device still exists without opening unrelated devices.
                if (!Devices.List(true).Any(d => d.Id == SelectedVideo!.Id)) throw new InvalidOperationException("อุปกรณ์วิดีโอถูกถอด — ค้นหาอุปกรณ์ใหม่");
                if (SelectedAudio is not null && !Devices.List(false).Any(d => d.Id == SelectedAudio.Id)) throw new InvalidOperationException("อุปกรณ์เสียงถูกถอด — ค้นหาอุปกรณ์ใหม่");
                settings.VideoId = SelectedVideo!.Id; settings.AudioId = SelectedAudio?.Id; settings.AudioConfigured = true; SaveSettings();
            }
            var next = new MediaPlayer(vlc) { Hwnd = videoHost.Handle, EnableMouseInput = false, EnableKeyInput = false };
            player = next;
            next.EncounteredError += (_, _) => OnUi(async () =>
            {
                if (player == next && !closing) await StopAsync("เปิดรับสัญญาณไม่สำเร็จ — ตรวจสาย, โหมดภาพ และโปรแกรมที่ใช้อุปกรณ์อยู่");
            });
            next.EndReached += (_, _) => OnUi(async () => { if (player == next && !closing) await StopAsync("สัญญาณสิ้นสุด — ตรวจสายและกดเริ่มใหม่"); });
            media = testClip is null ? new Media(vlc, "dshow://", FromType.FromLocation) : new Media(vlc, testClip, FromType.FromPath);
            if (testClip is null)
            {
                media.AddOption(":dshow-vdev=" + SelectedVideo!.Name);
                media.AddOption(":dshow-adev=none");
                media.AddOption(":no-dshow-config"); media.AddOption(":no-dshow-tuner");
                media.AddOption(":live-caching=" + settings.CacheMs);
                media.AddOption(":dshow-aspect-ratio=16:9");
                media.AddOption(":clock-jitter=0");
                // Avoid frame-thread decoder queues for the intra-frame MJPEG capture mode.
                media.AddOption(":avcodec-threads=1");
                if (settings.Mode is { } mode)
                {
                    media.AddOption($":dshow-size={mode.Width}x{mode.Height}");
                    media.AddOption(":dshow-fps=" + mode.Fps.ToString("0.###", CultureInfo.InvariantCulture));
                    media.AddOption(":dshow-chroma=" + mode.Chroma);
                }
            }
            else media.AddOption(":file-caching=50");
            // Apply before Play and again after the audio output has initialized.
            ApplyVolume();
            next.Playing += (_, _) => OnUi(() => { if (player == next) ApplyVolume(); });
            bool result = await Task.Run(() => next.Play(media));
            if (!result) throw new InvalidOperationException("ตัวเล่นไม่สามารถเริ่มรับสัญญาณได้");
            audioWarning = null;
            if (testClip is null && SelectedAudio is { } source)
            {
                try
                {
                    liveAudio = new LiveAudioMonitor(source);
                    liveAudio.SetVolume(volume.Value, mute.Checked);
                    liveAudio.Failed += message => OnUi(() => { audioWarning = "เสียงขัดข้อง: " + message; store.Log(audioWarning); });
                    liveAudio.Start();
                }
                catch (Exception e) { liveAudio?.Dispose(); liveAudio = null; audioWarning = "เปิดเสียงไม่ได้: " + e.Message; store.Log(e.ToString()); }
            }
            active = true; decoded = false; lastDecoded = 0; stalledSeconds = 0; statsLogged = false; startedAt = DateTime.UtcNow;
            placeholder.Visible = false; monitor.Start();
            status.Text = "กำลังรอภาพจากอุปกรณ์...";
            detail.Text = "F11 เต็มจอ • Esc กลับหน้าต่าง • Tab พับเมนู • M ปิดเสียง • ปิดแอปเพื่อคืนอุปกรณ์";
        }
        catch (Exception e)
        {
            store.Log(e.ToString()); await ReleasePlayerAsync();
            active = false; placeholder.Visible = true; status.Text = "เชื่อมต่อไม่สำเร็จ: " + e.Message; ScheduleRetry();
        }
        finally { transitioning = false; operation.Release(); UpdateControls(); }
    }

    async Task StopAsync(string message)
    {
        await operation.WaitAsync();
        try
        {
            transitioning = true; active = false; monitor.Stop(); UpdateControls();
            status.Text = "กำลังหยุดและคืนอุปกรณ์...";
            await ReleasePlayerAsync();
            placeholder.Visible = true; status.Text = message; detail.Text = "ไม่ได้เปิดรับภาพหรือเสียง";
            if (!closing && !configuring && !selfTest) ScheduleRetry();
        }
        finally { transitioning = false; operation.Release(); UpdateControls(); }
    }

    async Task ReleasePlayerAsync()
    {
        var oldPlayer = player; var oldMedia = media;
        player = null; media = null;
        var oldAudio = liveAudio; liveAudio = null;
        if (oldAudio is not null) await Task.Run(oldAudio.Dispose);
        if (oldPlayer is not null)
        {
            await Task.Run(() => { oldPlayer.Stop(); oldPlayer.Dispose(); });
        }
        oldMedia?.Dispose();
    }

    async Task CheckFramesAsync()
    {
        if (!active || transitioning || player is null || media is null) return;
        var stats = media.Statistics;
        if (stats.DecodedVideo > 0)
        {
            decoded = true;
            if (stats.DecodedVideo == lastDecoded) stalledSeconds++; else stalledSeconds = 0;
            lastDecoded = stats.DecodedVideo;
            uint width = 0, height = 0;
            player.Size(0, ref width, ref height);
            status.Text = $"กำลังรับภาพ {width} × {height}" + (mute.Checked ? " • ปิดเสียง" : "");
            if (audioWarning is not null) status.Text += " • " + audioWarning;
            if ((!statsLogged || DateTime.UtcNow >= nextDiagnosticAt) && (DateTime.UtcNow - startedAt).TotalSeconds >= 5)
            {
                statsLogged = true; failures = 0; nextDiagnosticAt = DateTime.UtcNow.AddSeconds(10);
                store.Log($"Playback check: decodedVideo={stats.DecodedVideo}, displayedPictures={stats.DisplayedPictures}, decodedAudio={stats.DecodedAudio}, playedAudioBuffers={stats.PlayedAudioBuffers}, wasapiBytes={liveAudio?.ReceivedBytes}, wasapiPlayedBytes={liveAudio?.PlayedBytes}, wasapiUnderrunBytes={liveAudio?.UnderrunBytes}, wasapiDiscardedBytes={liveAudio?.DiscardedBytes}, wasapiPeak={liveAudio?.Peak}, size={width}x{height}, requested={settings.Mode}, captureCacheMs={settings.CacheMs}");
                try
                {
                    File.WriteAllText(Path.Combine(store.Root, "capture-status.json"), JsonSerializer.Serialize(new
                    {
                        Timestamp = DateTimeOffset.Now, Video = SelectedVideo?.Name, Audio = SelectedAudio?.Name,
                        Width = width, Height = height, CacheMs = settings.CacheMs,
                        DecodedVideo = stats.DecodedVideo, DisplayedPictures = stats.DisplayedPictures,
                        AudioBytes = liveAudio?.ReceivedBytes, PlayedAudioBytes = liveAudio?.PlayedBytes,
                        UnderrunAudioBytes = liveAudio?.UnderrunBytes, DiscardedAudioBytes = liveAudio?.DiscardedBytes,
                        AudioPeak = liveAudio?.Peak, AudioWarning = audioWarning
                    }, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception e) { store.Log("Capture status: " + e.Message); }
            }
            if (stalledSeconds >= 3) await StopAsync("ภาพหยุดส่ง — กำลังรอ HDMI และจะเชื่อมต่อใหม่อัตโนมัติ...");
        }
        else if ((DateTime.UtcNow - startedAt).TotalSeconds >= 12)
            await StopAsync("ยังไม่ได้รับเฟรมภาพ — ตรวจสาย HDMI หรือโปรแกรมที่ใช้อุปกรณ์อยู่ จะลองเชื่อมต่อใหม่...");
    }

    void ApplyVolume() { if (player is { } p) { p.Volume = volume.Value; p.Mute = mute.Checked; } liveAudio?.SetVolume(volume.Value, mute.Checked); }
    void UpdateVolumeText() => volumeLabel.Text = $"ระดับเสียง  {volume.Value}%";
    void UpdateModeText() => modeInfo.Text = (settings.Mode?.ToString() ?? "Auto — ให้ไดรเวอร์เลือกโหมด") + $"\nบัฟเฟอร์รับสัญญาณ {settings.CacheMs} ms";
    void SaveSettings() { if (!store.Save(settings)) status.Text = "บันทึกค่าไม่สำเร็จ — ตรวจสิทธิ์โฟลเดอร์ข้อมูลแอป"; }
    void UpdateControls()
    {
        bool idle = !active && !transitioning && !closing;
        video.Enabled = audio.Enabled = !transitioning && !closing && !configuring;
        refresh.Enabled = configure.Enabled = !transitioning && !closing && !configuring && vlc is not null;
        start.Enabled = idle && vlc is not null && (testClip is not null || SelectedVideo is not null);
        stop.Enabled = active && !transitioning && !closing;
    }
    void OnUi(Action action)
    {
        if (!IsDisposed && IsHandleCreated) { try { BeginInvoke(action); } catch (InvalidOperationException) { } }
    }

    void ToggleFullscreen()
    {
        if (!fullscreen)
        {
            savedWindowState = WindowState; savedBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            WindowState = FormWindowState.Normal; FormBorderStyle = FormBorderStyle.None;
            header.Visible = sidebar.Visible = footer.Visible = false;
            Bounds = Screen.FromControl(this).Bounds; fullscreen = true;
        }
        else
        {
            fullscreen = false; FormBorderStyle = FormBorderStyle.Sizable;
            header.Visible = footer.Visible = true; sidebar.Visible = !settings.MenuCollapsed;
            Bounds = savedBounds; WindowState = savedWindowState;
        }
        ActiveControl = null; Focus();
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F11) { ToggleFullscreen(); return true; }
        if (keyData == Keys.Escape && fullscreen) { ToggleFullscreen(); return true; }
        if (keyData == Keys.Tab && !video.Focused && !audio.Focused) { ToggleMenu(); return true; }
        if (keyData == Keys.M && !video.Focused && !audio.Focused) { mute.Checked = !mute.Checked; return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
    async void OnClosing(object? sender, FormClosingEventArgs e)
    {
        if (closeReady) return;
        e.Cancel = true;
        if (closing) return;
        closing = true; reconnect.Stop(); SaveSettings(); UpdateControls();
        if (initialization is not null) { try { await initialization; } catch { } }
        await StopAsync("หยุดแล้ว"); closeReady = true; Close();
    }

    void ToggleMenu()
    {
        if (fullscreen) return;
        settings.MenuCollapsed = !settings.MenuCollapsed;
        sidebar.Visible = !settings.MenuCollapsed;
        menu.Text = settings.MenuCollapsed ? "แสดงเมนู  Tab" : "ซ่อนเมนู  Tab";
        SaveSettings(); ActiveControl = null; Focus();
    }
    void ScheduleRetry()
    {
        failures = Math.Min(failures + 1, 6);
        retryAt = DateTime.UtcNow.AddSeconds(Math.Min(60, 3 * Math.Pow(2, failures - 1)));
    }
    async Task AutoConnectAsync()
    {
        if (selfTest || retryBusy || active || transitioning || closing || configuring || vlc is null || DateTime.UtcNow < retryAt) return;
        retryBusy = true;
        try
        {
            RefreshDevices();
            if (SelectedVideo is not null && (settings.AudioId is null || SelectedAudio is not null)) await StartAsync();
        }
        finally { retryBusy = false; }
    }

    async Task RunSelfTestAsync()
    {
        var checks = new List<string>();
        var report = new { Passed = false, Checks = checks };
        try
        {
            void Check(bool value, string description) { if (!value) throw new Exception(description); checks.Add(description); }
            Check(testClip is not null && File.Exists(testClip), "Synthetic fixture exists; no camera or microphone opened");
            Check(!active && player is null, "Self-test mode leaves physical capture devices closed");
            ToggleMenu(); Check(!sidebar.Visible && store.Load().MenuCollapsed, "Collapsed menu state is saved");
            ToggleMenu(); Check(sidebar.Visible && !store.Load().MenuCollapsed, "Menu reopens and saves state");
            volume.Value = 37; mute.Checked = true;
            var reloaded = store.Load();
            Check(reloaded.Volume == 37 && reloaded.Muted, "Volume and mute survive settings round-trip");
            settings.Mode = new CaptureMode(1920, 1080, 60, "MJPG"); settings.CacheMs = 100; SaveSettings();
            reloaded = store.Load();
            Check(reloaded.Mode == settings.Mode && reloaded.CacheMs == 100, "Capture mode and buffer survive settings round-trip");
            ToggleFullscreen(); Check(fullscreen && FormBorderStyle == FormBorderStyle.None && !sidebar.Visible, "Fullscreen hides controls");
            ToggleFullscreen(); Check(!fullscreen && sidebar.Visible, "Fullscreen returns to window");
            for (int round = 0; round < 2; round++)
            {
                await StartAsync();
                var watch = Stopwatch.StartNew();
                while (watch.Elapsed < TimeSpan.FromSeconds(10) && (!decoded || media?.Statistics.DecodedAudio <= 0) && active) await Task.Delay(100);
                Check(decoded && active && player is not null, $"Playback round {round + 1} decodes video frames");
                Check(media!.Statistics.DecodedAudio > 0, $"Playback round {round + 1} decodes audio samples");
                Check(player!.Volume == 37 && player.Mute, $"Playback round {round + 1} applies volume and mute");
                if (round == 0)
                {
                    Check(player.TakeSnapshot(0, Path.Combine(store.Root, "synthetic-preview.png"), 640, 360), "Rendered frame can be snapshotted");
                    ToggleFullscreen(); await Task.Delay(300); ToggleFullscreen();
                    Check(player.IsPlaying, "Video continues across fullscreen changes");
                    mute.Checked = false; Check(!player.Mute, "Unmute reaches audio engine");
                    mute.Checked = true;
                }
                await StopAsync("ทดสอบหยุดแล้ว");
                Check(!active && player is null && media is null && start.Enabled, $"Playback round {round + 1} releases player and enables restart");
            }
            report = new { Passed = true, Checks = checks };
        }
        catch (Exception e) { checks.Add("FAIL: " + e); Environment.ExitCode = 2; }
        Directory.CreateDirectory(store.Root);
        File.WriteAllText(Path.Combine(store.Root, "self-test.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Close();
    }
}
