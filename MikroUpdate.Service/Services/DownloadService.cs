using System.Diagnostics;

using MikroUpdate.Shared.Helpers;
using MikroUpdate.Shared.Messages;
using MikroUpdate.Shared.Models;

namespace MikroUpdate.Service.Services;

/// <summary>
/// CDN üzerinden HTTP ile Mikro ERP setup dosyalarını indirir.
/// İlerleme callback'i ile pipe üzerinden tray uygulamasına canlı bildirim sağlar.
/// </summary>
public sealed class DownloadService : IDisposable
{
    private static readonly string TempDirectory = Path.Combine(Path.GetTempPath(), "MikroUpdate");

    /// <summary>İndirme arabellek boyutu (64 KB).</summary>
    private const int BufferSize = 64 * 1024;

    /// <summary>İlerleme bildirimi minimum aralığı.</summary>
    private static readonly TimeSpan ProgressInterval = TimeSpan.FromMilliseconds(250);

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    /// <summary>Başarısız indirmede tekrar deneme sayısı.</summary>
    private const int MaxRetryCount = 3;

    public DownloadService(ILogger logger, string? proxyAddress = null, int timeoutSeconds = 0)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        _httpClient = HttpClientFactory.CreateForDownload(proxyAddress, timeoutSeconds);
    }

    /// <summary>
    /// Belirtilen modül için CDN'den setup dosyasını indirir.
    /// </summary>
    /// <param name="config">Güncelleme yapılandırması (CDN URL, major version).</param>
    /// <param name="module">İndirilecek modül.</param>
    /// <param name="cdnCode">CDN versiyon kodu (ör: "40b").</param>
    /// <param name="onProgress">İlerleme bildirimi callback'i.</param>
    /// <param name="cancellationToken">İptal tokenı.</param>
    /// <returns>İndirilen dosyanın geçici yolu veya hata durumunda null.</returns>
    public async Task<string?> DownloadSetupAsync(
        UpdateConfig config,
        UpdateModule module,
        string cdnCode,
        Action<DownloadProgressInfo>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(cdnCode);

        string url = CdnHelper.BuildDownloadUrl(
            config.CdnBaseUrl, config.MajorVersion, cdnCode, module.SetupFileName);

        for (int attempt = 1; attempt <= MaxRetryCount; attempt++)
        {
            string? result = await TryDownloadAsync(
                url, module, onProgress, attempt, cancellationToken).ConfigureAwait(false);

            if (result is not null)
            {
                return result;
            }

            // Son denemeyse tekrar deneme yok
            if (attempt >= MaxRetryCount)
            {
                break;
            }

            // Exponential backoff: 2s, 4s, 8s...
            int delaySeconds = (int)Math.Pow(2, attempt);
            _logger.LogWarning(
                "[{Module}] İndirme denemesi {Attempt}/{Max} başarısız. {Delay}s sonra tekrar denenecek...",
                module.Name, attempt, MaxRetryCount, delaySeconds);

            ReportProgress(onProgress, module.Name, 0, 0, -1,
                $"{module.Name} tekrar deneniyor ({attempt}/{MaxRetryCount})... {delaySeconds}s bekleniyor");

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken).ConfigureAwait(false);
        }

        return null;
    }

    /// <summary>
    /// Tek bir indirme denemesi gerçekleştirir.
    /// </summary>
    private async Task<string?> TryDownloadAsync(
        string url,
        UpdateModule module,
        Action<DownloadProgressInfo>? onProgress,
        int attempt,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Module}] CDN indirme başlıyor (deneme {Attempt}/{Max}): {Url}",
            module.Name, attempt, MaxRetryCount, url);

        ReportProgress(onProgress, module.Name, 0, 0, -1,
            attempt > 1 ? $"{module.Name} tekrar deneniyor ({attempt}/{MaxRetryCount})..." : "İndirme başlatılıyor...");

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(
                url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "[{Module}] CDN HTTP hatası: {StatusCode} — {Url}",
                    module.Name, response.StatusCode, url);

                ReportProgress(onProgress, module.Name, 0, 0, -1,
                    $"İndirme hatası: HTTP {(int)response.StatusCode}");

                // 4xx hataları kalıcı — tekrar deneme anlamsız
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return null;
                }

                return null;
            }

            long totalBytes = response.Content.Headers.ContentLength ?? -1;

            _logger.LogInformation(
                "[{Module}] İndirme başladı. Boyut: {Size}",
                module.Name, totalBytes > 0 ? FormatBytes(totalBytes) : "bilinmiyor");

            Directory.CreateDirectory(TempDirectory);
            string tempFile = Path.Combine(TempDirectory, module.SetupFileName);

            await using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            await using FileStream fileStream = new(tempFile, FileMode.Create, FileAccess.Write, FileShare.None,
                BufferSize, useAsync: true);

            byte[] buffer = new byte[BufferSize];
            long bytesReceived = 0;
            Stopwatch speedTimer = Stopwatch.StartNew();
            Stopwatch progressTimer = Stopwatch.StartNew();
            long lastSpeedBytes = 0;

            while (true)
            {
                int bytesRead = await contentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    break;
                }

                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                bytesReceived += bytesRead;

                // Belirli aralıklarla ilerleme bildir
                if (progressTimer.Elapsed >= ProgressInterval)
                {
                    int percentage = totalBytes > 0 ? (int)(bytesReceived * 100 / totalBytes) : -1;
                    long speed = CalculateSpeed(bytesReceived, lastSpeedBytes, speedTimer);

                    ReportProgress(onProgress, module.Name, bytesReceived, totalBytes, speed,
                        FormatProgressText(module.Name, bytesReceived, totalBytes, speed));

                    progressTimer.Restart();
                    lastSpeedBytes = bytesReceived;
                    speedTimer.Restart();
                }
            }

            // Son ilerleme bildirimi (%100)
            ReportProgress(onProgress, module.Name, bytesReceived, totalBytes, -1,
                $"{module.Name} indirme tamamlandı — {FormatBytes(bytesReceived)}");

            _logger.LogInformation(
                "[{Module}] İndirme tamamlandı: {Size} → {Path}",
                module.Name, FormatBytes(bytesReceived), tempFile);

            return tempFile;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[{Module}] CDN bağlantı hatası (deneme {Attempt}): {Url}",
                module.Name, attempt, url);

            ReportProgress(onProgress, module.Name, 0, 0, -1,
                $"{module.Name} bağlantı hatası: {ex.Message}");

            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("[{Module}] CDN indirme zaman aşımı (deneme {Attempt}): {Url}",
                module.Name, attempt, url);

            ReportProgress(onProgress, module.Name, 0, 0, -1,
                $"{module.Name} indirme zaman aşımı.");

            return null;
        }
    }

    /// <summary>
    /// Geçici dizini temizler.
    /// </summary>
    public static void CleanupTempFiles()
    {
        if (Directory.Exists(TempDirectory))
        {
            try
            {
                Directory.Delete(TempDirectory, recursive: true);
            }
            catch (IOException)
            {
                // Dosyalar kullanımda olabilir
            }
        }
    }

    private static void ReportProgress(
        Action<DownloadProgressInfo>? onProgress,
        string moduleName,
        long bytesReceived,
        long totalBytes,
        long speed,
        string statusText)
    {
        onProgress?.Invoke(new DownloadProgressInfo
        {
            ModuleName = moduleName,
            BytesReceived = bytesReceived,
            TotalBytes = totalBytes,
            Percentage = totalBytes > 0 ? (int)(bytesReceived * 100 / totalBytes) : -1,
            StatusText = statusText,
            SpeedBytesPerSecond = speed
        });
    }

    private static long CalculateSpeed(long currentBytes, long previousBytes, Stopwatch timer)
    {
        double seconds = timer.Elapsed.TotalSeconds;

        return seconds > 0.1 ? (long)((currentBytes - previousBytes) / seconds) : -1;
    }

    private static string FormatProgressText(string moduleName, long received, long total, long speed)
    {
        string receivedStr = FormatBytes(received);

        if (total > 0)
        {
            string totalStr = FormatBytes(total);
            int pct = (int)(received * 100 / total);
            string speedStr = speed > 0 ? $" — {FormatBytes(speed)}/s" : "";

            return $"{moduleName}: {receivedStr} / {totalStr} (%{pct}){speedStr}";
        }

        return $"{moduleName}: {receivedStr} indiriliyor...";
    }

    private static string FormatBytes(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F2} GB"
    };

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
