using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace CsjAds;

/// <summary>
/// Extension methods for registering CSJ ads in a MAUI application.
/// </summary>
public static class CsjAdsExtensions
{
    /// <summary>
    /// Registers the CSJ ads service, configuration, and MAUI handlers.
    /// </summary>
    /// <param name="builder">The MAUI app builder.</param>
    /// <param name="configure">Action to configure the CSJ ad settings.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.UseCsjAds(config =>
    /// {
    ///     config.AppId = "5000000";
    ///     config.AppName = "MyApp";
    ///     config.IsDebug = true;
    /// });
    /// </code>
    /// </example>
    public static MauiAppBuilder UseCsjAds(
        this MauiAppBuilder builder,
        Action<CsjAdConfiguration> configure)
    {
        var config = new CsjAdConfiguration();
        configure(config);

        if (string.IsNullOrWhiteSpace(config.AppId))
            throw new ArgumentException("CsjAdConfiguration.AppId is required.");

        builder.Services.AddSingleton(config);

#if ANDROID
        builder.Services.AddSingleton<ICsjAdService, Platforms.Android.CsjAdService>();
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<CsjBannerView, Platforms.Android.Handlers.CsjBannerViewHandler>();
            handlers.AddHandler<CsjFeedAdView, Platforms.Android.Handlers.CsjFeedAdViewHandler>();
        });
#elif IOS && !CSJ_NO_NATIVE
        builder.Services.AddSingleton<ICsjAdService, Platforms.iOS.CsjAdService>();
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<CsjBannerView, Platforms.iOS.Handlers.CsjBannerViewHandler>();
            handlers.AddHandler<CsjFeedAdView, Internal.NullFeedAdViewHandler>();
        });
#else
        // No-op on unsupported platforms or iOS simulator — register stub service + handler
        builder.Services.AddSingleton<ICsjAdService, Internal.NullAdService>();
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<CsjBannerView, Internal.NullBannerViewHandler>();
            handlers.AddHandler<CsjFeedAdView, Internal.NullFeedAdViewHandler>();
        });
#endif

        return builder;
    }
}
