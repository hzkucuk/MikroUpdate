using System.Security.Cryptography;
using System.Text;

namespace MikroUpdate.Shared.Security;

/// <summary>
/// Sunucu erişim parolasını DPAPI ile makine bazlı korur.
/// </summary>
public static class NetworkCredentialProtector
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("MikroUpdate.NetworkCredential.v1");

    /// <summary>
    /// Parolayı makine bazlı DPAPI ile şifreler.
    /// </summary>
    /// <param name="plainTextPassword">Düz metin parola.</param>
    /// <returns>Base64 şifreli değer. Boş girişte boş döner.</returns>
    public static string Protect(string plainTextPassword)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword))
        {
            return string.Empty;
        }

        byte[] clearBytes = Encoding.UTF8.GetBytes(plainTextPassword);
        byte[] protectedBytes = ProtectedData.Protect(clearBytes, Entropy, DataProtectionScope.LocalMachine);

        return Convert.ToBase64String(protectedBytes);
    }

    /// <summary>
    /// DPAPI ile korunmuş Base64 parolayı çözer.
    /// </summary>
    /// <param name="encryptedPassword">Base64 şifreli parola.</param>
    /// <returns>Çözülen parola; çözülemezse null.</returns>
    public static string? Unprotect(string encryptedPassword)
    {
        if (string.IsNullOrWhiteSpace(encryptedPassword))
        {
            return null;
        }

        try
        {
            byte[] protectedBytes = Convert.FromBase64String(encryptedPassword);
            byte[] clearBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.LocalMachine);

            return Encoding.UTF8.GetString(clearBytes);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
