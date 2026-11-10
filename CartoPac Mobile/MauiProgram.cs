using CartoPac_Mobile.GNSS;
using MapLibreNative.Maui.Handlers;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CartoPac_Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            string gnssLogPath = Path.Combine(FileSystem.AppDataDirectory, "gnss.log");
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(gnssLogPath, shared: true, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = MauiApp.CreateBuilder();
            builder.Services.AddSingleton<LocationService>();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddHandler(typeof(MapLibreMap), typeof(MapLibreMapHandler));
                });

            builder.Logging.AddSerilog(logger: null, dispose: true);
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
