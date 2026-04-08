#if ANDROID
using System.Diagnostics;
using CsjAds.Internal;

namespace CsjAds.Platforms.Android;

internal sealed class CsjSplashAdImpl : ICsjSplashAd
{
    private readonly string _slotId;
    private Com.Csjads.Wrapper.CsjSplashAd? _nativeAd;
    private AdCallback? _adCallback;
    private bool _disposed;

    public CsjSplashAdImpl(string slotId)
    {
        _slotId = slotId;
    }

    public bool IsLoaded { get; private set; }
    public int TimeoutMilliseconds { get; set; } = 5000;

    public event EventHandler<AdEventArgs>? OnAdLoaded;
    public event EventHandler<AdErrorEventArgs>? OnAdFailed;
    public event EventHandler<AdEventArgs>? OnAdShown;
    public event EventHandler<AdEventArgs>? OnAdClicked;
    public event EventHandler<AdEventArgs>? OnAdClosed;

    public Task LoadAsync()
    {
        ThrowIfDisposed();
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void CompleteFailed(int code, string? msg)
        {
            IsLoaded = false;
            var error = new AdError(code, msg ?? "Unknown error");
            MainThreadDispatcher.Dispatch(() =>
            {
                try
                {
                    OnAdFailed?.Invoke(this, new AdErrorEventArgs(error));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CsjAds] OnAdFailed handler: {ex}");
                }
            });
            tcs.TrySetException(new InvalidOperationException($"Splash ad load failed: {error}"));
        }

        _nativeAd = new Com.Csjads.Wrapper.CsjSplashAd(_slotId, TimeoutMilliseconds);
        _adCallback = new AdCallback(
            onLoaded: () =>
            {
                try
                {
                    IsLoaded = true;
                    MainThreadDispatcher.Dispatch(() =>
                    {
                        try
                        {
                            OnAdLoaded?.Invoke(this, new AdEventArgs());
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[CsjAds] OnAdLoaded handler: {ex}");
                        }
                    });
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CsjAds] Splash onLoaded path: {ex}");
                    CompleteFailed(-1, ex.Message);
                }
            },
            onFailed: CompleteFailed,
            onShown: () => MainThreadDispatcher.Dispatch(() =>
            {
                try
                {
                    OnAdShown?.Invoke(this, new AdEventArgs());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CsjAds] OnAdShown: {ex}");
                }
            }),
            onClicked: () => MainThreadDispatcher.Dispatch(() =>
            {
                try
                {
                    OnAdClicked?.Invoke(this, new AdEventArgs());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CsjAds] OnAdClicked: {ex}");
                }
            }),
            onClosed: () =>
            {
                try
                {
                    IsLoaded = false;
                    MainThreadDispatcher.Dispatch(() =>
                    {
                        try
                        {
                            OnAdClosed?.Invoke(this, new AdEventArgs());
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[CsjAds] OnAdClosed: {ex}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CsjAds] Splash onClosed path: {ex}");
                }
            });

        try
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
                ?? throw new InvalidOperationException("No current activity available for ad loading.");
            _nativeAd.Load(activity, _adCallback);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CsjAds] Splash LoadAsync start failed: {ex}");
            tcs.TrySetException(ex);
        }

        return tcs.Task;
    }

    public void Show()
    {
        ThrowIfDisposed();
        if (!IsLoaded || _nativeAd == null)
            throw new InvalidOperationException("Splash ad is not loaded. Call LoadAsync() first.");

        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
            ?? throw new InvalidOperationException("No current activity available.");

        try
        {
            _nativeAd.Show(activity);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CsjAds] Native splash Show failed: {ex}");
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            // Force-remove any leftover splash views from the content root
            _nativeAd?.Destroy();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CsjAds] Splash Destroy: {ex.Message}");
        }
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

        public void OnAdLoaded()
        {
            try
            {
                _onLoaded();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CsjAds] JNI OnAdLoaded: {ex}");
            }
        }

        public void OnAdFailed(int code, string? message)
        {
            try
            {
                _onFailed(code, message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CsjAds] JNI OnAdFailed: {ex}");
            }
        }

        public void OnAdShow()
        {
            try
            {
                _onShown();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CsjAds] JNI OnAdShow: {ex}");
            }
        }

        public void OnAdClicked()
        {
            try
            {
                _onClicked();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CsjAds] JNI OnAdClicked: {ex}");
            }
        }

        public void OnAdClosed()
        {
            try
            {
                _onClosed();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CsjAds] JNI OnAdClosed: {ex}");
            }
        }
        public void OnRewardVerified(string? rewardName, int rewardAmount, bool verified) { }
    }
}
#endif
