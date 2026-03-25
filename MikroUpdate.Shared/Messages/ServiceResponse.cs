namespace MikroUpdate.Shared.Messages;

/// <summary>
/// Servisin mevcut durumunu temsil eden enum.
/// </summary>
public enum ServiceStatus
{
    /// <summary>Boşta, işlem yok.</summary>
    Idle,

    /// <summary>Versiyon kontrol ediliyor.</summary>
    Checking,

    /// <summary>Setup dosyası kopyalanıyor.</summary>
    CopyingSetup,

    /// <summary>CDN'den indiriliyor.</summary>
    Downloading,

    /// <summary>Kurulum çalışıyor.</summary>
    Installing,

    /// <summary>İşlem tamamlandı.</summary>
    Completed,

    /// <summary>Hata oluştu.</summary>
    Error
}

/// <summary>
/// Modül bazlı versiyon bilgisi.
/// </summary>
public sealed class ModuleVersionInfo
{
    /// <summary>Modül adı (ör: Client, e-Defter, Beyanname).</summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>Terminal versiyonu.</summary>
    public string? LocalVersion { get; set; }

    /// <summary>Sunucu versiyonu.</summary>
    public string? ServerVersion { get; set; }

    /// <summary>Bu modül için güncelleme gerekli mi.</summary>
    public bool UpdateRequired { get; set; }

    /// <summary>Kaynak türü: "Yerel" (UNC paylaşım), "CDN" (HTTP), veya boş.</summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>Sunucu tam yolu veya URL'si (tooltip bilgisi için).</summary>
    public string ServerPath { get; set; } = string.Empty;
}

/// <summary>
/// Servisten tray uygulamasına dönen yanıt mesajı.
/// </summary>
public sealed class ServiceResponse
{
    /// <summary>İşlem başarılı mı.</summary>
    public bool Success { get; set; }

    /// <summary>Servisin mevcut durumu.</summary>
    public ServiceStatus Status { get; set; }

    /// <summary>Durum veya hata mesajı.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Sunucu versiyonu (ana modül — geriye uyumluluk).</summary>
    public string? ServerVersion { get; set; }

    /// <summary>Terminal versiyonu (ana modül — geriye uyumluluk).</summary>
    public string? LocalVersion { get; set; }

    /// <summary>Güncelleme gerekli mi (herhangi bir modülde).</summary>
    public bool UpdateRequired { get; set; }

    /// <summary>Modül bazlı versiyon bilgileri.</summary>
    public List<ModuleVersionInfo> ModuleVersions { get; set; } = [];

    /// <summary>
    /// İndirme ilerleme bilgisi.
    /// Status == Downloading olduğunda dolu gelir.
    /// </summary>
    public DownloadProgressInfo? DownloadProgress { get; set; }

    /// <summary>
    /// Ara ilerleme mesajı mı? true ise bağlantı açık kalır ve sonraki mesaj beklenir.
    /// false ise bu son yanıttır (terminal).
    /// </summary>
    public bool IsProgressMessage { get; set; }
}

/// <summary>
/// CDN indirme ilerleme bilgisi.
/// Pipe üzerinden ara mesaj (progress stream) olarak gönderilir.
/// </summary>
public sealed class DownloadProgressInfo
{
    /// <summary>İndirilen modül adı (ör: Client, e-Defter).</summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>İndirilen byte miktarı.</summary>
    public long BytesReceived { get; set; }

    /// <summary>Toplam dosya boyutu (biliniyorsa, yoksa -1).</summary>
    public long TotalBytes { get; set; } = -1;

    /// <summary>Yüzdesel ilerleme (0-100). TotalBytes bilinmiyorsa -1.</summary>
    public int Percentage { get; set; } = -1;

    /// <summary>Kullanıcıya gösterilecek durum metni.</summary>
    public string StatusText { get; set; } = string.Empty;

    /// <summary>İndirme hızı (byte/sn). Hesaplanamıyorsa -1.</summary>
    public long SpeedBytesPerSecond { get; set; } = -1;
}
