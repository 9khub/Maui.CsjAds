#if ANDROID
using System.Diagnostics;
using CsjAds.Internal;

namespace CsjAds.Platforms.Android;

internal sealed class CsjFeedAdImpl : ICsjFeedAd
{
    private readonly string _slotId;
    private readonly int _adCount;
    private readonly int _width;
    private readonly int _height;
    private Com.Csjads.Wrapper.CsjFeedAd? _nativeAd;
    private FeedCallback? _callback;
    private bool _disposed;

    public CsjFeedAdImpl(string slotId, int adCount, int width, int height)
    {
        _slotId = slotId;
        _adCount = adCount;
        _width = width;
        _height = height;
    }

    public int LoadedCount => _nativeAd?.RenderedCount ?? 0;

    public event EventHandler<AdEventArgs>? OnAdsLoaded;
    public event EventHandler<AdErrorEventArgs>? OnAdsFailed;
    public event EventHandler<AdEventArgs>? OnAdClicked;

    public Task LoadAsync()
    {
        ThrowIfDisposed();
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _nativeAd = new Com.Csjads.Wrapper.CsjFeedAd(_slotId, _adCount, _width, _height);
        _callback = new FeedCallback(
            onLoaded: () =>
            {
                MainThreadDispatcher.Dispatch(() =>
                {
                    try { OnAdsLoaded?.Invoke(this, new AdEventArgs()); }
                    catch (Exception ex) { Debug.WriteLine($"[CsjAds] Feed OnAdsLoaded: {ex}"); }
                });
                tcs.TrySetResult();
            },
            onFailed: (code, msg) =>
            {
                MainThreadDispatcher.Dispatch(() =>
                {
                    try { OnAdsFailed?.Invoke(this, new AdErrorEventArgs(new AdError(code, msg ?? "Unknown"))); }
                    catch (Exception ex) { Debug.WriteLine($"[CsjAds] Feed OnAdsFailed: {ex}"); }
                });
                tcs.TrySetException(new InvalidOperationException($"Feed ad load failed: {code} {msg}"));
            },
            onClicked: () =>
            {
                MainThreadDispatcher.Dispatch(() =>
                {
                    try { OnAdClicked?.Invoke(this, new AdEventArgs()); }
                    catch (Exception ex) { Debug.WriteLine($"[CsjAds] Feed OnAdClicked: {ex}"); }
                });
            });

        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
            ?? throw new InvalidOperationException("No current activity.");
        _nativeAd.Load(activity, _callback);

        return tcs.Task;
    }

    public object? GetRenderedView(int index)
    {
        try { return _nativeAd?.GetRenderedView(index); }
        catch { return null; }
    }

    public bool IsSelfRenderMode
    {
        get
        {
            try { return _nativeAd?.IsSelfRenderMode ?? false; }
            catch { return false; }
        }
    }

    public string? GetAdTitle(int index)
    {
        try { return _nativeAd?.GetAdTitle(index); }
        catch { return null; }
    }

    public string? GetAdImageUrl(int index)
    {
        try { return _nativeAd?.GetAdImageUrl(index); }
        catch { return null; }
    }

    /// <summary>
    /// 将第 index 条广告渲染到已 attach 到 window 的容器中。
    /// 必须在主线程调用。
    /// </summary>
    public void RenderIntoContainer(int index, global::Android.Views.ViewGroup container, Action onSuccess, Action<string> onFailed)
    {
        Console.WriteLine($"[CsjAds] FeedAdImpl.RenderIntoContainer: index={index}, nativeAd={_nativeAd != null}, container.attached={container?.IsAttachedToWindow}, container.size={container?.Width}x{container?.Height}");
        if (_nativeAd == null) { onFailed("nativeAd is null"); return; }
        try
        {
            var cb = new RenderCallback(onSuccess, onFailed);
            _nativeAd.RenderIntoContainer(index, container, cb);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CsjAds] FeedAdImpl.RenderIntoContainer EXCEPTION: {ex.Message}");
            onFailed(ex.Message);
        }
    }

    private sealed class RenderCallback : Java.Lang.Object, Com.Csjads.Wrapper.ICsjAdCallback
    {
        private readonly Action _onSuccess;
        private readonly Action<string> _onFailed;
        public RenderCallback(Action onSuccess, Action<string> onFailed) { _onSuccess = onSuccess; _onFailed = onFailed; }
        public void OnAdLoaded() => MainThreadDispatcher.Dispatch(() => _onSuccess());
        public void OnAdFailed(int code, string? msg) => MainThreadDispatcher.Dispatch(() => _onFailed(msg ?? $"code={code}"));
        public void OnAdShow() { }
        public void OnAdClicked() { }
        public void OnAdClosed() { }
        public void OnRewardVerified(string? n, int a, bool v) { }
    }

    public void Destroy()
    {
        _nativeAd?.Destroy();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Destroy();
        _nativeAd?.Dispose();
        _nativeAd = null;
        _callback = null;
    }

    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(_disposed, this);

    private sealed class FeedCallback : Java.Lang.Object, Com.Csjads.Wrapper.ICsjAdCallback
    {
        private readonly Action _onLoaded;
        private readonly Action<int, string?> _onFailed;
        private readonly Action _onClicked;

        public FeedCallback(Action onLoaded, Action<int, string?> onFailed, Action onClicked)
        {
            _onLoaded = onLoaded;
            _onFailed = onFailed;
            _onClicked = onClicked;
        }

        public void OnAdLoaded() { try { _onLoaded(); } catch (Exception ex) { Debug.WriteLine($"[CsjAds] JNI Feed OnAdLoaded: {ex}"); } }
        public void OnAdFailed(int code, string? message) { try { _onFailed(code, message); } catch (Exception ex) { Debug.WriteLine($"[CsjAds] JNI Feed OnAdFailed: {ex}"); } }
        public void OnAdClicked() { try { _onClicked(); } catch (Exception ex) { Debug.WriteLine($"[CsjAds] JNI Feed OnAdClicked: {ex}"); } }
        public void OnAdShow() { }
        public void OnAdClosed() { }
        public void OnRewardVerified(string? rewardName, int rewardAmount, bool verified) { }
    }
}
#endif
