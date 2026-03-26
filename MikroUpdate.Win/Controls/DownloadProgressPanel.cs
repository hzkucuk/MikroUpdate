using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace MikroUpdate.Win.Controls;

/// <summary>
/// Custom-painted, double-buffered indirme ilerleme paneli.
/// Rounded gradient progress bar, modül adı, boyut, yüzde ve hız bilgisi çizer.
/// </summary>
internal sealed class DownloadProgressPanel : Panel
{
    private static readonly Color s_barBackground = Color.FromArgb(50, 50, 50);
    private static readonly Color s_gradientStart = Color.FromArgb(0, 190, 110);
    private static readonly Color s_gradientEnd = Color.FromArgb(0, 140, 80);
    private static readonly Color s_textModule = Color.FromArgb(80, 210, 140);
    private static readonly Color s_textInfo = Color.FromArgb(170, 170, 170);
    private static readonly Color s_textDim = Color.FromArgb(120, 120, 120);

    private readonly Font _fontModule = new("Segoe UI Semibold", 8.5F);
    private readonly Font _fontInfo = new("Segoe UI", 8F);
    private readonly Font _fontSmall = new("Segoe UI", 7.5F);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ModuleName { get; set; } = "";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public long BytesReceived { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public long TotalBytes { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Percentage { get; set; } = -1;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public long SpeedBps { get; set; }

    public DownloadProgressPanel()
    {
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw, true);
        Height = 56;
        BackColor = Color.Transparent;
    }

    /// <summary>Paneli ilk durumuna sıfırlar.</summary>
    public void Reset()
    {
        ModuleName = "";
        BytesReceived = 0;
        TotalBytes = 0;
        Percentage = -1;
        SpeedBps = 0;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        int w = ClientSize.Width;

        // --- Row 0: Modül adı (sol) + Boyut (sağ) ---
        using SolidBrush brushModule = new(s_textModule);
        using SolidBrush brushInfo = new(s_textInfo);
        using SolidBrush brushDim = new(s_textDim);

        if (!string.IsNullOrEmpty(ModuleName))
        {
            g.DrawString($"▼ {ModuleName}", _fontModule, brushModule, 0, 0);
        }

        if (TotalBytes > 0)
        {
            string sizeText = $"{FormatBytes(BytesReceived)} / {FormatBytes(TotalBytes)}";
            SizeF sz = g.MeasureString(sizeText, _fontInfo);
            g.DrawString(sizeText, _fontInfo, brushInfo, w - sz.Width, 1);
        }

        // --- Row 1: Rounded progress bar ---
        int barY = 22;
        int barH = 12;
        Rectangle barBounds = new(0, barY, w, barH);
        int radius = 6;

        // Arka plan
        using GraphicsPath bgPath = CreateRoundedRect(barBounds, radius);
        using SolidBrush bgBrush = new(s_barBackground);
        g.FillPath(bgBrush, bgPath);

        // Dolgu (gradient)
        if (Percentage > 0)
        {
            int fillW = Math.Max(barH, (int)(w * Percentage / 100.0));
            Rectangle fillBounds = new(0, barY, fillW, barH);

            using GraphicsPath fillPath = CreateRoundedRect(fillBounds, radius);
            using LinearGradientBrush fillBrush = new(
                new Rectangle(0, barY, w, barH),
                s_gradientStart,
                s_gradientEnd,
                LinearGradientMode.Horizontal);
            g.FillPath(fillBrush, fillPath);
        }
        else if (Percentage < 0)
        {
            // Belirsiz durum — yarı genişlikte animasyonsuz gösterge
            int indW = w / 3;
            Rectangle indBounds = new(0, barY, indW, barH);
            using GraphicsPath indPath = CreateRoundedRect(indBounds, radius);
            using SolidBrush indBrush = new(Color.FromArgb(80, s_gradientStart));
            g.FillPath(indBrush, indPath);
        }

        // --- Row 2: Yüzde (sol) + Hız (sağ) ---
        int row2Y = barY + barH + 4;

        if (Percentage >= 0)
        {
            g.DrawString($"%{Percentage}", _fontSmall, brushDim, 0, row2Y);
        }

        if (SpeedBps > 0)
        {
            string speedText = $"{FormatBytes(SpeedBps)}/s";
            SizeF sz = g.MeasureString(speedText, _fontSmall);
            g.DrawString(speedText, _fontSmall, brushDim, w - sz.Width, row2Y);
        }
    }

    private static GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
    {
        GraphicsPath path = new();
        int d = radius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fontModule.Dispose();
            _fontInfo.Dispose();
            _fontSmall.Dispose();
        }

        base.Dispose(disposing);
    }

    internal static string FormatBytes(long bytes) => bytes switch
    {
        >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F1} GB",
        >= 1_048_576 => $"{bytes / 1_048_576.0:F1} MB",
        >= 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes} B"
    };
}
