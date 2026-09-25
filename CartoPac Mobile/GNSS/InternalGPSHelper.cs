using GNSS;
using GNSS.Providers;

namespace CartoPac_Mobile.GNSS;

public sealed class InternalGpsHelper : IInternalGPSHelper
{
    private TimeSpan PollInterval { get; } = TimeSpan.FromMilliseconds(1000);
    private CancellationTokenSource? Source { get; set; }
    private Action<GPSLocation>? OnNewLocation { get; set; }

    public async Task<bool> EnsurePermissionsAsync(CancellationToken ct = default)
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        return status == PermissionStatus.Granted;
    }

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

    private async Task PollAsync(CancellationToken ct)
    {
        if (!await EnsurePermissionsAsync(ct))
        {
            return;
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
                        location.Accuracy));
                }

                Source?.Token.WaitHandle.WaitOne(PollInterval);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (PermissionException)
            {
                break;
            }
            catch (FeatureNotSupportedException)
            {
                break;
            }
        }
    }
}
