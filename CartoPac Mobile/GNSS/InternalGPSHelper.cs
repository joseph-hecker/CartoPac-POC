using GNSS;
using GNSS.Nmea;
using GNSS.Providers;

namespace CartoPac_Mobile.GNSS;

public sealed class InternalGpsHelper : IInternalGPSHelper
{
    private TimeSpan PollInterval { get; } = TimeSpan.FromMilliseconds(500);
    private CancellationTokenSource? Source { get; set; }
    private Action<GPSLocation>? OnNewLocation { get; set; }

    public void Start(Action<GPSLocation> onNewLocation)
    {
        Stop();
        OnNewLocation = onNewLocation;
        Source = new CancellationTokenSource();
        _ = PollAsync(Source.Token);
    }

    public void Stop()
    {
        Source?.Cancel();
        Source?.Dispose();
        Source = null;
        OnNewLocation = null;
    }

    public async Task<bool> EnsurePermissionsAsync(CancellationToken ct = default)
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        return status == PermissionStatus.Granted;
    }

    private async Task PollAsync(CancellationToken ct)
    {
        if (!await EnsurePermissionsAsync(ct))
        {
            throw new PermissionException("Location permissions are not granted.");
        }

        GeolocationRequest request = new(GeolocationAccuracy.Best);
        while (!ct.IsCancellationRequested)
        {
            try
            {
                Location? location = await Geolocation.GetLocationAsync(request, ct);
                if (location is not null && OnNewLocation is not null)
                {
                    OnNewLocation.Invoke(new GPSLocation(
                        location.Latitude,
                        location.Longitude,
                        location.Altitude,
                        location.Speed,
                        location.Course,
                        location.Accuracy,
                        location.IsFromMockProvider ? FixQuality.Simulation : FixQuality.GPS,
                        location.Timestamp.DateTime));
                }

                Source?.Token.WaitHandle.WaitOne(PollInterval);
            }
            catch (Exception)
            {
                Stop();
                break;
            }
        }
    }
}
