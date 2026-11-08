using System.ComponentModel;
namespace RoHelper;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundedRect : Control
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor { get; set; } = SystemColors.ActiveBorder;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 1000;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool MaxRoundCorner { get; set; } = false;

    public RoundedRect()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        int radius = CornerRadius;
        if (MaxRoundCorner)
        {
            radius = Math.Min(Height,Width)/2;
        }

        using var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();

        using var brush = new SolidBrush(FillColor);
        e.Graphics.FillPath(brush, path);
    }
}