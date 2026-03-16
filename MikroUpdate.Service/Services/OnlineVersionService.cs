using System.Net;

using MikroUpdate.Shared.Helpers;
using MikroUpdate.Shared.Messages;
using MikroUpdate.Shared.Models;

namespace MikroUpdate.Service.Services;

/// <summary>
/// CDN üzerinden HTTP HEAD istekleriyle Mikro ERP versiyon kontrolü yapar.
/// Her modül için mevcut versiyondan başlayarak ileriye doğru probe eder ve
/// en güncel CDN versiyon kodunu tespit eder.
/// </summary>
public sealed class OnlineVersionService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    /// <summary>Probe sırasında bir minor'da hiçbir aday bulunamazsa boş streak sayacı artar.</summary>
    private const int MaxEmptyMinorStreak = 2;

    /// <summary>
    /// Son probe sonucunda bulunan en güncel CDN versiyon kodu (ör: "40b").
    /// DownloadService tarafından indirme URL'si oluşturmak için kullanılır.
    /// </summary>
    public string? LatestCdnCode { get; private set; }

    public OnlineVersionService(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        _httpClient = new HttpClient(new SocketsHttpHandler
        {
            ConnectTimeout = TimeSpan.FromSeconds(10),
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MikroUpdate/1.0");
    }

    /// <summary>
    /// Tüm aktif modüller için CDN'den en güncel versiyonu tespit eder.
    /// Ana modül (Client) üzerinden probe yapılır, bulunan CDN kodu tüm modüllere uygulanır.
    /// </summary>
    public async Task<List<ModuleVersionInfo>> GetOnlineModuleVersionsAsync(
        UpdateConfig config,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        List<ModuleVersionInfo> results = [];

        // Client modülünden mevcut yerel versiyonu al (probe başlangıç noktası)
        UpdateModule? clientModule = config.EnabledModules
            .FirstOrDefault(m => m.Name.Equals("Client", StringComparison.OrdinalIgnoreCase));

        if (clientModule is null)
        {
            _logger.LogWarning("Client modülü bulunamadı, online versiyon kontrolü yapılamaz.");

            return results;
        }

        string localExePath = Path.Combine(config.LocalInstallPath, clientModule.ExeFileName);
        Version? localClientVersion = GetLocalVersion(localExePath);

        // CDN'de en güncel versiyon kodunu bul
        string? latestCdnCode = await ProbeLatestCdnVersionAsync(
            config, clientModule, localClientVersion, cancellationToken).ConfigureAwait(false);

        // Son probe sonucunu sakla (DownloadService için)
        LatestCdnCode = latestCdnCode;

        // Her modül için sonuçları oluştur
        foreach (UpdateModule module in config.EnabledModules)
        {
            string localPath = Path.Combine(config.LocalInstallPath, module.ExeFileName);
            Version? localVersion = GetLocalVersion(localPath);

            string? serverVersionStr = null;
            bool updateRequired = false;

            if (latestCdnCode is not null && localClientVersion is not null)
            {
                string? currentCdnCode = CdnHelper.EncodeCdnVersion(localClientVersion);

                if (currentCdnCode is not null && !latestCdnCode.Equals(currentCdnCode, StringComparison.OrdinalIgnoreCase))
                {
                    updateRequired = true;

                    // CDN kodundan yaklaşık versiyon üret
                    (int Minor, int Patch)? decoded = CdnHelper.DecodeCdnVersion(latestCdnCode);

                    if (decoded is not null)
                    {
                        serverVersionStr = $"{localClientVersion.Major}.{decoded.Value.Minor}.{decoded.Value.Patch}.0";
                    }
                }
                else
                {
                    serverVersionStr = localVersion?.ToString();
                }
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
    /// Belirtilen modül için CDN'deki en güncel versiyon kodunu probe eder.
    /// </summary>
    /// <returns>En güncel CDN versiyon kodu (ör: "40b") veya bulunamazsa null.</returns>
    private async Task<string?> ProbeLatestCdnVersionAsync(
        UpdateConfig config,
        UpdateModule module,
        Version? currentVersion,
        CancellationToken cancellationToken)
    {
        if (currentVersion is null)
        {
            _logger.LogWarning("Yerel versiyon okunamadı, CDN probe yapılamaz.");

            return null;
        }

        string? currentCode = CdnHelper.EncodeCdnVersion(currentVersion);

        if (currentCode is null)
        {
            _logger.LogWarning("Mevcut versiyon CDN koduna dönüştürülemedi: {Version}", currentVersion);

            return null;
        }

        _logger.LogDebug("CDN probe başlıyor. Mevcut: {Code} ({Version})", currentCode, currentVersion);

        string? latestFound = currentCode;
        int currentMinor = currentVersion.Minor;
        int emptyMinorStreak = 0;

        foreach (string candidate in CdnHelper.GenerateProbeCandidates(currentVersion))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Minor değişimini takip et (boş streak kontrolü için)
            (int Minor, int Patch)? decoded = CdnHelper.DecodeCdnVersion(candidate);

            if (decoded is null)
            {
                continue;
            }

            if (decoded.Value.Minor != currentMinor)
            {
                // Önceki minor'da hiç bulunamadıysa streak arttır
                if (decoded.Value.Patch == 1 && decoded.Value.Minor > currentMinor + 1)
                {
                    // Yeni minor'ın ilk patch'i; önceki minor kontrol edildi
                }

                currentMinor = decoded.Value.Minor;
            }

            string url = CdnHelper.BuildDownloadUrl(
                config.CdnBaseUrl, config.MajorVersion, candidate, module.SetupFileName);

            bool exists = await CheckUrlExistsAsync(url, cancellationToken).ConfigureAwait(false);

            if (exists)
            {
                _logger.LogDebug("CDN'de bulundu: {Code} → {Url}", candidate, url);
                latestFound = candidate;
                emptyMinorStreak = 0;
            }
            else
            {
                // Bu minor'ın son patch'iyse ve hiç bulunmadıysa streak arttır
                if (decoded.Value.Patch == 10) // MaxPatchPerMinor
                {
                    emptyMinorStreak++;

                    if (emptyMinorStreak >= MaxEmptyMinorStreak)
                    {
                        _logger.LogDebug(
                            "Ardışık {Count} boş minor, probe durduruluyor. Son bulunan: {Code}",
                            MaxEmptyMinorStreak, latestFound);

                        break;
                    }
                }
            }
        }

        // Mevcut versiyonla aynıysa güncelleme yok
        return latestFound.Equals(currentCode, StringComparison.OrdinalIgnoreCase)
            ? currentCode
            : latestFound;
    }

    /// <summary>
    /// URL'nin CDN'de mevcut olup olmadığını HTTP HEAD isteği ile kontrol eder.
    /// </summary>
    private async Task<bool> CheckUrlExistsAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Head, url);
            using HttpResponseMessage response = await _httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogDebug("HTTP HEAD hatası: {Url} — {Error}", url, ex.Message);

            return false;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("HTTP HEAD zaman aşımı: {Url}", url);

            return false;
        }
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

        System.Diagnostics.FileVersionInfo versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath);

        if (string.IsNullOrWhiteSpace(versionInfo.FileVersion))
        {
            return null;
        }

        return Version.TryParse(versionInfo.FileVersion, out Version? version) ? version : null;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
