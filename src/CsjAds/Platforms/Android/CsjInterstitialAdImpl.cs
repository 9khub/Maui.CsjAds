#if ANDROID
using CsjAds.Internal;

namespace CsjAds.Platforms.Android;

internal sealed class CsjInterstitialAdImpl : ICsjInterstitialAd
{
    private readonly string _slotId;
    private Com.Csjads.Wrapper.CsjInterstitialAd? _nativeAd;
    private AdCallback? _adCallback;
    private bool _disposed;

    public CsjInterstitialAdImpl(string slotId)
    {
        _slotId = slotId;
    }

    public bool IsLoaded { get; private set; }

    public event EventHandler<AdEventArgs>? OnAdLoaded;
    public event EventHandler<AdErrorEventArgs>? OnAdFailed;
    public event EventHandler<AdEventArgs>? OnAdShown;
    public event EventHandler<AdEventArgs>? OnAdClicked;
    public event EventHandler<AdEventArgs>? OnAdClosed;

    public Task LoadAsync()
    {
        ThrowIfDisposed();
        var tcs = new TaskCompletionSource();

        _nativeAd = new Com.Csjads.Wrapper.CsjInterstitialAd(_slotId);
        _adCallback = new AdCallback(
            onLoaded: () =>
            {
                IsLoaded = true;
                MainThreadDispatcher.Dispatch(() => OnAdLoaded?.Invoke(this, new AdEventArgs()));
                tcs.TrySetResult();
            },
            onFailed: (code, msg) =>
            {
                IsLoaded = false;
                var error = new AdError(code, msg ?? "Unknown error");
                MainThreadDispatcher.Dispatch(() => OnAdFailed?.Invoke(this, new AdErrorEventArgs(error)));
                tcs.TrySetException(new Exception($"Ad load failed: {error}"));
            },
            onShown: () => MainThreadDispatcher.Dispatch(() => OnAdShown?.Invoke(this, new AdEventArgs())),
            onClicked: () => MainThreadDispatcher.Dispatch(() => OnAdClicked?.Invoke(this, new AdEventArgs())),
            onClosed: () =>
            {
                IsLoaded = false;
                MainThreadDispatcher.Dispatch(() => OnAdClosed?.Invoke(this, new AdEventArgs()));
            });

        _nativeAd.Load(global::Android.App.Application.Context, _adCallback);

        return tcs.Task;
    }

    public void Show()
    {
        ThrowIfDisposed();
        if (!IsLoaded || _nativeAd == null)
            throw new InvalidOperationException("Ad is not loaded. Call LoadAsync() first.");

        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
            ?? throw new InvalidOperationException("No current activity available.");

        try
        {
            _nativeAd.Show(activity);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CsjAds] Interstitial Show: {ex}");
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _nativeAd?.Dispose();
        _nativeAd = null;
        _adCallback = null;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private sealed class AdCallback : Java.Lang.Object, Com.Csjads.Wrapper.ICsjAdCallback
    {
        private readonly Action _onLoaded;
        private readonly Action<int, string?> _onFailed;
        private readonly Action _onShown;
        private readonly Action _onClicked;
        private readonly Action _onClosed;

        public AdCallback(
            Action onLoaded,
            Action<int, string?> onFailed,
            Action onShown,
            Action onClicked,
            Action onClosed)
        {
            _onLoaded = onLoaded;
            _onFailed = onFailed;
            _onShown = onShown;
            _onClicked = onClicked;
            _onClosed = onClosed;
        }

        public void OnAdLoaded() => _onLoaded();
        public void OnAdFailed(int code, string? message) => _onFailed(code, message);
        public void OnAdShow() => _onShown();
        public void OnAdClicked() => _onClicked();
        public void OnAdClosed() => _onClosed();
        public void OnRewardVerified(string? rewardName, int rewardAmount, bool verified) { }
    }
}
#endif
