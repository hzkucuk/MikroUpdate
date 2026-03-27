using System.Drawing.Drawing2D;
using System.ServiceProcess;

namespace MikroUpdate.Win.Controls;

/// <summary>
/// Tray icon'u servis durumuna göre yönetir.
/// Servis çalışırken yeşil, durmuşken kırmızı yanıp sönen durum noktası gösterir.
/// </summary>
internal sealed class TrayIconManager : IDisposable
{
    private const string ServiceName = "MikroUpdateService";
    private const int PollIntervalMs = 10_000;
    private const int BlinkIntervalMs = 600;

    private readonly NotifyIcon _notifyIcon;
    private readonly Icon _baseIcon;
    private readonly System.Windows.Forms.Timer _pollTimer;
    private readonly System.Windows.Forms.Timer _blinkTimer;

    private Icon? _greenIcon;
    private Icon? _redIcon;
    private bool _blinkVisible = true;
    private bool _isServiceRunning;
    private bool _disposed;

    /// <summary>
    /// Servis durumu değiştiğinde tetiklenir. True = çalışıyor, False = durmuş.
    /// </summary>
    public event Action<bool>? ServiceStatusChanged;

    public TrayIconManager(NotifyIcon notifyIcon, Icon baseIcon)
    {
        ArgumentNullException.ThrowIfNull(notifyIcon);
        ArgumentNullException.ThrowIfNull(baseIcon);

        _notifyIcon = notifyIcon;
        _baseIcon = baseIcon;

        _pollTimer = new System.Windows.Forms.Timer { Interval = PollIntervalMs };
        _pollTimer.Tick += PollTimer_Tick;

        _blinkTimer = new System.Windows.Forms.Timer { Interval = BlinkIntervalMs };
        _blinkTimer.Tick += BlinkTimer_Tick;

        _greenIcon = CreateOverlayIcon(Color.FromArgb(50, 205, 50));
        _redIcon = CreateOverlayIcon(Color.FromArgb(220, 40, 40));
    }

    /// <summary>
    /// Son kontrol edilen servis durumunu döndürür.
    /// </summary>
    public bool IsServiceRunning => _isServiceRunning;

    /// <summary>
    /// Periyodik servis durum kontrolünü başlatır ve ilk kontrolü hemen yapar.
    /// </summary>
    public void Start()
    {
        CheckServiceStatus();
        _pollTimer.Start();
    }

    /// <summary>
    /// Periyodik kontrolü durdurur ve blink'i kapatır.
    /// </summary>
    public void Stop()
    {
        _pollTimer.Stop();
        _blinkTimer.Stop();
    }

    /// <summary>
    /// Servis durumunu hemen kontrol eder ve icon'u günceller.
    /// Dışarıdan tetiklenen servis komutları sonrası çağrılabilir.
    /// </summary>
    public void Refresh()
    {
        CheckServiceStatus();
    }

    private void PollTimer_Tick(object? sender, EventArgs e)
    {
        CheckServiceStatus();
    }

    private void BlinkTimer_Tick(object? sender, EventArgs e)
    {
        _blinkVisible = !_blinkVisible;
        _notifyIcon.Icon = _blinkVisible ? _redIcon : _baseIcon;
    }

    private void CheckServiceStatus()
    {
        bool running = QueryServiceRunning();

        if (running == _isServiceRunning && (_isServiceRunning || _blinkTimer.Enabled))
        {
            // Durum değişmedi — mevcut animasyonu koru
            return;
        }

        bool changed = running != _isServiceRunning;
        _isServiceRunning = running;

        if (running)
        {
            _blinkTimer.Stop();
            _blinkVisible = true;
            _notifyIcon.Icon = _greenIcon;
        }
        else
        {
            _notifyIcon.Icon = _redIcon;
            _blinkVisible = true;
            _blinkTimer.Start();
        }

        if (changed)
        {
            ServiceStatusChanged?.Invoke(running);
        }
    }

    private static bool QueryServiceRunning()
    {
        try
        {
            using ServiceController sc = new(ServiceName);
            sc.Refresh();

            return sc.Status == ServiceControllerStatus.Running;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Base icon üzerine sağ alt köşeye renkli durum noktası çizer.
    /// </summary>
    private Icon CreateOverlayIcon(Color dotColor)
    {
        const int size = 32;
        const int dotSize = 12;
        const int dotMargin = 1;

        using Bitmap bmp = new(size, size);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Base icon çiz
            g.DrawIcon(_baseIcon, new Rectangle(0, 0, size, size));

            // Durum noktası konumu: sağ alt köşe
            int dotX = size - dotSize - dotMargin;
            int dotY = size - dotSize - dotMargin;

            // Beyaz halka (border) — görünürlük için
            using SolidBrush borderBrush = new(Color.FromArgb(30, 30, 30));
            g.FillEllipse(borderBrush, dotX - 1, dotY - 1, dotSize + 2, dotSize + 2);

            // Renkli dolgu
            using SolidBrush fillBrush = new(dotColor);
            g.FillEllipse(fillBrush, dotX, dotY, dotSize, dotSize);
        }

        return System.Drawing.Icon.FromHandle(bmp.GetHicon());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _pollTimer.Stop();
        _pollTimer.Dispose();
        _blinkTimer.Stop();
        _blinkTimer.Dispose();

        if (_greenIcon is not null)
        {
            DestroyIcon(_greenIcon.Handle);
            _greenIcon.Dispose();
            _greenIcon = null;
        }

        if (_redIcon is not null)
        {
            DestroyIcon(_redIcon.Handle);
            _redIcon.Dispose();
            _redIcon = null;
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool DestroyIcon(nint hIcon);
}
