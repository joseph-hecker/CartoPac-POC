using GNSS;

namespace CartoPac_Mobile.GNSS;

public sealed class LocationService
{
    public static readonly Dictionary<ProviderType, string> AvailableProviders = new()
    {
        { ProviderType.InternalGPS, "Internal GPS" },
        { ProviderType.Bluetooth, "Bluetooth" },
        { ProviderType.TCP, "TCP" },
        { ProviderType.TextFile, "Text File" }
    };

    private static GPS _deviceGPS = new();
    private static GPS _mapGPS = new();
    private GPS GPS { get; set; } = _mapGPS;

    private IMapCenterSource? MapCenterSource { get; set; }
    private InternalGpsHelper InternalGPSHelper { get; } = new();

    public bool IsDeviceGnssActive { get; set; }

    public event GPS.OnLocationUpdatedEventHandler? OnLocationUpdated
    {
        add => GPS.OnLocationUpdated += value;
        remove => GPS.OnLocationUpdated -= value;
    }

    public void RegisterMapCenterSource(IMapCenterSource source)
    {
        MapCenterSource = source;
        _mapGPS.Connect(new MapHelper(() => MapCenterSource.GetMapCenter()));
    }

    public void UnregisterMapCenterSource(IMapCenterSource source)
    {
        _mapGPS.Disconnect();
        if (ReferenceEquals(MapCenterSource, source))
        {
            MapCenterSource = null;
        }
    }

    public void ConnectGPS()
    {
        ProviderType provider;
        switch (provider = GetLocationProviderSettings(out Dictionary<string, string>? options))
        {

            case ProviderType.InternalGPS:
                _deviceGPS.Connect(InternalGPSHelper);
                break;

            case ProviderType.Bluetooth:
            case ProviderType.TCP:
            case ProviderType.TextFile:
                if (options is null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                _deviceGPS.Connect(provider, options);
                break;
        }
    }

    public void DisconnectGPS()
    {
        _deviceGPS.Disconnect();
    }

    public void EnableGNSS()
    {
        IsDeviceGnssActive = true;
        _mapGPS.StopUpdatingLocation();
        _deviceGPS.StartUpdatingLocation();
        GPS = _deviceGPS;
    }

    public void DisableGNSS()
    {
        IsDeviceGnssActive = false;
        _deviceGPS.StopUpdatingLocation();
        _mapGPS.StartUpdatingLocation();
        GPS = _mapGPS;
    }

    private static ProviderType GetLocationProviderSettings(out Dictionary<string, string>? providerOptions)
    {
        ProviderType provider = Enum.Parse<ProviderType>(Preferences.Get(Options.Provider, string.Empty));
        providerOptions = provider switch
        {
            ProviderType.TCP => new Dictionary<string, string>
            {
                { Options.Host, Preferences.Get(Options.Host, string.Empty) },
                { Options.Port, Preferences.Get(Options.Port, string.Empty) },
            },
            ProviderType.Bluetooth => new Dictionary<string, string>
            {
                { Options.COMPort, Preferences.Get(Options.COMPort, string.Empty) },
                { Options.BaudRate, Preferences.Get(Options.BaudRate, string.Empty) },
            },
            ProviderType.TextFile => new Dictionary<string, string>
            {
                { Options.NmeaFile, Preferences.Get(Options.NmeaFile, string.Empty) },
            },
            _ => null
        };

        return provider;
    }
}
