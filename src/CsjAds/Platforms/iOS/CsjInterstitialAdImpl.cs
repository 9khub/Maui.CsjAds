#if IOS && !CSJ_NO_NATIVE
using CsjAds.Internal;
using CsjAds.iOS.Binding;
using UIKit;

namespace CsjAds.Platforms.iOS;

internal sealed class CsjInterstitialAdImpl : ICsjInterstitialAd
{
    private readonly string _slotId;
    private CsjInterstitialAd? _nativeAd;
    private CsjAdCallbackProxy? _callbackProxy;
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
                tcs.TrySetException(new Exception($"Ad load failed: {error}"));
            },
            onShown: () => MainThreadDispatcher.Dispatch(() => OnAdShown?.Invoke(this, new AdEventArgs())),
            onClicked: () => MainThreadDispatcher.Dispatch(() => OnAdClicked?.Invoke(this, new AdEventArgs())),
            onClosed: () =>
            {
                IsLoaded = false;
                MainThreadDispatcher.Dispatch(() => OnAdClosed?.Invoke(this, new AdEventArgs()));
            });

        _nativeAd = new CsjInterstitialAd(_slotId);
        _nativeAd.LoadWithCallback(_callbackProxy);

        return tcs.Task;
    }

    public void Show()
    {
        ThrowIfDisposed();
        if (!IsLoaded || _nativeAd == null)
            throw new InvalidOperationException("Ad is not loaded. Call LoadAsync() first.");

        var viewController = GetRootViewController()
            ?? throw new InvalidOperationException("No root view controller available.");

        _nativeAd.ShowFrom(viewController);
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

    private static UIViewController? GetRootViewController()
    {
        var window = UIApplication.SharedApplication?.KeyWindow
            ?? UIApplication.SharedApplication?.Windows?.FirstOrDefault();
        return window?.RootViewController;
    }
}
#endif
