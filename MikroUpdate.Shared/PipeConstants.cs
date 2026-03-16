namespace MikroUpdate.Shared;

/// <summary>
/// Named Pipe haberleşme sabitleri.
/// </summary>
public static class PipeConstants
{
    /// <summary>Named Pipe adı.</summary>
    public const string PipeName = "MikroUpdateServicePipe";

    /// <summary>Pipe bağlantı zaman aşımı (ms).</summary>
    public const int ConnectionTimeoutMs = 5000;

    /// <summary>Periyodik versiyon kontrol aralığı (dakika).</summary>
    public const int CheckIntervalMinutes = 30;
}
