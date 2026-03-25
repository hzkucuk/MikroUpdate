using MikroUpdate.Service;
using MikroUpdate.Shared.Logging;

try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "MikroUpdateService";
    });

    builder.Logging.AddDiagnosticFileLogger("Service");

    builder.Services.AddHostedService<UpdateWorker>();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    // Host başlamadan servis çökerse, hatayı doğrudan dosyaya yaz
    LogStartupFailure(ex);
    throw;
}

/// <summary>
/// Servis host başlamadan çökerse, hatayı ProgramData log dosyasına yazar.
/// ILogger henüz kullanılamadığından doğrudan dosyaya yazılır.
/// </summary>
static void LogStartupFailure(Exception ex)
{
    try
    {
        string logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "MikroUpdate", "logs");

        Directory.CreateDirectory(logDir);

        string logFile = Path.Combine(logDir, $"Service_CRASH_{DateTime.Now:yyyy-MM-dd}.log");
        string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [FATAL] Service startup failed: {ex}{Environment.NewLine}";
        File.AppendAllText(logFile, entry);
    }
    catch
    {
        // Log yazılamıyorsa yapılacak bir şey yok
    }
}
