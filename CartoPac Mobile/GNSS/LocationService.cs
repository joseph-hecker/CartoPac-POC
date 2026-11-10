using GNSS;
using GNSS.Workflows;

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

    private MapWorkflow MapGPS { get => field ?? throw new InvalidOperationException("Map center source has not been registered yet."); set; }
    private GNSSWorkflow? DeviceGPS { get; set; }
    private InternalGpsHelper InternalGPSHelper { get; } = new();

    public bool IsDeviceGnssActive { get; private set; }

    public event GPS.OnLocationUpdatedEventHandler? OnLocationUpdated
    {
        add => GPS.OnLocationUpdated += value;
        remove => GPS.OnLocationUpdated -= value;
    }

    public void RegisterMapCenterSource(IMapCenterSource source)
    {
        MapGPS = new(source);
    }

    public void ConnectGNSS()
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
        }

        DeviceGPS = new StandardGPSWorkflow(ct => Task.CompletedTask);
    }

    public void DisconnectGNSS()
    {
        IsDeviceGnssActive = false;
        DeviceGPS = null;
        GPS.Disconnect();
    }

    public void EnableGNSS()
    {
        if (DeviceGPS is not null)
        {
            IsDeviceGnssActive = true;
            GPS.StartUpdatingLocation();
        }
    }

    public void DisableGNSS()
    {
        IsDeviceGnssActive = false;
        GPS.StopUpdatingLocation();
    }

    public async Task<GPSData> CollectVertex()
    {
        if (IsDeviceGnssActive && DeviceGPS is not null)
        {
            return await DeviceGPS.CollectVertexAsync();
        }

        return await MapGPS.CollectVertexAsync();
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
