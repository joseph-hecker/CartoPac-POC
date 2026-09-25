using CartoPac_Mobile.GNSS;
using CartoPac_Mobile.Services;
using GNSS;
using MapLibreNative.Maui.Handlers;
using System.Globalization;

namespace CartoPac_Mobile;

public partial class MapPage : ContentPage, IMapCenterSource
{
    public const double FortCollinsLatitude = 40.5853;
    public const double FortCollinsLongitude = -105.0844;
    public const double InitialZoom = 12;

    private LocationService? LocationService { get; set; }
    private bool _initialCameraApplied;
    private bool _isMapGesturing;
    private int _initialCameraScheduleAttempts;
    private bool _subscribedToGps;

    public MapPage()
    {
        InitializeComponent();
        Map.StyleLoaded += (_, _) => ScheduleInitialCamera();
    }

    #region IMapCenterSource Members

    public GPSLocation? TryGetMapCenter()
    {
        var region = Map.VisibleRegion ?? Map.GetVisibleRegion();
        if (_isMapGesturing || region is null)
        {
            return null;
        }

        return new GPSLocation(region.Center.Latitude, region.Center.Longitude);
    }

    #endregion

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LocationService = ServiceHelper.GetRequiredService<LocationService>();
        LocationService.RegisterMapCenterSource(this);
    }

    protected override void OnDisappearing()
    {
        LocationService?.UnregisterMapCenterSource(this);
        base.OnDisappearing();
    }

    private async void OnGnssToggleClicked(object? sender, EventArgs e)
    {
        if (LocationService is null)
        {
            return;
        }

        try
        {
            LocationService.Connect();
            LocationService.EnableGNSS();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("GNSS", ex.Message, "OK");
        }
    }

    private void OnLocationUpdated(GPSData data)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var controller = GetMapController();
            if (controller is null)
            {
                return;
            }
            controller.UpdateGpsLocation(data.Latitude, data.Longitude, 0, (float)data.EstimatedAccuracy);
        });
    }

    private void ScheduleInitialCamera()
    {
        if (_initialCameraApplied)
        {
            return;
        }

        if (_initialCameraScheduleAttempts++ > 30)
        {
            return;
        }

        var controller = GetMapController();
        if (controller is null)
        {
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), ScheduleInitialCamera);
            return;
        }

        _initialCameraApplied = true;
        controller.FollowLocation = false;
        controller.JumpTo(FortCollinsLatitude, FortCollinsLongitude, InitialZoom);
    }


    private IMapLibreMapController? GetMapController()
    {
        return Map.Handler is MapLibreMapHandler handler ? handler.Controller : null;
    }
}
