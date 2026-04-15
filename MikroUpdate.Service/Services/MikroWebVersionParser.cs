using System.Globalization;
using System.Text.RegularExpressions;

namespace MikroUpdate.Service.Services;

/// <summary>
/// mikro.com.tr sürüm güncellemeleri sayfasından en son CDN versiyon kodunu parse eder.
/// <para>
/// Sayfa yapısı: Her sürüm girişi bir tarih ve version_id query parametresi içerir.
/// İlk eşleşme en son yayınlanan sürümdür.
/// </para>
/// <example>
/// V16: version_id=16-40b → CDN kodu "40b"
/// V17: version_id=17-06d → CDN kodu "06d"
/// </example>
/// </summary>
public sealed partial class MikroWebVersionParser
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    /// <summary>
    /// Regex: sayfadaki tarih + version_id parametresini yakalar.
    /// Grup 1: Tarih (dd.MM.yyyy), Grup 2: CDN kodu (ör: 06d, 40b)
    /// </summary>
    [GeneratedRegex(
        @"(\d{2}\.\d{2}\.\d{4}).*?version_id=\d+-(\d+[a-z])",
        RegexOptions.Singleline,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex VersionPattern();

    public MikroWebVersionParser(HttpClient httpClient, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Mikro sürüm güncellemeleri sayfasından en son CDN kodunu parse eder.
    /// </summary>
    /// <param name="releaseNotesUrl">Sürüm güncellemeleri sayfası URL'si.</param>
    /// <param name="cancellationToken">İptal token'ı.</param>
    /// <returns>En son CDN kodu ve yayın tarihi veya okunamazsa null.</returns>
    public async Task<WebVersionResult?> GetLatestVersionAsync(
        string releaseNotesUrl, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(releaseNotesUrl);

        try
        {
            _logger.LogDebug("Mikro web sürüm kontrolü başlıyor: {Url}", releaseNotesUrl);

            string html = await _httpClient.GetStringAsync(releaseNotesUrl, cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(html))
            {
                _logger.LogWarning("Mikro sürüm sayfası boş döndü: {Url}", releaseNotesUrl);
                return null;
            }

            Match match = VersionPattern().Match(html);

            if (!match.Success)
            {
                _logger.LogWarning(
                    "Mikro sürüm sayfasında version_id bulunamadı: {Url}", releaseNotesUrl);
                return null;
            }

            string cdnCode = match.Groups[2].Value;
            string dateStr = match.Groups[1].Value;

            DateOnly? releaseDate = DateOnly.TryParseExact(
                dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out DateOnly parsed)
                ? parsed
                : null;

            _logger.LogInformation(
                "Mikro web sürüm tespit edildi: {CdnCode} ({Date})",
                cdnCode, releaseDate?.ToString("yyyy-MM-dd") ?? "tarih yok");

            return new WebVersionResult(cdnCode, releaseDate);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                "Mikro sürüm sayfasına erişilemedi: {Url} — {Error}", releaseNotesUrl, ex.Message);
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Mikro sürüm sayfası zaman aşımı: {Url}", releaseNotesUrl);
            return null;
        }
        catch (RegexMatchTimeoutException)
        {
            _logger.LogWarning(
                "Mikro sürüm sayfası regex zaman aşımı: {Url}", releaseNotesUrl);
            return null;
        }
    }

    /// <summary>
    /// Mikro sürüm sayfasından tüm sürüm kodlarını tarih sırasıyla parse eder.
    /// </summary>
    /// <param name="releaseNotesUrl">Sürüm güncellemeleri sayfası URL'si.</param>
    /// <param name="cancellationToken">İptal token'ı.</param>
    /// <returns>Tüm sürümler (en yeniden eskiye) veya boş liste.</returns>
    public async Task<IReadOnlyList<WebVersionResult>> GetAllVersionsAsync(
        string releaseNotesUrl, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(releaseNotesUrl);

        List<WebVersionResult> results = [];

        try
        {
            string html = await _httpClient.GetStringAsync(releaseNotesUrl, cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(html))
            {
                return results;
            }

            foreach (Match match in VersionPattern().Matches(html))
            {
                string cdnCode = match.Groups[2].Value;
                string dateStr = match.Groups[1].Value;

                DateOnly? releaseDate = DateOnly.TryParseExact(
                    dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateOnly parsed)
                    ? parsed
                    : null;

                results.Add(new WebVersionResult(cdnCode, releaseDate));
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or RegexMatchTimeoutException)
        {
            _logger.LogWarning(
                "Mikro sürüm listesi alınamadı: {Url} — {Error}", releaseNotesUrl, ex.Message);
        }

        return results;
    }
}

/// <summary>
/// Mikro web sitesinden parse edilen sürüm bilgisi.
/// </summary>
/// <param name="CdnCode">CDN versiyon kodu (ör: "06d", "40b").</param>
/// <param name="ReleaseDate">Yayın tarihi (parse edilebildiyse).</param>
public sealed record WebVersionResult(string CdnCode, DateOnly? ReleaseDate);
