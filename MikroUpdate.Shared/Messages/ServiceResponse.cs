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

    /// <summary>Kurulum çalışıyor.</summary>
    Installing,

    /// <summary>İşlem tamamlandı.</summary>
    Completed,

    /// <summary>Hata oluştu.</summary>
    Error
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

    /// <summary>Sunucu versiyonu (varsa).</summary>
    public string? ServerVersion { get; set; }

    /// <summary>Terminal versiyonu (varsa).</summary>
    public string? LocalVersion { get; set; }

    /// <summary>Güncelleme gerekli mi.</summary>
    public bool UpdateRequired { get; set; }
}
