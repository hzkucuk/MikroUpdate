using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MikroUpdate.Service.Services;

/// <summary>
/// SYSTEM oturumundan (Session 0) aktif kullanıcının masaüstü oturumunda process başlatır.
/// WTSQueryUserToken + CreateProcessAsUser P/Invoke pattern kullanır.
/// Başarısız olursa explorer.exe fallback yöntemi dener.
/// </summary>
[SupportedOSPlatform("windows")]
internal static partial class UserSessionLauncher
{
    /// <summary>
    /// Aktif konsol oturumundaki kullanıcı olarak belirtilen uygulamayı başlatır.
    /// İlk olarak CreateProcessAsUser dener, başarısız olursa explorer.exe fallback kullanır.
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

        logger.LogInformation("LaunchInUserSession başlatılıyor. Exe: {ExePath}", exePath);

        // Yöntem 1: CreateProcessAsUser ile doğrudan başlat
        bool launched = TryCreateProcessAsUser(exePath, logger);

        if (launched)
        {
            return true;
        }

        // Yöntem 2: explorer.exe üzerinden başlat (fallback)
        logger.LogWarning("CreateProcessAsUser başarısız, explorer.exe fallback deneniyor...");

        return TryLaunchViaExplorer(exePath, logger);
    }

    /// <summary>
    /// CreateProcessAsUser ile kullanıcı oturumunda process başlatır.
    /// Win32 API'nin lpCommandLine parametresi writable buffer gerektirir.
    /// </summary>
    private static bool TryCreateProcessAsUser(string exePath, ILogger logger)
    {
        nint userToken = nint.Zero;
        nint duplicateToken = nint.Zero;
        nint environment = nint.Zero;
        nint cmdLinePtr = nint.Zero;
        nint desktopPtr = nint.Zero;
        nint workDirPtr = nint.Zero;

        try
        {
            // 1. Aktif konsol oturumunu bul
            uint sessionId = WTSGetActiveConsoleSessionId();

            if (sessionId == 0xFFFFFFFF)
            {
                logger.LogError("Aktif konsol oturumu bulunamadı (0xFFFFFFFF).");

                return false;
            }

            logger.LogInformation("Aktif konsol oturumu: {SessionId}", sessionId);

            // 2. Oturumdaki kullanıcının token'ını al
            if (!WTSQueryUserToken(sessionId, out userToken))
            {
                int error = Marshal.GetLastWin32Error();
                logger.LogError("WTSQueryUserToken başarısız. Session: {Session}, Win32: {Error}", sessionId, error);

                return false;
            }

            logger.LogInformation("Kullanıcı token'ı alındı.");

            // 3. Token'ı duplicate et — SecurityImpersonation kullanıcı bağlamı için gerekli
            if (!DuplicateTokenEx(
                    userToken,
                    MAXIMUM_ALLOWED,
                    nint.Zero,
                    SecurityImpersonationLevel.SecurityImpersonation,
                    TokenType.TokenPrimary,
                    out duplicateToken))
            {
                int error = Marshal.GetLastWin32Error();
                logger.LogError("DuplicateTokenEx başarısız. Win32: {Error}", error);

                return false;
            }

            logger.LogInformation("Token duplicate edildi (SecurityImpersonation).");

            // 4. Kullanıcı ortam değişkenlerini oluştur
            if (!CreateEnvironmentBlock(out environment, duplicateToken, false))
            {
                int error = Marshal.GetLastWin32Error();
                logger.LogWarning("CreateEnvironmentBlock başarısız (Win32: {Error}), ortam değişkensiz devam.", error);
                environment = nint.Zero;
            }

            // 5. Writable buffer'lar hazırla (Win32 CreateProcessAsUser lpCommandLine'ı değiştirebilir)
            string cmdLine = $"\"{exePath}\"";
            cmdLinePtr = Marshal.StringToHGlobalUni(cmdLine);
            desktopPtr = Marshal.StringToHGlobalUni("winsta0\\default");

            string? workDir = Path.GetDirectoryName(exePath);

            if (!string.IsNullOrEmpty(workDir))
            {
                workDirPtr = Marshal.StringToHGlobalUni(workDir);
            }

            STARTUPINFO startupInfo = new()
            {
                cb = Marshal.SizeOf<STARTUPINFO>(),
                lpDesktop = desktopPtr,
                dwFlags = STARTF_USESHOWWINDOW,
                wShowWindow = SW_SHOWNORMAL
            };

            uint creationFlags = CREATE_UNICODE_ENVIRONMENT | CREATE_NEW_CONSOLE;

            // 6. Process başlat (raw nint overload — writable buffer garantisi)
            if (!CreateProcessAsUserW(
                    duplicateToken,
                    nint.Zero,
                    cmdLinePtr,
                    nint.Zero,
                    nint.Zero,
                    false,
                    creationFlags,
                    environment,
                    workDirPtr,
                    ref startupInfo,
                    out PROCESS_INFORMATION processInfo))
            {
                int error = Marshal.GetLastWin32Error();
                logger.LogError("CreateProcessAsUser başarısız. Exe: {ExePath}, Win32: {Error}", exePath, error);

                return false;
            }

            uint pid = processInfo.dwProcessId;
            logger.LogInformation("CreateProcessAsUser başarılı, PID: {PID}. Process hayatta mı kontrol ediliyor (2sn)...", pid);

            // 7. Process hayatta mı kontrol et (2 saniye bekle)
            uint waitResult = WaitForSingleObject(processInfo.hProcess, 2000);

            CloseHandle(processInfo.hProcess);
            CloseHandle(processInfo.hThread);

            if (waitResult == WAIT_OBJECT_0)
            {
                // Process 2 saniye içinde sonlandı — bir sorun var
                logger.LogError("Process başlatıldı (PID: {PID}) ama 2 saniye içinde sonlandı!", pid);

                return false;
            }

            // WAIT_TIMEOUT (0x102) = process hâlâ çalışıyor — başarılı
            logger.LogInformation("Process çalışıyor (PID: {PID}). Başlatma başarılı.", pid);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TryCreateProcessAsUser hatası. Exe: {ExePath}", exePath);

            return false;
        }
        finally
        {
            if (workDirPtr != nint.Zero) Marshal.FreeHGlobal(workDirPtr);
            if (desktopPtr != nint.Zero) Marshal.FreeHGlobal(desktopPtr);
            if (cmdLinePtr != nint.Zero) Marshal.FreeHGlobal(cmdLinePtr);
            if (environment != nint.Zero) DestroyEnvironmentBlock(environment);
            if (duplicateToken != nint.Zero) CloseHandle(duplicateToken);
            if (userToken != nint.Zero) CloseHandle(userToken);
        }
    }

    /// <summary>
    /// Fallback: explorer.exe üzerinden uygulamayı kullanıcı oturumunda başlatır.
    /// Explorer zaten kullanıcı oturumunda çalıştığı için CreateProcessAsUser'dan
    /// bağımsız olarak shell entegrasyonu ile uygulamayı açabilir.
    /// </summary>
    private static bool TryLaunchViaExplorer(string exePath, ILogger logger)
    {
        nint userToken = nint.Zero;
        nint duplicateToken = nint.Zero;
        nint environment = nint.Zero;
        nint cmdLinePtr = nint.Zero;
        nint desktopPtr = nint.Zero;

        try
        {
            uint sessionId = WTSGetActiveConsoleSessionId();

            if (sessionId == 0xFFFFFFFF)
            {
                logger.LogError("Explorer fallback: Aktif oturum yok.");

                return false;
            }

            if (!WTSQueryUserToken(sessionId, out userToken))
            {
                int error = Marshal.GetLastWin32Error();
                logger.LogError("Explorer fallback: WTSQueryUserToken başarısız. Win32: {Error}", error);

                return false;
            }

            if (!DuplicateTokenEx(userToken, MAXIMUM_ALLOWED, nint.Zero,
                    SecurityImpersonationLevel.SecurityImpersonation,
                    TokenType.TokenPrimary, out duplicateToken))
            {
                int error = Marshal.GetLastWin32Error();
                logger.LogError("Explorer fallback: DuplicateTokenEx başarısız. Win32: {Error}", error);

                return false;
            }

            CreateEnvironmentBlock(out environment, duplicateToken, false);

            // explorer.exe + uygulama yolu: shell üzerinden başlatır
            string explorerPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");

            string cmdLine = $"\"{explorerPath}\" \"{exePath}\"";
            cmdLinePtr = Marshal.StringToHGlobalUni(cmdLine);
            desktopPtr = Marshal.StringToHGlobalUni("winsta0\\default");

            STARTUPINFO si = new()
            {
                cb = Marshal.SizeOf<STARTUPINFO>(),
                lpDesktop = desktopPtr
            };

            uint flags = CREATE_UNICODE_ENVIRONMENT;

            if (!CreateProcessAsUserW(duplicateToken, nint.Zero, cmdLinePtr,
                    nint.Zero, nint.Zero, false, flags,
                    environment != nint.Zero ? environment : nint.Zero,
                    nint.Zero, ref si, out PROCESS_INFORMATION pi))
            {
                int error = Marshal.GetLastWin32Error();
                logger.LogError("Explorer fallback: CreateProcessAsUser başarısız. Win32: {Error}", error);

                return false;
            }

            CloseHandle(pi.hProcess);
            CloseHandle(pi.hThread);

            logger.LogInformation("Explorer fallback başarılı. PID: {PID}, Exe: {ExePath}", pi.dwProcessId, exePath);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TryLaunchViaExplorer hatası.");

            return false;
        }
        finally
        {
            if (desktopPtr != nint.Zero) Marshal.FreeHGlobal(desktopPtr);
            if (cmdLinePtr != nint.Zero) Marshal.FreeHGlobal(cmdLinePtr);
            if (environment != nint.Zero) DestroyEnvironmentBlock(environment);
            if (duplicateToken != nint.Zero) CloseHandle(duplicateToken);
            if (userToken != nint.Zero) CloseHandle(userToken);
        }
    }

    #region P/Invoke Declarations

    private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
    private const uint CREATE_NEW_CONSOLE = 0x00000010;
    private const uint MAXIMUM_ALLOWED = 0x02000000;
    private const int STARTF_USESHOWWINDOW = 0x00000001;
    private const short SW_SHOWNORMAL = 1;
    private const uint WAIT_OBJECT_0 = 0x00000000;

    private enum SecurityImpersonationLevel
    {
        SecurityImpersonation = 2
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

    /// <summary>
    /// CreateProcessAsUserW — tüm parametreler nint olarak (writable buffer garantisi).
    /// Win32 API'si lpCommandLine parametresini değiştirebileceği için string yerine nint kullanılır.
    /// </summary>
    [LibraryImport("advapi32.dll", EntryPoint = "CreateProcessAsUserW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CreateProcessAsUserW(
        nint hToken,
        nint lpApplicationName,
        nint lpCommandLine,
        nint lpProcessAttributes,
        nint lpThreadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles,
        uint dwCreationFlags,
        nint lpEnvironment,
        nint lpCurrentDirectory,
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

    [LibraryImport("kernel32.dll")]
    private static partial uint WaitForSingleObject(nint hHandle, uint dwMilliseconds);

    #endregion
}
