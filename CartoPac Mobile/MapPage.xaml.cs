using CartoPac_Mobile.GNSS;
using CartoPac_Mobile.Services;
using GNSS;
using GNSS.Nmea;
using MapLibreNative.Maui.Geometry;
using MapLibreNative.Maui.Handlers;
using MapLibreNative.Maui.Handlers.EventArgs;
using Microsoft.Maui.Devices.Sensors;
using System.Linq.Expressions;

namespace CartoPac_Mobile;

public partial class MapPage : ContentPage, IMapCenterSource
{
    public const double FortCollinsLatitude = 40.5853;
    public const double FortCollinsLongitude = -105.0844;
    public const double InitialZoom = 12;

    private const double PuckDiameterDp = 14;
    private const double MinAccuracyRadiusMeters = 5;
    private const double FollowZoomMinimum = 14;

    /// <summary>MapLibre <c>REASON_API_GESTURE</c> — user pan, pinch, rotate, etc.</summary>
    private const int CameraReasonApiGesture = 1;

    private LocationService? LocationService { get; set; }
    private bool _initialCameraApplied;
    private bool _isMapGesturing;
    private int _initialCameraScheduleAttempts;
    private GPSData? _lastLocationFix;

    /// <summary>
    /// Controls puck visibility and whether the camera follows location updates.
    /// </summary>
    public LocationPuckMode PuckTrackingMode { get; set; } = LocationPuckMode.Show;

    public MapPage()
    {
        InitializeComponent();
        Map.StyleLoaded += (_, _) => ScheduleInitialCamera();
        Map.CameraIdleCommand = new Command(OnCameraIdle);
    }

    #region IMapCenterSource Members

    public GPSLocation GetMapCenter()
    {
        MapSpan region = Map.VisibleRegion 
            ?? throw new InvalidOperationException();

        return new GPSLocation(region.Center.Latitude, region.Center.Longitude);
    }

    #endregion

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LocationService = ServiceHelper.GetRequiredService<LocationService>();
        LocationService.RegisterMapCenterSource(this);
        LocationService.OnLocationUpdated += OnLocationUpdated;
        GnssSwitch.IsToggled = LocationService.IsDeviceGnssActive;
        if (LocationService.IsDeviceGnssActive)
        {
            GnssSwitch.IsToggled = true;
            OnGnssToggled(GnssSwitch, new(true));
        }
    }

    protected override void OnDisappearing()
    {
        LocationService?.OnLocationUpdated -= OnLocationUpdated;
        LocationService?.UnregisterMapCenterSource(this);
        base.OnDisappearing();
    }

    private void OnCameraIdle()
    {
        if (LocationService is null || LocationService.IsDeviceGnssActive)
        {
            return;
        }

        try
        {
            GPSLocation location = GetMapCenter();
            UpdateLocationPuck(location);
            CoordinatesLabel.Text = $"Lat: {location.Latitude:F6}\nLon: {location.Longitude:F6}";
        }
        catch (Exception) { }
    }

    private async void OnGnssToggled(object? sender, ToggledEventArgs e)
    {
        try
        {
            if (e.Value) LocationService?.EnableGNSS();
            else LocationService?.DisableGNSS();
        }
        catch (InvalidOperationException ex) when (ex.Message == $"GPS provider is not connected. Call {nameof(GPS.Connect)}() first.")
        {
            LocationService?.ConnectGPS();
            if (e.Value) LocationService?.EnableGNSS();
            else LocationService?.DisableGNSS();
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
            UpdateLocationPuck(data);
            CoordinatesLabel.Text = $"Lat: {data.Latitude:F6}\nLon: {data.Longitude:F6}";
        });
    }

    public void UpdateLocationPuck(GPSLocation location)
    {
        IMapLibreMapController? controller = GetMapController();
        if (controller is null)
        {
            return;
        }

        var (x, y) = controller.LatLngToScreenPoint(location.Latitude, location.Longitude);
        if (double.IsNaN(x) || double.IsNaN(y))
        {
            LocationPuck.IsVisible = false;
            LocationPuckAccuracyRing.IsVisible = false;
            return;
        }

        Location center = new(location.Latitude, location.Longitude);

        double zoom = controller.GetZoom();
        double latRad = location.Latitude * Math.PI / 180.0;
        double metersPerPixel = 40075016.686 * Math.Abs(Math.Cos(latRad)) / (512 * Math.Pow(2, zoom));
        double puckRadiusMeters = PuckDiameterDp / 2.0 * DeviceDisplay.MainDisplayInfo.Density * metersPerPixel;

        LocationPuck.IsVisible = true;
        LocationPuck.Center = center;
        LocationPuck.Radius = Distance.FromMeters(Math.Max(puckRadiusMeters, 0.5));

        LocationPuckAccuracyRing.IsVisible = true;
        LocationPuckAccuracyRing.Center = center;
        LocationPuckAccuracyRing.Radius = Distance.FromMeters(MinAccuracyRadiusMeters);
    }

    public void UpdateLocationPuck(GPSData data)
    {
        Location center = new(data.Latitude, data.Longitude);

        LocationPuck.IsVisible = true;
        LocationPuck.Center = center;
        LocationPuck.Radius = Distance.FromMeters(data.EstimatedAccuracy);

        LocationPuckAccuracyRing.IsVisible = true;
        LocationPuckAccuracyRing.Center = center;
        LocationPuckAccuracyRing.Radius = Distance.FromMeters(data.EstimatedAccuracy);
        
        //UpdatePuckScreenPosition();
    }

    private void ScheduleInitialCamera()
    {
        //if (_initialCameraApplied)
        //{
        //    return;
        //}
        //
        //if (_initialCameraScheduleAttempts++ > 30)
        //{
        //    return;
        //}
        
        var controller = GetMapController();
        if (controller is null)
        {
            //Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), ScheduleInitialCamera);
            return;
        }

        //_initialCameraApplied = true;
        controller.JumpTo(FortCollinsLatitude, FortCollinsLongitude, InitialZoom);

        //if (_lastLocationFix is { } fix)
        //{
        //    UpdateLocationPuck(fix, PuckTrackingMode);
        //}
    }

    private IMapLibreMapController? GetMapController()
    {
        return Map.Handler is MapLibreMapHandler handler ? handler.Controller : null;
    }
}
