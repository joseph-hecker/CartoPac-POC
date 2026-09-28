using CartoPac_Mobile.GNSS;
using CartoPac_Mobile.Services;
using GNSS;

namespace CartoPac_Mobile;

public partial class SettingsPage : ContentPage
{
    private static readonly FilePickerFileType NmeaFileTypes = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.WinUI, [".txt", ".log"] },
        { DevicePlatform.Android, ["text/plain", "application/octet-stream"] },
        { DevicePlatform.iOS, ["public.plain-text", "public.text"] },
    });

    private LocationService? LocationService { get; set; }
    private string NmeaFile { get; set; } = string.Empty;

    public SettingsPage()
    {
        InitializeComponent();
        ProviderPicker.ItemsSource = LocationService.AvailableProviders.Values.ToList();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LocationService = ServiceHelper.GetRequiredService<LocationService>();
        LoadSettings();
        OnProviderChanged(ProviderPicker, new());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private ProviderType GetSelectedProvider()
    {
        foreach (KeyValuePair<ProviderType, string> provider in LocationService.AvailableProviders)
        {
            if (ProviderPicker.SelectedItem as string == provider.Value)
            {
                return provider.Key;
            }
        }

        return ProviderType.InternalGPS;
    }

    private void SaveSettings()
    {
        ProviderType provider = GetSelectedProvider();
        Preferences.Set(Options.Provider, provider.ToString());
        Preferences.Set(Options.Host, TcpHostEntry.Text);
        Preferences.Set(Options.Port, TcpPortEntry.Text);
        Preferences.Set(Options.COMPort, ComPortEntry.Text);
        Preferences.Set(Options.BaudRate, BaudRateEntry.Text);
        Preferences.Set(Options.NmeaFile, NmeaFile);
    }

    private void LoadSettings()
    {
        TcpHostEntry.Text = Preferences.Get(Options.Host, TcpHostEntry.Placeholder);
        TcpPortEntry.Text = Preferences.Get(Options.Port, TcpPortEntry.Placeholder);
        ComPortEntry.Text = Preferences.Get(Options.COMPort, string.Empty);
        BaudRateEntry.Text = Preferences.Get(Options.BaudRate, string.Empty);
        NmeaFile = Preferences.Get(Options.NmeaFile, string.Empty);
        NmeaFileLabel.Text = Path.GetFileName(NmeaFile);
        ProviderType provider = Enum.Parse<ProviderType>(Preferences.Get(Options.Provider, ProviderType.InternalGPS.ToString()));
        ProviderPicker.SelectedItem = LocationService.AvailableProviders[provider];
    }

    private void OnEditComplete(object? sender, EventArgs e)
    {
        SaveSettings();
    }

    private void OnProviderChanged(object? sender, EventArgs e)
    {
        ProviderType provider = GetSelectedProvider();
        TcpOptions.IsVisible = provider == ProviderType.TCP;
        BluetoothOptions.IsVisible = provider == ProviderType.Bluetooth;
        TextFileOptions.IsVisible = provider == ProviderType.TextFile;
        SaveSettings();
    }


    private async void OnPickNmeaFileClicked(object? sender, EventArgs e)
    {
        FileResult? result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select nmea file",
            FileTypes = NmeaFileTypes,
        });

        if (result is null)
        {
            return;
        }

        try
        {
            NmeaFile = await CopyPickedFileToCacheAsync(result);
            NmeaFileLabel.Text = Path.GetFileName(NmeaFile);
            SaveSettings();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("NMEA file", ex.Message, "OK");
        }
    }

    private async void OnConnectClicked(object? sender, EventArgs e)
    {
        try
        {
            LocationService?.ConnectGPS();
            await DisplayAlertAsync("GNSS", "Connection was successful.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("GNSS", $"Connection failed, {ex.Message}.", "OK");
        }
    }

    private static async Task<string> CopyPickedFileToCacheAsync(FileResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.FullPath) && File.Exists(result.FullPath))
        {
            return result.FullPath;
        }

        string dest = Path.Combine(FileSystem.CacheDirectory, result.FileName);
        using Stream source = await result.OpenReadAsync();
        using FileStream target = File.Open(dest, FileMode.Create, FileAccess.Write, FileShare.None);
        await source.CopyToAsync(target);
        return dest;
    }
}
