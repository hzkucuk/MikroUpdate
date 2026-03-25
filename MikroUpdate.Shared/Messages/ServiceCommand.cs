namespace MikroUpdate.Shared.Messages;

/// <summary>
/// Pipe üzerinden gönderilen komut türleri.
/// </summary>
public enum CommandType
{
    /// <summary>Versiyon kontrolü yap.</summary>
    CheckVersion,

    /// <summary>Güncelleme başlat.</summary>
    RunUpdate,

    /// <summary>Mevcut durumu sorgula.</summary>
    GetStatus,

    /// <summary>Yapılandırmayı yeniden yükle.</summary>
    ReloadConfig,

    /// <summary>CDN'den güncelleme indir ve kur (online mod).</summary>
    DownloadUpdate,

    /// <summary>Tray app'in indirdiği self-update installer'ı servisten çalıştır (UAC'sız).</summary>
    InstallSelfUpdate
}

/// <summary>
/// Tray uygulamasından servise gönderilen komut mesajı.
/// </summary>
public sealed class ServiceCommand
{
    /// <summary>Komut türü.</summary>
    public CommandType Command { get; set; }

    /// <summary>Komut ile birlikte gönderilecek ek veri (ör. installer dosya yolu).</summary>
    public string? Data { get; set; }
}
