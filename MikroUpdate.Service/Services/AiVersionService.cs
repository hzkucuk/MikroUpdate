using MikroUpdate.Shared.Helpers;
using MikroUpdate.Shared.Messages;
using MikroUpdate.Shared.Models;

namespace MikroUpdate.Service.Services;

/// <summary>
/// AI modu versiyon kontrol servisi.
/// Mikro güncelleme sayfasını indirir, Gemini API ile versiyon bilgisi çıkarır,
/// CDN koduna dönüştürür ve indirme için hazırlar.
/// </summary>
public sealed class AiVersionService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly GeminiService _geminiService;
    private readonly ILogger _logger;

    /// <summary>
    /// AI ile tespit edilen en güncel CDN versiyon kodu (ör: "40b").
    /// DownloadService tarafından indirme URL'si oluşturmak için kullanılır.
    /// </summary>
    public string? LatestCdnCode { get; private set; }

    public AiVersionService(GeminiService geminiService, ILogger logger, string? proxyAddress = null, int timeoutSeconds = 0)
    {
        ArgumentNullException.ThrowIfNull(geminiService);
        ArgumentNullException.ThrowIfNull(logger);

        _geminiService = geminiService;
        _logger = logger;

        _httpClient = HttpClientFactory.Create(
            proxyAddress,
            timeoutSeconds,
            defaultTimeoutSeconds: 30,
            connectTimeoutSeconds: 15);
    }

    /// <summary>
    /// AI modu ile tüm aktif modüller için en güncel versiyonu tespit eder.
    /// Güncelleme sayfasını indirir → Gemini ile parse eder → CDN koduna çevirir.
    /// </summary>
    public async Task<List<ModuleVersionInfo>> GetAiModuleVersionsAsync(
        UpdateConfig config,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        List<ModuleVersionInfo> results = [];

        // API anahtarını çöz
        string apiKey = AiKeyManager.Decrypt(config.GeminiApiKey);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API anahtarı yapılandırılmamış veya çözülemedi.");

            return BuildErrorResults(config, "API anahtarı eksik");
        }

        // Güncelleme sayfasını indir
        string? pageHtml = await DownloadUpdatePageAsync(
            config.MikroUpdatePageUrl, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(pageHtml))
        {
            _logger.LogWarning("Güncelleme sayfası indirilemedi: {Url}", config.MikroUpdatePageUrl);

            return BuildErrorResults(config, "Sayfa indirilemedi");
        }

        // Gemini ile versiyon çıkar
        Version? detectedVersion = await _geminiService.ExtractVersionFromHtmlAsync(
            pageHtml, apiKey, config.MajorVersion, cancellationToken).ConfigureAwait(false);

        if (detectedVersion is null)
        {
            _logger.LogWarning("AI versiyon tespit edemedi.");

            return BuildErrorResults(config, "Versiyon tespit edilemedi");
        }

        // CDN koduna dönüştür
        string? cdnCode = CdnHelper.EncodeCdnVersion(detectedVersion);

        if (cdnCode is null)
        {
            _logger.LogWarning("AI versiyonu CDN koduna dönüştürülemedi: {Version}", detectedVersion);

            return BuildErrorResults(config, "CDN kodu oluşturulamadı");
        }

        LatestCdnCode = cdnCode;
        _logger.LogInformation("AI versiyon tespit etti: {Version} → CDN: {Code}", detectedVersion, cdnCode);

        // Her modül için sonuçları oluştur
        foreach (UpdateModule module in config.EnabledModules)
        {
            string localPath = Path.Combine(config.LocalInstallPath, module.ExeFileName);
            Version? localVersion = GetLocalVersion(localPath);

            bool updateRequired = false;
            string? serverVersionStr = detectedVersion.ToString();

            if (localVersion is not null)
            {
                string? localCdnCode = CdnHelper.EncodeCdnVersion(localVersion);
                updateRequired = localCdnCode is null ||
                    !cdnCode.Equals(localCdnCode, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // Yerel versiyon okunamazsa güncelleme gerekli sayılır
                updateRequired = true;
            }

            results.Add(new ModuleVersionInfo
            {
                ModuleName = module.Name,
                LocalVersion = localVersion?.ToString(),
                ServerVersion = serverVersionStr,
                UpdateRequired = updateRequired
            });
        }

        return results;
    }

    /// <summary>
    /// Güncelleme sayfasının HTML içeriğini indirir.
    /// </summary>
    private async Task<string?> DownloadUpdatePageAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Güncelleme sayfası indiriliyor: {Url}", url);

            using HttpResponseMessage response = await _httpClient.GetAsync(
                url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Sayfa indirme hatası: {Url} — {Error}", url, ex.Message);

            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Sayfa indirme zaman aşımı: {Url}", url);

            return null;
        }
    }

    /// <summary>
    /// Hata durumunda tüm modüller için boş sonuç listesi oluşturur.
    /// </summary>
    private static List<ModuleVersionInfo> BuildErrorResults(UpdateConfig config, string reason)
    {
        return config.EnabledModules.Select(module =>
        {
            string localPath = Path.Combine(config.LocalInstallPath, module.ExeFileName);
            Version? localVersion = GetLocalVersion(localPath);

            return new ModuleVersionInfo
            {
                ModuleName = module.Name,
                LocalVersion = localVersion?.ToString(),
                ServerVersion = null,
                UpdateRequired = false
            };
        }).ToList();
    }

    /// <summary>
    /// Yerel EXE dosyasının versiyonunu okur.
    /// </summary>
    private static Version? GetLocalVersion(string exePath)
    {
        if (!File.Exists(exePath))
        {
            return null;
        }

        System.Diagnostics.FileVersionInfo versionInfo =
            System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath);

        if (string.IsNullOrWhiteSpace(versionInfo.FileVersion))
        {
            return null;
        }

        return Version.TryParse(versionInfo.FileVersion, out Version? version) ? version : null;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _geminiService.Dispose();
    }
}
