namespace CsjAds.Internal;

/// <summary>
/// No-op ad service for unsupported platforms (e.g., Windows, net8.0).
/// All methods are safe to call but will not display any ads.
/// </summary>
internal sealed class NullAdService : ICsjAdService
{
    public bool IsInitialized => false;

    public void Configure(CsjAdConfiguration configuration) { }

    public Task<bool> StartAsync() => Task.FromResult(false);

    public ICsjRewardedVideoAd CreateRewardedVideoAd(string slotId)
        => throw new PlatformNotSupportedException("CSJ ads are only supported on Android and iOS.");

    public ICsjInterstitialAd CreateInterstitialAd(string slotId)
        => throw new PlatformNotSupportedException("CSJ ads are only supported on Android and iOS.");

    public ICsjSplashAd CreateSplashAd(string slotId)
        => throw new PlatformNotSupportedException("CSJ ads are only supported on Android and iOS.");
}
