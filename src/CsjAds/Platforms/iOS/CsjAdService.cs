#if IOS && !CSJ_NO_NATIVE
using CsjAds.Internal;
using CsjAds.iOS.Binding;

namespace CsjAds.Platforms.iOS;

/// <summary>
/// iOS implementation of <see cref="ICsjAdService"/>.
/// Delegates to the native Objective-C wrapper which internally uses the Ads-CN SDK.
/// </summary>
internal sealed class CsjAdService : ICsjAdService
{
    private CsjAdConfiguration? _configuration;
    private bool _isConfigured;

    public bool IsInitialized { get; private set; }

    public void Configure(CsjAdConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Call the native wrapper's init (pre-consent step)
        CsjSdkWrapper.Configure(
            configuration.AppId,
            configuration.AppName,
            configuration.IsDebug,
            configuration.Privacy.AllowPersonalizedAd,
            configuration.Privacy.AllowLocation);

        _isConfigured = true;
    }

    public Task<bool> StartAsync()
    {
        if (!_isConfigured)
            throw new InvalidOperationException("Call Configure() before StartAsync().");

        if (IsInitialized)
            return Task.FromResult(true);

        var tcs = new TaskCompletionSource<bool>();

        CsjSdkWrapper.Start((success, code, message) =>
        {
            IsInitialized = success;
            MainThreadDispatcher.Dispatch(() => tcs.TrySetResult(success));
        });

        return tcs.Task;
    }

    public ICsjRewardedVideoAd CreateRewardedVideoAd(string slotId)
    {
        EnsureInitialized();
        return new CsjRewardedVideoAdImpl(slotId);
    }

    public ICsjInterstitialAd CreateInterstitialAd(string slotId)
    {
        EnsureInitialized();
        return new CsjInterstitialAdImpl(slotId);
    }

    public ICsjSplashAd CreateSplashAd(string slotId)
    {
        EnsureInitialized();
        return new CsjSplashAdImpl(slotId);
    }

    private void EnsureInitialized()
    {
        if (!IsInitialized)
            throw new InvalidOperationException(
                "CSJ SDK is not initialized. Call Configure() and await StartAsync() first.");
    }
}
#endif
