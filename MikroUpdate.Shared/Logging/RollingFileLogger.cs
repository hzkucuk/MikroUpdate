using System.Globalization;
using System.Text;

namespace MikroUpdate.Shared.Logging;

/// <summary>
/// Thread-safe rolling file logger.
/// Günlük rotasyon, boyut limiti (~5 MB/dosya) ve otomatik eski log temizliği (7 gün) sağlar.
/// Hem Service hem Tray App tarafından kullanılır.
/// </summary>
public sealed class RollingFileLogger : IDisposable
{
    /// <summary>Maximum single log file size in bytes (~5 MB).</summary>
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    /// <summary>Number of days to keep old log files.</summary>
    private const int RetentionDays = 7;

    private static readonly string DefaultLogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "MikroUpdate", "logs");

    private readonly string _logDirectory;
    private readonly string _filePrefix;
    private readonly Lock _lock = new();
    private StreamWriter? _writer;
    private string _currentDate = string.Empty;
    private long _currentFileSize;
    private int _currentSegment;
    private bool _disposed;
    private DateTime _lastCleanup = DateTime.MinValue;

    /// <summary>
    /// Yeni bir RollingFileLogger oluşturur.
    /// </summary>
    /// <param name="filePrefix">Log dosyası ön eki (örn. "Service" veya "App").</param>
    /// <param name="logDirectory">Log dizini. Null ise varsayılan ProgramData yolu kullanılır.</param>
    public RollingFileLogger(string filePrefix, string? logDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePrefix);
        _filePrefix = filePrefix;
        _logDirectory = logDirectory ?? DefaultLogDirectory;
    }

    /// <summary>
    /// Log dizininin tam yolunu döner.
    /// </summary>
    public string LogDirectory => _logDirectory;

    /// <summary>
    /// Belirtilen seviye, kaynak ve mesajı log dosyasına yazar.
    /// </summary>
    public void Write(string level, string source, string message)
    {
        if (_disposed || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        string line = $"[{timestamp}] [{level,-5}] [{source}] {message}";

        lock (_lock)
        {
            try
            {
                EnsureWriter();
                _writer?.WriteLine(line);
                _writer?.Flush();

                _currentFileSize += Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;
            }
            catch (IOException)
            {
                // Dosya yazım hatası — sessizce geç, uygulama akışını bozma
            }
            catch (UnauthorizedAccessException)
            {
                // Yetkisizlik — sessizce geç
            }
        }
    }

    /// <summary>
    /// Kaynakları serbest bırakır.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        lock (_lock)
        {
            _writer?.Dispose();
            _writer = null;
        }
    }

    private void EnsureWriter()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        bool dateChanged = _currentDate != today;
        bool sizeExceeded = _currentFileSize >= MaxFileSizeBytes;

        if (_writer is not null && !dateChanged && !sizeExceeded)
        {
            return;
        }

        _writer?.Dispose();
        _writer = null;

        if (dateChanged)
        {
            _currentDate = today;
            _currentSegment = 0;
            _currentFileSize = 0;

            CleanupOldLogsIfNeeded();
        }

        if (sizeExceeded)
        {
            _currentSegment++;
            _currentFileSize = 0;
        }

        Directory.CreateDirectory(_logDirectory);

        string fileName = _currentSegment == 0
            ? $"{_filePrefix}_{today}.log"
            : $"{_filePrefix}_{today}_{_currentSegment}.log";

        string filePath = Path.Combine(_logDirectory, fileName);

        var fileInfo = new FileInfo(filePath);
        _currentFileSize = fileInfo.Exists ? fileInfo.Length : 0;

        _writer = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8)
        {
            AutoFlush = false
        };
    }

    /// <summary>
    /// Günde en fazla bir kez eski log dosyalarını temizler.
    /// </summary>
    private void CleanupOldLogsIfNeeded()
    {
        if ((DateTime.UtcNow - _lastCleanup).TotalHours < 24)
        {
            return;
        }

        _lastCleanup = DateTime.UtcNow;

        try
        {
            if (!Directory.Exists(_logDirectory))
            {
                return;
            }

            DateTime cutoff = DateTime.Now.AddDays(-RetentionDays);
            string pattern = $"{_filePrefix}_*.log";

            foreach (string file in Directory.EnumerateFiles(_logDirectory, pattern))
            {
                try
                {
                    if (File.GetLastWriteTime(file) < cutoff)
                    {
                        File.Delete(file);
                    }
                }
                catch (IOException)
                {
                    // Kullanımda olan dosya — atla
                }
                catch (UnauthorizedAccessException)
                {
                    // Yetki yok — atla
                }
            }
        }
        catch (IOException)
        {
            // Dizin erişim hatası — temizlik atlanır
        }
    }
}
