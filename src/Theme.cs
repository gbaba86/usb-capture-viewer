namespace SwitchCaptureViewer;

internal static class Theme
{
    public static readonly Color Background = Color.FromArgb(19, 23, 31);
    public static readonly Color Surface = Color.FromArgb(28, 34, 44);
    public static readonly Color Text = Color.FromArgb(235, 239, 246);
    public static readonly Color Subtle = Color.FromArgb(163, 176, 195);
    public static readonly Color Accent = Color.FromArgb(82, 219, 186);

    public static Label Label(string text, float size = 10, bool bold = false) => new()
    {
        Text = text, ForeColor = Text, AutoSize = true,
        Font = new Font("Leelawadee UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
        Margin = new Padding(0, 5, 0, 6)
    };
    public static Button Button(string text, bool primary = false) => new()
    {
        Text = text, AutoSize = false, Height = 42, Width = 140,
        FlatStyle = FlatStyle.Flat, BackColor = primary ? Accent : Surface,
        ForeColor = primary ? Background : Text,
        Font = new Font("Leelawadee UI", 10, FontStyle.Bold),
        Cursor = Cursors.Hand, Margin = new Padding(0, 4, 8, 4),
        UseVisualStyleBackColor = false
    };
    public static ComboBox Combo() => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill,
        Font = new Font("Leelawadee UI", 10), IntegralHeight = false, DropDownHeight = 300,
        BackColor = Surface, ForeColor = Text, Margin = new Padding(0, 0, 0, 10)
    };
}
