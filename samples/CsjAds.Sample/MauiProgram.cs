using CsjAds;

namespace CsjAds.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseCsjAds(config =>
            {
                // Replace with your actual CSJ platform App ID
                config.AppId = "5000000";
                config.AppName = "CSJ Ads Demo";
                config.IsDebug = true;

                config.Privacy.AllowPersonalizedAd = true;
                config.Privacy.AllowLocation = false;
                config.Privacy.AllowPhoneState = false;
            });

        return builder.Build();
    }
}
