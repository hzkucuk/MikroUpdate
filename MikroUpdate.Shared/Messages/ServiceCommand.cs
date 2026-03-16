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
    ReloadConfig
}

/// <summary>
/// Tray uygulamasından servise gönderilen komut mesajı.
/// </summary>
public sealed class ServiceCommand
{
    /// <summary>Komut türü.</summary>
    public CommandType Command { get; set; }
}
