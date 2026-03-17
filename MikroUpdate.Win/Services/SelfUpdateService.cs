using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;

namespace MikroUpdate.Win.Services;

/// <summary>
/// MikroUpdate uygulamasının kendini güncellemesini sağlar.
/// GitHub Releases API üzerinden en son sürümü kontrol eder,
/// installer'ı indirir ve sessiz kurulum başlatır.
/// </summary>
public sealed class SelfUpdateService : IDisposable
{
    private const string GitHubOwner = "hzkucuk";
    private const string GitHubRepo = "MikroUpdate";
    private const string LatestReleaseUrl =
        $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";

    private readonly HttpClient _httpClient;
    private bool _disposed;

    public SelfUpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MikroUpdate-SelfUpdate/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// GitHub'daki en son release bilgisini getirir.
    /// </summary>
    /// <returns>Release bilgisi veya hata durumunda null.</returns>
    public async Task<ReleaseInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        GitHubRelease? release = await _httpClient
            .GetFromJsonAsync<GitHubRelease>(LatestReleaseUrl, cancellationToken);

        if (release is null || string.IsNullOrWhiteSpace(release.TagName))
        {
            return null;
        }

        string versionText = release.TagName.TrimStart('v', 'V');

        if (!Version.TryParse(versionText, out Version? latestVersion))
        {
            return null;
        }

        Version? currentVersion = GetCurrentVersion();

        if (currentVersion is null || latestVersion <= currentVersion)
        {
            return null;
        }

        // Installer asset'ini bul (MikroUpdate_Setup_X.Y.Z.exe)
        GitHubAsset? installerAsset = release.Assets?
            .FirstOrDefault(a => a.Name?.StartsWith("MikroUpdate_Setup_", StringComparison.OrdinalIgnoreCase) == true
                              && a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        if (installerAsset is null || string.IsNullOrWhiteSpace(installerAsset.DownloadUrl))
        {
            return null;
        }

        return new ReleaseInfo
        {
            CurrentVersion = currentVersion,
            LatestVersion = latestVersion,
            DownloadUrl = installerAsset.DownloadUrl,
            InstallerFileName = installerAsset.Name!,
            ReleaseNotes = release.Body ?? string.Empty,
            PublishedAt = release.PublishedAt
        };
    }

    /// <summary>
    /// Installer'ı indirip sessiz kurulum başlatır.
    /// </summary>
    /// <param name="releaseInfo">İndirilecek release bilgisi.</param>
    /// <param name="progress">İndirme ilerleme bildirimi (0-100).</param>
    /// <param name="cancellationToken">İptal token'ı.</param>
    public async Task<string> DownloadInstallerAsync(
        ReleaseInfo releaseInfo,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(releaseInfo);

        string tempDir = Path.Combine(Path.GetTempPath(), "MikroUpdate_Update");
        Directory.CreateDirectory(tempDir);
        string installerPath = Path.Combine(tempDir, releaseInfo.InstallerFileName);

        // Önceki indirme varsa sil
        if (File.Exists(installerPath))
        {
            File.Delete(installerPath);
        }

        using HttpResponseMessage response = await _httpClient.GetAsync(
            releaseInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;

        await using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using FileStream fileStream = new(installerPath, FileMode.Create, FileAccess.Write, FileShare.None);

        byte[] buffer = new byte[81920];
        long totalRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalRead += bytesRead;

            if (totalBytes > 0 && progress is not null)
            {
                int percent = (int)(totalRead * 100 / totalBytes.Value);
                progress.Report(percent);
            }
        }

        return installerPath;
    }

    /// <summary>
    /// İndirilen installer'ı sessiz modda çalıştırır.
    /// Uygulama kapatılmalıdır.
    /// </summary>
    public static void LaunchInstaller(string installerPath)
    {
        ArgumentNullException.ThrowIfNull(installerPath);

        if (!File.Exists(installerPath))
        {
            throw new FileNotFoundException("Installer dosyası bulunamadı.", installerPath);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = "/SILENT /SUPPRESSMSGBOXES /RESTARTAPPLICATIONS",
            UseShellExecute = true
        });
    }

    private static Version? GetCurrentVersion()
    {
        string? versionText = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(versionText))
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        // +commitHash kısmını kes
        int plusIndex = versionText.IndexOf('+');

        if (plusIndex > 0)
        {
            versionText = versionText[..plusIndex];
        }

        return Version.TryParse(versionText, out Version? version) ? version : null;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Güncelleme bilgilerini taşıyan kayıt.
/// </summary>
public sealed class ReleaseInfo
{
    public required Version CurrentVersion { get; init; }
    public required Version LatestVersion { get; init; }
    public required string DownloadUrl { get; init; }
    public required string InstallerFileName { get; init; }
    public string ReleaseNotes { get; init; } = string.Empty;
    public DateTime? PublishedAt { get; init; }
}

// GitHub API DTO'ları (iç kullanım)
internal sealed class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string? TagName { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("published_at")]
    public DateTime? PublishedAt { get; set; }

    [JsonPropertyName("assets")]
    public List<GitHubAsset>? Assets { get; set; }
}

internal sealed class GitHubAsset
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string? DownloadUrl { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }
}
