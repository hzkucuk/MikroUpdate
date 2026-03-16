using System.Security.Cryptography;
using System.Text;

namespace MikroUpdate.Shared.Helpers;

/// <summary>
/// Gemini API anahtarını DPAPI ile şifreler/çözer.
/// <c>DataProtectionScope.LocalMachine</c> kullanılarak hem Windows Service hem de
/// kullanıcı uygulaması aynı makinede anahtara erişebilir.
/// </summary>
public static class AiKeyManager
{
    private static readonly byte[] s_entropy =
        "MikroUpdate.AI.2025"u8.ToArray();

    /// <summary>
    /// Düz metin API anahtarını DPAPI ile şifreleyerek Base64 döndürür.
    /// </summary>
    public static string Encrypt(string plainTextKey)
    {
        if (string.IsNullOrWhiteSpace(plainTextKey))
        {
            return string.Empty;
        }

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainTextKey);

        byte[] encryptedBytes = ProtectedData.Protect(
            plainBytes,
            s_entropy,
            DataProtectionScope.LocalMachine);

        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Base64 şifreli anahtarı DPAPI ile çözerek düz metne döndürür.
    /// Hata durumunda boş string döner (farklı makine, bozuk veri vb.).
    /// </summary>
    public static string Decrypt(string encryptedBase64)
    {
        if (string.IsNullOrWhiteSpace(encryptedBase64))
        {
            return string.Empty;
        }

        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);

            byte[] plainBytes = ProtectedData.Unprotect(
                encryptedBytes,
                s_entropy,
                DataProtectionScope.LocalMachine);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException)
        {
            return string.Empty;
        }
        catch (FormatException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Şifreli anahtarın geçerli olup olmadığını kontrol eder.
    /// </summary>
    public static bool HasValidKey(string? encryptedBase64) =>
        !string.IsNullOrWhiteSpace(encryptedBase64) &&
        !string.IsNullOrWhiteSpace(Decrypt(encryptedBase64));
}
