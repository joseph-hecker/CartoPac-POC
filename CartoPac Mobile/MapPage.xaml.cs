using CartoPac_Mobile.GNSS;
using CartoPac_Mobile.Services;
using GNSS;
using GNSS.Nmea;
using GNSS.Workflows;
using MapLibreNative.Maui.Geometry;
using MapLibreNative.Maui.Handlers;
using MapLibreNative.Maui.Handlers.Overlays;

namespace CartoPac_Mobile;

public partial class MapPage : ContentPage, IMapCenterSource
{
    private LocationService? LocationService { get; set; }

    public MapPage()
    {
        InitializeComponent();
        Map.CameraIdle += OnCameraIdle;
        Map.StyleLoaded += (_, _) => ScheduleInitialCamera();
    }

    #region IMapCenterSource Members

    public GPSLocation GetMapCenter()
    {
        MapSpan region = Map.VisibleRegion 
            ?? throw new InvalidOperationException();

        return new GPSLocation(region.Center.Latitude, region.Center.Longitude, FixQuality: FixQuality.Simulation, UTC: DateTime.UtcNow);
    }

    #endregion

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LocationService = ServiceHelper.GetRequiredService<LocationService>();
        LocationService.RegisterMapCenterSource(this);
        GnssSwitch.IsToggled = LocationService.IsDeviceGnssActive;
        if (LocationService.IsDeviceGnssActive)
        {
            GnssSwitch.IsToggled = true;
            OnGnssToggled(GnssSwitch, new(true));
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private void OnCameraIdle(object? sender, EventArgs e)
    {
        if (LocationService is null || LocationService.IsDeviceGnssActive)
        {
            return;
        }

        GPSLocation location = GetMapCenter();
        UpdateLocationPuck(location);
    }

    private async void OnCollectVertexClicked(object? sender, EventArgs e)
    {
        if (LocationService is null)
        {
            return;
        }

        try
        {
            CollectVertexButton.IsEnabled = false;
            GPSData data = await LocationService.CollectVertex();
            Map.Add(new Pin()
            {
                Location = new Location(data.Latitude, data.Longitude),
                TintColor = Colors.Crimson,
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Vertex Collection Failed", ex.Message, "OK");
        }
        finally
        {
            CollectVertexButton.IsEnabled = true;
        }
    }

    private async void OnGnssToggled(object? sender, ToggledEventArgs e)
    {
        bool toggleEnabled = e.Value;
        try
        {
            if (toggleEnabled)
            {
                LocationService?.EnableGNSS();
                LocationService?.OnLocationUpdated += OnLocationUpdated;
            }
            else
            {
                LocationService?.DisableGNSS();
                LocationService?.OnLocationUpdated -= OnLocationUpdated;
            }
        }
        catch (Exception)
        {
            try
            {
                // GNSS might not be connected, so try connecting first before enabling GNSS.
                LocationService?.ConnectGNSS();
                if (toggleEnabled)
                {
                    LocationService?.EnableGNSS();
                    LocationService?.OnLocationUpdated += OnLocationUpdated;
                }
                else
                {
                    LocationService?.DisableGNSS();
                    LocationService?.OnLocationUpdated -= OnLocationUpdated;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("GNSS", ex.Message, "OK");
            }
        }
    }

    private void OnLocationUpdated(GPSData data)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateLocationPuck(data);
            CoordinatesLabel.Text = $"Lat: {data.Latitude:F8}\nLon: {data.Longitude:F8}";
        });
    }

    public void UpdateLocationPuck(GPSData data)
    {
        IMapLibreMapController? controller = GetMapController();
        if (controller is null)
        {
            return;
        }

        Location center = new()
        {
            Latitude = data.Latitude,
            Longitude = data.Longitude,
            Altitude = data.Altitude,
            Speed = data.Speed,
            Course = data.Course,
            Accuracy = data.EstimatedAccuracy,
            IsFromMockProvider = data.FixQuality == FixQuality.Simulation,
            Timestamp = new DateTimeOffset(data.UTC),
        };

        LocationPuck.IsVisible = true;
        LocationPuck.Location = center;
    }

    public void UpdateLocationPuck(GPSLocation location)
    {
        UpdateLocationPuck(new GPSData(location));
    }

    private void ScheduleInitialCamera()
    {
        IMapLibreMapController? controller = GetMapController();
        if (controller is null)
        {
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), ScheduleInitialCamera);
            return;
        }

        double fortCollinsLatitude = 40.5853;
        double fortCollinsLongitude = -105.0844;
        double zoom = 12;
        controller.JumpTo(fortCollinsLatitude, fortCollinsLongitude, zoom);
    }

    private IMapLibreMapController? GetMapController()
    {
        return Map.Handler is MapLibreMapHandler handler ? handler.Controller : null;
    }
}
