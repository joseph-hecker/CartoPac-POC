namespace CartoPac_Mobile.Services;

public static class ServiceHelper
{
    public static T GetRequiredService<T>() where T : notnull =>
        GetServiceProvider().GetRequiredService<T>();

    private static IServiceProvider GetServiceProvider()
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        if (services is null)
        {
            throw new InvalidOperationException("Application services are not available yet.");
        }

        return services;
    }
}
