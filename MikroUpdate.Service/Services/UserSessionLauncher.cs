using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MikroUpdate.Service.Services;

/// <summary>
/// SYSTEM oturumundan (Session 0) aktif kullanıcının masaüstü oturumunda process başlatır.
/// WTSQueryUserToken + CreateProcessAsUser P/Invoke pattern kullanır.
/// </summary>
[SupportedOSPlatform("windows")]
internal static partial class UserSessionLauncher
{
    /// <summary>
    /// Aktif konsol oturumundaki kullanıcı olarak belirtilen uygulamayı başlatır.
    /// </summary>
    /// <param name="exePath">Çalıştırılacak uygulamanın tam yolu.</param>
    /// <param name="logger">Loglama için ILogger instance'ı.</param>
    /// <returns>İşlem başarılı ise true.</returns>
    public static bool LaunchInUserSession(string exePath, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        if (string.IsNullOrWhiteSpace(exePath))
        {
            logger.LogError("LaunchInUserSession: exePath boş veya null.");

            return false;
        }

        nint userToken = nint.Zero;
        nint duplicateToken = nint.Zero;
        nint environment = nint.Zero;

        try
        {
            // 1. Aktif konsol oturumunu bul
            uint sessionId = WTSGetActiveConsoleSessionId();

            if (sessionId == 0xFFFFFFFF)
            {
                logger.LogError("Aktif konsol oturumu bulunamadı.");

                return false;
            }

            logger.LogInformation("Aktif konsol oturumu: {SessionId}", sessionId);

            // 2. Oturumdaki kullanıcının token'ını al
            if (!WTSQueryUserToken(sessionId, out userToken))
            {
                int error = Marshal.GetLastWin32Error();
                logger.LogError("WTSQueryUserToken başarısız. Win32 hata kodu: {ErrorCode}", error);

                return false;
            }

            // 3. Token'ı duplicate et (CreateProcessAsUser için gerekli)
            if (!DuplicateTokenEx(
                    userToken,
                    MAXIMUM_ALLOWED,
                    nint.Zero,
                    SecurityImpersonationLevel.SecurityIdentification,
                    TokenType.TokenPrimary,
                    out duplicateToken))
            {
                int error = Marshal.GetLastWin32Error();
                logger.LogError("DuplicateTokenEx başarısız. Win32 hata kodu: {ErrorCode}", error);

                return false;
            }

            // 4. Kullanıcı ortam değişkenlerini oluştur
            if (!CreateEnvironmentBlock(out environment, duplicateToken, false))
            {
                int error = Marshal.GetLastWin32Error();
                logger.LogWarning("CreateEnvironmentBlock başarısız (hata: {ErrorCode}), ortam değişkensiz devam ediliyor.", error);
                environment = nint.Zero;
            }

            // 5. Process bilgilerini hazırla
            nint desktopPtr = Marshal.StringToHGlobalUni("winsta0\\default");

            try
            {
                STARTUPINFO startupInfo = new()
                {
                    cb = Marshal.SizeOf<STARTUPINFO>(),
                    lpDesktop = desktopPtr,
                    dwFlags = STARTF_USESHOWWINDOW,
                    wShowWindow = SW_SHOWNORMAL
                };

                uint creationFlags = CREATE_UNICODE_ENVIRONMENT | CREATE_NEW_CONSOLE;

                // 6. Kullanıcı oturumunda process başlat
                // lpCommandLine: exe yolunu tırnak içinde ver (boşluk içeren yollar için)
                string commandLine = $"\"{exePath}\"";

                if (!CreateProcessAsUser(
                        duplicateToken,
                        exePath,
                        commandLine,
                        nint.Zero,
                        nint.Zero,
                        false,
                        creationFlags,
                        environment,
                        Path.GetDirectoryName(exePath),
                        ref startupInfo,
                        out PROCESS_INFORMATION processInfo))
                {
                    int error = Marshal.GetLastWin32Error();
                    logger.LogError(
                        "CreateProcessAsUser başarısız. Exe: {ExePath}, Win32 hata kodu: {ErrorCode}",
                        exePath, error);

                    return false;
                }

                // Handle'ları kapat
                CloseHandle(processInfo.hProcess);
                CloseHandle(processInfo.hThread);

                logger.LogInformation(
                    "Kullanıcı oturumunda process başlatıldı. PID: {ProcessId}, Exe: {ExePath}",
                    processInfo.dwProcessId, exePath);

                return true;
            }
            finally
            {
                Marshal.FreeHGlobal(desktopPtr);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LaunchInUserSession hatası. Exe: {ExePath}", exePath);

            return false;
        }
        finally
        {
            if (environment != nint.Zero)
            {
                DestroyEnvironmentBlock(environment);
            }

            if (duplicateToken != nint.Zero)
            {
                CloseHandle(duplicateToken);
            }

            if (userToken != nint.Zero)
            {
                CloseHandle(userToken);
            }
        }
    }

    #region P/Invoke Declarations

    private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
    private const uint CREATE_NEW_CONSOLE = 0x00000010;
    private const uint MAXIMUM_ALLOWED = 0x02000000;
    private const int STARTF_USESHOWWINDOW = 0x00000001;
    private const short SW_SHOWNORMAL = 1;

    private enum SecurityImpersonationLevel
    {
        SecurityIdentification = 1
    }

    private enum TokenType
    {
        TokenPrimary = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct STARTUPINFO
    {
        public int cb;
        public nint lpReserved;
        public nint lpDesktop;
        public nint lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public nint lpReserved2;
        public nint hStdInput;
        public nint hStdOutput;
        public nint hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
        public nint hProcess;
        public nint hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    [LibraryImport("kernel32.dll")]
    private static partial uint WTSGetActiveConsoleSessionId();

    [LibraryImport("wtsapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool WTSQueryUserToken(uint sessionId, out nint phToken);

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DuplicateTokenEx(
        nint hExistingToken,
        uint dwDesiredAccess,
        nint lpTokenAttributes,
        SecurityImpersonationLevel impersonationLevel,
        TokenType tokenType,
        out nint phNewToken);

    [LibraryImport("advapi32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CreateProcessAsUser(
        nint hToken,
        string? lpApplicationName,
        string? lpCommandLine,
        nint lpProcessAttributes,
        nint lpThreadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles,
        uint dwCreationFlags,
        nint lpEnvironment,
        string? lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    [LibraryImport("userenv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CreateEnvironmentBlock(
        out nint lpEnvironment,
        nint hToken,
        [MarshalAs(UnmanagedType.Bool)] bool bInherit);

    [LibraryImport("userenv.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyEnvironmentBlock(nint lpEnvironment);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(nint hObject);

    #endregion
}
