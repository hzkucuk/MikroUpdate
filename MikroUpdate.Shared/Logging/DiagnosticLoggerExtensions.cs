using Microsoft.Extensions.Logging;

namespace MikroUpdate.Shared.Logging;

/// <summary>
/// ILoggingBuilder için DiagnosticFileLogger uzantı metotları.
/// </summary>
public static class DiagnosticLoggerExtensions
{
    /// <summary>
    /// Dosya tabanlı tanılama log sağlayıcısını DI'a ekler.
    /// Günlük rotasyon, 5 MB boyut limiti ve 7 gün saklama süresi ile çalışır.
    /// </summary>
    /// <param name="builder">Logging builder.</param>
    /// <param name="filePrefix">Log dosyası ön eki (örn. "Service").</param>
    public static ILoggingBuilder AddDiagnosticFileLogger(this ILoggingBuilder builder, string filePrefix = "Service")
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.AddProvider(new DiagnosticFileLoggerProvider(filePrefix));
        return builder;
    }
}
