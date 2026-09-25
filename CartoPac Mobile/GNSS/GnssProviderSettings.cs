using GNSS;

namespace CartoPac_Mobile.GNSS;

public sealed class GnssProviderSettings
{
    private const string PrefProvider = "gnss_provider_type";
    private const string PrefTcpHost = "gnss_tcp_host";
    private const string PrefTcpPort = "gnss_tcp_port";
    private const string PrefComPort = "gnss_bluetooth_com";
    private const string PrefBaudRate = "gnss_bluetooth_baud";
    private const string PrefNmeaPath = "gnss_nmea_path";

    public const string DefaultTcpHost = "127.0.0.1";
    public const string DefaultTcpPort = "77776";
    public const string DefaultComPort = "COM4";
    public const string DefaultBaudRate = "9600";
    private const string BundledNmeaFileName = "Arrow100_SBAS_SHORT.txt";

    public ProviderType ProviderType { get; set; } = ProviderType.InternalGPS;
    public string TcpHost { get; set; } = DefaultTcpHost;
    public string TcpPort { get; set; } = DefaultTcpPort;
    public string BluetoothComPort { get; set; } = DefaultComPort;
    public string BluetoothBaudRate { get; set; } = DefaultBaudRate;
    public string NmeaFilePath { get; set; } = string.Empty;

    public static GnssProviderSettings Load()
    {
        return new GnssProviderSettings
        {
            ProviderType = Enum.TryParse<ProviderType>(Preferences.Get(PrefProvider, ProviderType.InternalGPS.ToString()), out var type)
                ? type
                : ProviderType.InternalGPS,
            TcpHost = Preferences.Get(PrefTcpHost, DefaultTcpHost),
            TcpPort = Preferences.Get(PrefTcpPort, DefaultTcpPort),
            BluetoothComPort = Preferences.Get(PrefComPort, DefaultComPort),
            BluetoothBaudRate = Preferences.Get(PrefBaudRate, DefaultBaudRate),
            NmeaFilePath = Preferences.Get(PrefNmeaPath, string.Empty),
        };
    }

    public void Persist()
    {
        Preferences.Set(PrefProvider, ProviderType.ToString());
        Preferences.Set(PrefTcpHost, TcpHost);
        Preferences.Set(PrefTcpPort, TcpPort);
        Preferences.Set(PrefComPort, BluetoothComPort);
        Preferences.Set(PrefBaudRate, BluetoothBaudRate);
        Preferences.Set(PrefNmeaPath, NmeaFilePath);
    }

    public async Task<Dictionary<string, string>> ToProviderOptionsAsync()
    {
        return ProviderType switch
        {
            ProviderType.TCP => new Dictionary<string, string>
            {
                [Options.Host] = string.IsNullOrWhiteSpace(TcpHost) ? DefaultTcpHost : TcpHost.Trim(),
                [Options.Port] = string.IsNullOrWhiteSpace(TcpPort) ? DefaultTcpPort : TcpPort.Trim(),
            },
            ProviderType.Bluetooth => new Dictionary<string, string>
            {
                [Options.COMPort] = string.IsNullOrWhiteSpace(BluetoothComPort) ? DefaultComPort : BluetoothComPort.Trim(),
                [Options.BaudRate] = string.IsNullOrWhiteSpace(BluetoothBaudRate) ? DefaultBaudRate : BluetoothBaudRate.Trim(),
            },
            ProviderType.TextFile => new Dictionary<string, string>
            {
                [Options.NmeaFile] = await ResolveNmeaFilePathAsync(),
            },
            _ => throw new InvalidOperationException($"Provider '{ProviderType}' does not use connection options."),
        };
    }

    public async Task<string> ResolveNmeaFilePathAsync()
    {
        if (!string.IsNullOrWhiteSpace(NmeaFilePath) && File.Exists(NmeaFilePath))
        {
            return NmeaFilePath;
        }

        var dest = Path.Combine(FileSystem.CacheDirectory, BundledNmeaFileName);
        if (!File.Exists(dest))
        {
            await using var source = await FileSystem.OpenAppPackageFileAsync(BundledNmeaFileName);
            await using var target = File.Open(dest, FileMode.Create, FileAccess.Write, FileShare.None);
            await source.CopyToAsync(target);
        }

        return dest;
    }
}
