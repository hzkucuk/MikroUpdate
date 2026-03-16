namespace MikroUpdate.Shared.Helpers;

/// <summary>
/// Mikro ERP CDN versiyon kodlama ve URL oluşturma yardımcısı.
/// <para>
/// CDN versiyon kodu formatı: {minor}{harf}
/// Patch 1→a, 2→b, ..., 6→f, 7→g, ..., 10→j
/// Örnek: 16.39.6 → "39f", 16.40.1 → "40a"
/// </para>
/// </summary>
public static class CdnHelper
{
    /// <summary>Probe sırasında minor içinde taranacak maksimum patch (a=1 ... j=10).</summary>
    private const int MaxPatchPerMinor = 10;

    /// <summary>Probe sırasında ardışık boş minor bulununca durulacak limit.</summary>
    private const int MaxEmptyMinorStreak = 2;

    /// <summary>
    /// Versiyon nesnesinden CDN versiyon kodunu üretir.
    /// </summary>
    /// <param name="version">Mikro ERP versiyonu (ör: 16.39.6.46086).</param>
    /// <returns>CDN versiyon kodu (ör: "39f") veya patch geçersizse null.</returns>
    public static string? EncodeCdnVersion(Version version)
    {
        ArgumentNullException.ThrowIfNull(version);

        int minor = version.Minor;
        int patch = version.Build;

        if (patch < 1)
        {
            return null;
        }

        char letter = (char)('a' + patch - 1);

        return $"{minor}{letter}";
    }

    /// <summary>
    /// CDN versiyon kodunu minor ve patch değerlerine çözer.
    /// </summary>
    /// <param name="cdnCode">CDN versiyon kodu (ör: "39f").</param>
    /// <returns>Minor ve patch değerleri veya geçersiz kodda null.</returns>
    public static (int Minor, int Patch)? DecodeCdnVersion(string cdnCode)
    {
        if (string.IsNullOrWhiteSpace(cdnCode) || cdnCode.Length < 2)
        {
            return null;
        }

        char letter = cdnCode[^1];

        if (!char.IsAsciiLetterLower(letter))
        {
            return null;
        }

        string minorStr = cdnCode[..^1];

        if (!int.TryParse(minorStr, out int minor))
        {
            return null;
        }

        int patch = letter - 'a' + 1;

        return (minor, patch);
    }

    /// <summary>
    /// Belirtilen versiyon için CDN setup indirme URL'sini oluşturur.
    /// </summary>
    /// <param name="cdnBaseUrl">CDN temel URL'si (ör: "https://cdn-mikro.atros.com.tr/mikro").</param>
    /// <param name="majorVersion">Ana sürüm (ör: "V16", "V17").</param>
    /// <param name="cdnCode">CDN versiyon kodu (ör: "39f").</param>
    /// <param name="setupFileName">Setup dosya adı (ör: "Jump_v16xx_Client_Setupx064.exe").</param>
    /// <returns>Tam indirme URL'si.</returns>
    public static string BuildDownloadUrl(string cdnBaseUrl, string majorVersion, string cdnCode, string setupFileName)
    {
        ArgumentNullException.ThrowIfNull(cdnBaseUrl);
        ArgumentNullException.ThrowIfNull(majorVersion);
        ArgumentNullException.ThrowIfNull(cdnCode);
        ArgumentNullException.ThrowIfNull(setupFileName);

        string versionFolder = majorVersion.Equals("V17", StringComparison.OrdinalIgnoreCase) ? "v17" : "v16";

        return $"{cdnBaseUrl.TrimEnd('/')}/{versionFolder}/{cdnCode}/{setupFileName}";
    }

    /// <summary>
    /// Mevcut versiyondan başlayarak ileriye dönük aday CDN kodlarını üretir.
    /// Probe algoritması tarafından kullanılır.
    /// </summary>
    /// <param name="currentVersion">Mevcut terminal versiyonu (ör: 16.39.6.46086).</param>
    /// <returns>Denenecek CDN kodları sırayla (ör: "39g", "39h", ..., "40a", "40b", ...).</returns>
    public static IEnumerable<string> GenerateProbeCandidates(Version currentVersion)
    {
        ArgumentNullException.ThrowIfNull(currentVersion);

        int minor = currentVersion.Minor;
        int patch = currentVersion.Build;
        int emptyMinorStreak = 0;

        // Mevcut minor'daki kalan patch'leri dene
        for (int p = patch + 1; p <= MaxPatchPerMinor; p++)
        {
            yield return $"{minor}{(char)('a' + p - 1)}";
        }

        // Sonraki minor'lara geç
        for (int m = minor + 1; emptyMinorStreak < MaxEmptyMinorStreak; m++)
        {
            bool anyFound = false;

            for (int p = 1; p <= MaxPatchPerMinor; p++)
            {
                string candidate = $"{m}{(char)('a' + p - 1)}";

                yield return candidate;
                anyFound = true;
            }

            // Boş minor streak takibi probe sırasında çağıran tarafça yapılır,
            // burada tüm adayları üretiyoruz.
            if (!anyFound)
            {
                emptyMinorStreak++;
            }
            else
            {
                // Bir minor'da en az 1 aday üretildi,
                // çağıran taraf hangilerinin 200 döndüğünü kontrol edecek.
                // Streak sıfırlama çağıran tarafa bırakılır.
                emptyMinorStreak = 0;
            }

            // Makul üst sınır: 20 minor ileriye kadar tara
            if (m - minor > 20)
            {
                break;
            }
        }
    }
}
