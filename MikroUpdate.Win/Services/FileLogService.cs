namespace MikroUpdate.Win.Services;

/// <summary>
/// Dosya tabanlı log servisi.
/// Günlük rotasyonlu log dosyalarını %ProgramData%\MikroUpdate\logs\ dizinine yazar.
/// </summary>
public sealed class FileLogService : IDisposable
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "MikroUpdate", "logs");

    private readonly Lock _lock = new();
    private StreamWriter? _writer;
    private string _currentDate = string.Empty;
    private bool _disposed;

    /// <summary>
    /// Log dizininin tam yolunu döner.
    /// </summary>
    public static string GetLogDirectory() => LogDirectory;

    /// <summary>
    /// Belirtilen seviye ve mesajı log dosyasına yazar.
    /// </summary>
    public void Write(LogLevel level, string message)
    {
        if (_disposed || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string line = $"[{timestamp}] [{level}] {message}";

        lock (_lock)
        {
            try
            {
                EnsureWriter();
                _writer?.WriteLine(line);
                _writer?.Flush();
            }
            catch (IOException)
            {
                // Dosya yazım hatası — UI log zaten devam ediyor, sessizce geç
            }
            catch (UnauthorizedAccessException)
            {
                // Yetkisizlik — sessizce geç
            }
        }
    }

    /// <summary>
    /// Bilgi seviyesinde log yazar.
    /// </summary>
    public void Info(string message) => Write(LogLevel.INFO, message);

    /// <summary>
    /// Başarı seviyesinde log yazar.
    /// </summary>
    public void Success(string message) => Write(LogLevel.OK, message);

    /// <summary>
    /// Uyarı seviyesinde log yazar.
    /// </summary>
    public void Warning(string message) => Write(LogLevel.WARN, message);

    /// <summary>
    /// Hata seviyesinde log yazar.
    /// </summary>
    public void Error(string message) => Write(LogLevel.ERROR, message);

    /// <summary>
    /// Hata seviyesinde exception detayıyla log yazar.
    /// </summary>
    public void Error(string message, Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        Write(LogLevel.ERROR, $"{message} | {ex.GetType().Name}: {ex.Message}");
    }

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
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        if (_writer is not null && _currentDate == today)
        {
            return;
        }

        _writer?.Dispose();
        Directory.CreateDirectory(LogDirectory);

        string logFilePath = Path.Combine(LogDirectory, $"MikroUpdate_{today}.log");
        _writer = new StreamWriter(logFilePath, append: true, encoding: System.Text.Encoding.UTF8)
        {
            AutoFlush = false
        };

        _currentDate = today;
    }
}

/// <summary>
/// Log seviyeleri.
/// </summary>
public enum LogLevel
{
    INFO,
    OK,
    WARN,
    ERROR
}
