#if ANDROID
using Android.App;
using CsjAds.Internal;

namespace CsjAds.Platforms.Android;

/// <summary>
/// Android implementation of <see cref="ICsjAdService"/>.
/// Delegates to the native Java wrapper which internally uses the CSJ SDK.
/// </summary>
internal sealed class CsjAdService : ICsjAdService
{
    private CsjAdConfiguration? _configuration;
    private bool _isConfigured;

    public bool IsInitialized { get; private set; }

    public void Configure(CsjAdConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        var context = global::Android.App.Application.Context;

        // Call the native wrapper's init (pre-consent step)
        // This sets up TTAdConfig without starting the SDK
        Com.Csjads.Wrapper.CsjSdkWrapper.Init(
            context,
            configuration.AppId,
            configuration.AppName,
            configuration.IsDebug,
            configuration.Privacy.AllowPersonalizedAd,
            configuration.Privacy.AllowLocation,
            configuration.Privacy.AllowPhoneState,
            configuration.Privacy.AllowWriteExternal,
            configuration.Privacy.CustomDeviceId);

        _isConfigured = true;
    }

    public Task<bool> StartAsync()
    {
        if (!_isConfigured)
            throw new InvalidOperationException("Call Configure() before StartAsync().");

        if (IsInitialized)
            return Task.FromResult(true);

        var tcs = new TaskCompletionSource<bool>();

        Com.Csjads.Wrapper.CsjSdkWrapper.Start(new SdkStartCallback(success =>
        {
            IsInitialized = success;
            tcs.TrySetResult(success);
        }));

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

    /// <summary>
    /// Callback adapter for the native SDK start result.
    /// </summary>
    private sealed class SdkStartCallback : Java.Lang.Object, Com.Csjads.Wrapper.ICsjSdkCallback
    {
        private readonly Action<bool> _onResult;

        public SdkStartCallback(Action<bool> onResult)
        {
            _onResult = onResult;
        }

        public void OnSuccess()
        {
            MainThreadDispatcher.Dispatch(() => _onResult(true));
        }

        public void OnFailed(int code, string? message)
        {
            MainThreadDispatcher.Dispatch(() => _onResult(false));
        }
    }
}
#endif
