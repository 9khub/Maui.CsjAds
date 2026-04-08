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
    private SdkStartCallback? _sdkStartCallback;

    public bool IsInitialized { get; private set; }

    public void Configure(CsjAdConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Check device support before attempting init
        if (!Com.Csjads.Wrapper.CsjSdkWrapper.IsDeviceSupported)
        {
            Console.WriteLine("[CsjAds] Device ABI not supported (x86/x86_64 emulator?), ads disabled");
            _isConfigured = false;
            return;
        }

        var context = global::Android.App.Application.Context;

        Console.WriteLine($"[CsjAds] Configuring SDK: AppId={configuration.AppId}, UseMediation={configuration.UseMediation}");

        // Call the native wrapper's init (pre-consent step)
        var initResult = Com.Csjads.Wrapper.CsjSdkWrapper.Init(
            context,
            configuration.AppId,
            configuration.AppName,
            configuration.IsDebug,
            configuration.Privacy.AllowPersonalizedAd,
            configuration.Privacy.AllowLocation,
            configuration.Privacy.AllowPhoneState,
            configuration.Privacy.AllowWriteExternal,
            configuration.Privacy.AllowWifiState,
            configuration.Privacy.AllowAndroidId,
            configuration.Privacy.AndroidIdOverride,
            configuration.UseMediation,
            configuration.Privacy.CustomDeviceId);

        _isConfigured = initResult;
        if (!initResult)
        {
            Console.WriteLine("[CsjAds] SDK init returned false, ads disabled");
        }
    }

    public async Task<bool> StartAsync()
    {
        // If device is not supported or init failed, return false silently
        if (!_isConfigured)
            return false;

        if (IsInitialized)
            return true;

        var tcs = new TaskCompletionSource<bool>();

        Console.WriteLine("[CsjAds] Starting SDK...");

        _sdkStartCallback = new SdkStartCallback(async success =>
        {
            if (success)
            {
                // Step 3: Wait for internal readiness (isInitSuccess)
                bool isReady = await WaitForInternalReadinessAsync();
                IsInitialized = isReady;
                tcs.TrySetResult(isReady);
            }
            else
            {
                IsInitialized = false;
                tcs.TrySetResult(false);
            }
        });

        Com.Csjads.Wrapper.CsjSdkWrapper.Start(_sdkStartCallback);

        return await tcs.Task;
    }

    private async Task<bool> WaitForInternalReadinessAsync()
    {
        const int maxRetries = 10;
        const int delayMs = 500;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // Use JNI to call TTAdSdk.isInitSuccess() directly
                // This checks if the SDK rendering engine is actually ready
                IntPtr ttAdSdkClass = global::Android.Runtime.JNIEnv.FindClass("com/bytedance/sdk/openadsdk/TTAdSdk");
                IntPtr isInitSuccessMethod = global::Android.Runtime.JNIEnv.GetStaticMethodID(ttAdSdkClass, "isInitSuccess", "()Z");
                bool isReady = global::Android.Runtime.JNIEnv.CallStaticBooleanMethod(ttAdSdkClass, isInitSuccessMethod);

                if (isReady)
                {
                    Console.WriteLine($"[CsjAds] SDK internal readiness confirmed after {i * delayMs}ms.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CsjAds] Error checking readiness: {ex.Message}");
            }

            Console.WriteLine($"[CsjAds] Waiting for SDK internal readiness... (attempt {i + 1}/{maxRetries})");
            await Task.Delay(delayMs);
        }

        Console.WriteLine("[CsjAds] SDK internal readiness timeout.");
        return false;
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

    public ICsjFeedAd CreateFeedAd(string slotId, int adCount = 3, int width = 0, int height = 0)
    {
        EnsureInitialized();
        return new CsjFeedAdImpl(slotId, adCount, width, height);
    }
    
    public void LaunchDebugTool(string url)
    {
        // This is a placeholder for debugging purposes
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
