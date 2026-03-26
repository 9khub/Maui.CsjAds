#if IOS && !CSJ_NO_NATIVE
using CsjAds.Internal;
using CsjAds.iOS.Binding;
using UIKit;

namespace CsjAds.Platforms.iOS;

internal sealed class CsjSplashAdImpl : ICsjSplashAd
{
    private readonly string _slotId;
    private CsjSplashAd? _nativeAd;
    private CsjAdCallbackProxy? _callbackProxy;
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

        _callbackProxy = new CsjAdCallbackProxy(
            onLoaded: () =>
            {
                IsLoaded = true;
                MainThreadDispatcher.Dispatch(() => OnAdLoaded?.Invoke(this, new AdEventArgs()));
                tcs.TrySetResult();
            },
            onFailed: (code, msg) =>
            {
                IsLoaded = false;
                var error = new AdError((int)code, msg ?? "Unknown error");
                MainThreadDispatcher.Dispatch(() => OnAdFailed?.Invoke(this, new AdErrorEventArgs(error)));
                tcs.TrySetException(new Exception($"Splash ad load failed: {error}"));
            },
            onShown: () => MainThreadDispatcher.Dispatch(() => OnAdShown?.Invoke(this, new AdEventArgs())),
            onClicked: () => MainThreadDispatcher.Dispatch(() => OnAdClicked?.Invoke(this, new AdEventArgs())),
            onClosed: () =>
            {
                IsLoaded = false;
                MainThreadDispatcher.Dispatch(() => OnAdClosed?.Invoke(this, new AdEventArgs()));
            });

        _nativeAd = new CsjSplashAd(_slotId, TimeoutMilliseconds);
        _nativeAd.LoadWithCallback(_callbackProxy);

        return tcs.Task;
    }

    public void Show()
    {
        ThrowIfDisposed();
        if (!IsLoaded || _nativeAd == null)
            throw new InvalidOperationException("Splash ad is not loaded. Call LoadAsync() first.");

        var window = UIApplication.SharedApplication?.KeyWindow
            ?? UIApplication.SharedApplication?.Windows?.FirstOrDefault()
            ?? throw new InvalidOperationException("No key window available.");

        _nativeAd.ShowInWindow(window);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _nativeAd?.Dispose();
        _nativeAd = null;
        _callbackProxy?.Dispose();
        _callbackProxy = null;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
#endif
