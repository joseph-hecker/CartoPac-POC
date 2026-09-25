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

    private IMapCenterSource? MapCenterSource { get; set; }
    private InternalGpsHelper InternalGPSHelper { get; } = new();


    public event GPS.OnLocationUpdatedEventHandler? OnLocationUpdated
    {
        add => GPS.OnLocationUpdated += value;
        remove => GPS.OnLocationUpdated -= value;
    }

    public void RegisterMapCenterSource(IMapCenterSource source) => MapCenterSource = source;

    public void UnregisterMapCenterSource(IMapCenterSource source)
    {
        if (ReferenceEquals(MapCenterSource, source))
        {
            MapCenterSource = null;
        }
    }

    private ProviderType GetLocationProviderSettings(out Dictionary<string, string>? providerOptions)
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

    public void Connect()
    {
        ProviderType provider;
        switch (provider = GetLocationProviderSettings(out Dictionary<string, string>? options))
        {

            case ProviderType.InternalGPS:
                GPS.Connect(InternalGPSHelper);
                break;

            case ProviderType.Bluetooth:
            case ProviderType.TCP:
            case ProviderType.TextFile:
                if (options is null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                GPS.Connect(provider, options);
                break;

            case ProviderType.Map:
            default:
                if (MapCenterSource is null)
                {
                    throw new ArgumentNullException(nameof(MapCenterSource));
                }

                GPS.Connect(new MapHelper(MapCenterSource.TryGetMapCenter));
                break;
        }
    }

    public void Disconnect()
    {
        GPS.Disconnect();
    }

    public void EnableGNSS()
    {
        GPS.StartUpdatingLocation();
    }

    public void DisableGNSS()
    {
        GPS.StopUpdatingLocation();
    }
}
