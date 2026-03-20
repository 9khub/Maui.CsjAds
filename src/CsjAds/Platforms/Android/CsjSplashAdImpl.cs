#if ANDROID
using CsjAds.Internal;

namespace CsjAds.Platforms.Android;

internal sealed class CsjSplashAdImpl : ICsjSplashAd
{
    private readonly string _slotId;
    private Com.Csjads.Wrapper.CsjSplashAd? _nativeAd;
    private bool _disposed;

    public CsjSplashAdImpl(string slotId)
    {
        _slotId = slotId;
    }

    public bool IsLoaded { get; private set; }
    public int TimeoutMilliseconds { get; set; } = 3000;

    public event EventHandler<AdEventArgs>? OnAdLoaded;
    public event EventHandler<AdErrorEventArgs>? OnAdFailed;
    public event EventHandler<AdEventArgs>? OnAdShown;
    public event EventHandler<AdEventArgs>? OnAdClicked;
    public event EventHandler<AdEventArgs>? OnAdClosed;

    public Task LoadAsync()
    {
        ThrowIfDisposed();
        var tcs = new TaskCompletionSource();

        _nativeAd = new Com.Csjads.Wrapper.CsjSplashAd(_slotId, TimeoutMilliseconds);
        _nativeAd.Load(global::Android.App.Application.Context, new AdCallback(
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
                tcs.TrySetException(new Exception($"Splash ad load failed: {error}"));
            },
            onShown: () => MainThreadDispatcher.Dispatch(() => OnAdShown?.Invoke(this, new AdEventArgs())),
            onClicked: () => MainThreadDispatcher.Dispatch(() => OnAdClicked?.Invoke(this, new AdEventArgs())),
            onClosed: () =>
            {
                IsLoaded = false;
                MainThreadDispatcher.Dispatch(() => OnAdClosed?.Invoke(this, new AdEventArgs()));
            }));

        return tcs.Task;
    }

    public void Show()
    {
        ThrowIfDisposed();
        if (!IsLoaded || _nativeAd == null)
            throw new InvalidOperationException("Splash ad is not loaded. Call LoadAsync() first.");

        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
            ?? throw new InvalidOperationException("No current activity available.");

        _nativeAd.Show(activity);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _nativeAd?.Dispose();
        _nativeAd = null;
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
