using GNSS;
using GNSS.Providers;

namespace CartoPac_Mobile.GNSS;
/// <summary>
/// Polls a host-supplied callback for the map view center (e.g. from <see cref="MapPage"/>).
/// </summary>
public sealed class MapHelper(Func<GPSLocation?> tryGetMapCenter) : IMapHelper
{
    private TimeSpan PollInterval { get; } = TimeSpan.FromMilliseconds(100);
    private Timer? PollTimer { get; set; }
    private Action<GPSLocation>? OnNewLocation { get; set; }

    public void Start(Action<GPSLocation> onNewLocation)
    {
        Stop();
        OnNewLocation = onNewLocation;
        PollTimer = new Timer(PollMapCenter, null, TimeSpan.Zero, PollInterval);
    }

    public void Stop()
    {
        PollTimer?.Dispose();
        PollTimer = null;
        OnNewLocation = null;
    }

    private void PollMapCenter(object? state)
    {
        GPSLocation? location = tryGetMapCenter();
        if (location is not null && OnNewLocation is not null)
        {
            OnNewLocation.Invoke(location.Value);
        }
    }
}
