using System.Text.Json.Serialization;

namespace MikroUpdate.Shared.Models;

/// <summary>
/// UNC paylaşımına erişim kimlik modeli.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NetworkAccessMode
{
    /// <summary>
    /// Servisin mevcut kimliğiyle doğrudan erişim (domain/gMSA).
    /// </summary>
    Direct,

    /// <summary>
    /// Kaydedilen kullanıcı adı/parolayla erişim (workgroup).
    /// </summary>
    Credential
}
