using System.Text.Json.Serialization;

namespace MikroUpdate.Shared.Models;

/// <summary>
/// Güncelleme modu. Versiyon kontrolü ve indirme kaynağını belirler.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UpdateMode
{
    /// <summary>Sadece yerel ağ (UNC path) üzerinden kontrol ve kurulum.</summary>
    Local,

    /// <summary>Sadece CDN üzerinden HTTP ile kontrol ve indirme.</summary>
    Online,

    /// <summary>Önce yerel ağı dene, erişilemezse CDN'ye düş.</summary>
    Hybrid,

    /// <summary>Gemini AI ile mikro.com.tr sayfasından versiyon tespiti ve CDN indirme.</summary>
    AI
}
