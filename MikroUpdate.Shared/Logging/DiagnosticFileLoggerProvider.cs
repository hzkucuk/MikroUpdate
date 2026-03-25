using Microsoft.Extensions.Logging;

namespace MikroUpdate.Shared.Logging;

/// <summary>
/// Microsoft.Extensions.Logging ile entegre dosya log sağlayıcı.
/// Service projesi DI üzerinden bu provider'ı kullanır.
/// </summary>
[ProviderAlias("DiagnosticFile")]
public sealed class DiagnosticFileLoggerProvider : ILoggerProvider
{
    private readonly RollingFileLogger _writer;
    private bool _disposed;

    /// <summary>
    /// Yeni bir DiagnosticFileLoggerProvider oluşturur.
    /// </summary>
    /// <param name="filePrefix">Log dosyası ön eki (örn. "Service").</param>
    public DiagnosticFileLoggerProvider(string filePrefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePrefix);
        _writer = new RollingFileLogger(filePrefix);
    }

    /// <summary>
    /// Belirtilen kategori için bir ILogger oluşturur.
    /// </summary>
    public ILogger CreateLogger(string categoryName)
    {
        return new DiagnosticFileLogger(_writer, categoryName);
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
        _writer.Dispose();
    }

    /// <summary>
    /// ILogger implementasyonu. RollingFileLogger'a delege eder.
    /// </summary>
    private sealed class DiagnosticFileLogger : ILogger
    {
        private readonly RollingFileLogger _writer;
        private readonly string _category;

        public DiagnosticFileLogger(RollingFileLogger writer, string category)
        {
            _writer = writer;
            _category = SimplifyCategory(category);
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(formatter);

            string message = formatter(state, exception);

            if (string.IsNullOrWhiteSpace(message) && exception is null)
            {
                return;
            }

            string level = logLevel switch
            {
                LogLevel.Trace => "TRACE",
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => "CRIT",
                _ => "NONE"
            };

            if (exception is not null)
            {
                message = $"{message} | {exception.GetType().Name}: {exception.Message}";
            }

            _writer.Write(level, _category, message);
        }

        /// <summary>
        /// Kategori adını kısaltır: "MikroUpdate.Service.UpdateWorker" → "UpdateWorker"
        /// </summary>
        private static string SimplifyCategory(string category)
        {
            int lastDot = category.LastIndexOf('.');
            return lastDot >= 0 ? category[(lastDot + 1)..] : category;
        }
    }
}
