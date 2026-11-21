namespace SwitchCaptureViewer;

internal sealed class CaptureSettingsForm : Form
{
    readonly ComboBox resolution = Theme.Combo(), fps = Theme.Combo(), chroma = Theme.Combo(), cache = Theme.Combo();
    readonly Button read = Theme.Button("อ่านโหมดจากอุปกรณ์"), recommended = Theme.Button("เลือก 1080p/60 ถ้ามี");
    readonly Label state = Theme.Label("ยังไม่ได้อ่านโหมด — ใช้ Auto ได้ทันที");
    readonly string? deviceId;
    List<CaptureMode> modes = [];
    CaptureMode? preserved;
    bool updating;
    public CaptureMode? SelectedMode { get; private set; }
    public int CacheMs { get; private set; }

    public CaptureSettingsForm(CaptureDevice? device, AppSettings settings)
    {
        deviceId = device?.Id; preserved = settings.Mode;
        SelectedMode = settings.Mode; CacheMs = settings.CacheMs;
        Text = "ตั้งค่าการรับภาพและเสียง"; Size = new Size(700, 760); MinimumSize = new Size(640, 700);
        StartPosition = FormStartPosition.CenterParent; BackColor = Theme.Background; ForeColor = Theme.Text;
        Font = new Font("Leelawadee UI", 10); AutoScaleMode = AutoScaleMode.Dpi;
        MaximizeBox = false; MinimizeBox = false;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 1, RowCount = 13, AutoScroll = true };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);
        root.Controls.Add(Theme.Label("การรับภาพในแอป", 17, true));
        root.Controls.Add(Theme.Label(device?.Name ?? "เลือกอุปกรณ์วิดีโอที่หน้าหลักก่อน"));
        var action = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        read.Width = 195; recommended.Width = 225; recommended.Enabled = false; read.Enabled = device is not null;
        action.Controls.Add(read); action.Controls.Add(recommended); root.Controls.Add(action);
        state.MaximumSize = new Size(600, 0); state.ForeColor = Theme.Subtle; root.Controls.Add(state);
        if (preserved is not null) state.Text = "ค่าที่บันทึกไว้: " + preserved + "\nอ่านโหมดอีกครั้งเพื่อเปลี่ยนค่า หรือเลือก Auto";
        root.Controls.Add(Labeled("ความละเอียด", resolution));
        root.Controls.Add(Labeled("อัตราเฟรม (FPS)", fps));
        root.Controls.Add(Labeled("รูปแบบพิกเซล / การบีบอัด", chroma));
        var auto = Theme.Button("กลับไปใช้ Auto"); auto.Width = 175; root.Controls.Add(auto);
        auto.Click += (_, _) => { preserved = null; resolution.SelectedIndex = 0; UpdateFps(); state.Text = "เลือก Auto — ไดรเวอร์จะเจรจาโหมดเมื่อกดเริ่ม"; };
        resolution.Items.Add("Auto — ให้ไดรเวอร์เลือก"); resolution.SelectedIndex = 0;
        fps.Items.Add("Auto"); fps.SelectedIndex = 0; fps.Enabled = false;
        chroma.Items.Add("Auto"); chroma.SelectedIndex = 0; chroma.Enabled = false;
        if (preserved is not null)
        {
            resolution.Items.Add(preserved.Resolution); resolution.SelectedIndex = 1;
            fps.Items.Clear(); fps.Items.Add(preserved.Rate); fps.SelectedIndex = 0;
            chroma.Items.Clear(); chroma.Items.Add(preserved.Chroma); chroma.SelectedIndex = 0;
        }
        cache.Items.AddRange(new object[] { "50 ms — เริ่มต้น", "100 ms — เมื่อภาพสะดุด", "200 ms — เพิ่มความทนต่อการสะดุด" });
        cache.SelectedIndex = settings.CacheMs == 200 ? 2 : settings.CacheMs == 100 ? 1 : 0;
        root.Controls.Add(Labeled("บัฟเฟอร์รับภาพ (เสียงใช้ WASAPI แยก)", cache));
        var note = Theme.Label("บัฟเฟอร์นี้ไม่ใช่ค่าหน่วงรวมที่วัดได้ ยิ่งเพิ่มอาจยิ่งหน่วง\nเสียงรับ/ส่งผ่าน WASAPI แบบ shared ไม่เปลี่ยนค่าระบบ\nโหมดภาพอ่านจากไดรเวอร์ เฉพาะ SDR ไม่เกิน 1080p/60");
        note.ForeColor = Theme.Subtle; root.Controls.Add(note);
        root.Controls.Add(Theme.Label("ตั้งบน Nintendo Switch 2 เอง", 13, true));
        var console = Theme.Label("System Settings → Display: TV Resolution = 1080p,\n120 Hz Output = Off, HDR Output = Off (SDR)\nAudio → TV Sound = Stereo\nเป็นค่าเริ่มทดลองเมื่อการ์ดรองรับ แอปเปลี่ยน HDMI ของเครื่องเกมไม่ได้");
        console.ForeColor = Theme.Subtle; root.Controls.Add(console);
        var buttons = new FlowLayoutPanel { Height = 64, Dock = DockStyle.Bottom, Padding = new Padding(16, 4, 16, 6), FlowDirection = FlowDirection.RightToLeft };
        var apply = Theme.Button("บันทึก", true); var cancel = Theme.Button("ยกเลิก");
        buttons.Controls.Add(apply); buttons.Controls.Add(cancel); Controls.Add(buttons); buttons.BringToFront();
        AcceptButton = apply; CancelButton = cancel;
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        apply.Click += (_, _) =>
        {
            SelectedMode = Resolve(); CacheMs = cache.SelectedIndex == 2 ? 200 : cache.SelectedIndex == 1 ? 100 : 50;
            DialogResult = DialogResult.OK; Close();
        };
        read.Click += async (_, _) =>
        {
            read.Enabled = false; apply.Enabled = false; state.Text = "กำลังอ่านโหมดจากไดรเวอร์ (ยังไม่เริ่มรับภาพ)...";
            try
            {
                modes = await Devices.ReadModesAsync(deviceId!).WaitAsync(TimeSpan.FromSeconds(10));
                if (IsDisposed) return;
                preserved = null;
                updating = true;
                resolution.Items.Clear(); resolution.Items.Add("Auto — ให้ไดรเวอร์เลือก");
                foreach (var r in modes.Select(m => m.Resolution).Distinct()) resolution.Items.Add(r);
                resolution.SelectedIndex = 0; updating = false; UpdateFps();
                recommended.Enabled = modes.Any(m => m.Width == 1920 && m.Height == 1080 && m.Fps >= 59 && m.Fps <= 60.01);
                state.Text = modes.Count > 0 ? $"พบ {modes.Count} โหมดที่ไดรเวอร์รายงาน • Auto ใช้การเจรจากับไดรเวอร์" : "ไดรเวอร์ไม่รายงานโหมด SDR ที่รองรับในแอป — ใช้ Auto";
            }
            catch (Exception e) { if (!IsDisposed) state.Text = "อ่านโหมดไม่สำเร็จ — ใช้ Auto ได้\n" + e.Message; }
            finally { if (!IsDisposed) { apply.Enabled = true; /* Reopen dialog to retry a hung driver. */ } }
        };
        resolution.SelectedIndexChanged += (_, _) => { if (!updating) { preserved = null; UpdateFps(); } };
        fps.SelectedIndexChanged += (_, _) => { if (!updating) UpdateChroma(); };
        recommended.Click += (_, _) =>
        {
            var mode = modes.Where(m => m.Width == 1920 && m.Height == 1080 && m.Fps >= 59 && m.Fps <= 60.01)
                .OrderBy(m => m.Chroma == "YUY2" ? 0 : m.Chroma == "NV12" ? 1 : m.Chroma == "MJPG" ? 2 : 3).ThenByDescending(m => m.Fps).First();
            resolution.SelectedItem = mode.Resolution; fps.SelectedItem = mode.Rate; chroma.SelectedItem = mode.Chroma;
        };
    }

    static Control Labeled(string text, Control control)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Margin = new Padding(0) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.Controls.Add(Theme.Label(text)); panel.Controls.Add(control); return panel;
    }
    void UpdateFps()
    {
        updating = true; fps.Items.Clear();
        if (resolution.SelectedIndex <= 0) fps.Items.Add("Auto");
        else foreach (var r in modes.Where(m => m.Resolution == resolution.Text).Select(m => m.Rate).Distinct()) fps.Items.Add(r);
        fps.SelectedIndex = 0; fps.Enabled = resolution.SelectedIndex > 0;
        updating = false; UpdateChroma();
    }
    void UpdateChroma()
    {
        chroma.Items.Clear();
        if (resolution.SelectedIndex <= 0) chroma.Items.Add("Auto");
        else foreach (var c in modes.Where(m => m.Resolution == resolution.Text && m.Rate == fps.Text).Select(m => m.Chroma).Distinct()) chroma.Items.Add(c);
        chroma.SelectedIndex = 0; chroma.Enabled = resolution.SelectedIndex > 0;
    }
    CaptureMode? Resolve() => preserved ?? (resolution.SelectedIndex <= 0 ? null : modes.FirstOrDefault(m => m.Resolution == resolution.Text && m.Rate == fps.Text && m.Chroma == chroma.Text));
}
