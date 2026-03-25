using MikroUpdate.Shared.Logging;

namespace MikroUpdate.Win.Services;

/// <summary>
/// Dosya tabanlı log servisi.
/// Paylaşılan RollingFileLogger altyapısını kullanır.
/// Günlük rotasyon, ~5 MB boyut limiti ve 7 gün saklama süresi ile çalışır.
/// </summary>
public sealed class FileLogService : IDisposable
{
    private readonly RollingFileLogger _logger = new("App");

    /// <summary>
    /// Log dizininin tam yolunu döner.
    /// </summary>
    public static string GetLogDirectory() => new RollingFileLogger("App").LogDirectory;

    /// <summary>
    /// Belirtilen seviye ve mesajı log dosyasına yazar.
    /// </summary>
    public void Write(LogLevel level, string message)
    {
        _logger.Write(level.ToString(), "App", message);
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
        _logger.Dispose();
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
