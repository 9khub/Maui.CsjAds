#if ANDROID
using System.Diagnostics;
using Android.Views;
using CsjAds.Internal;
using Microsoft.Maui.Handlers;

namespace CsjAds.Platforms.Android.Handlers;

/// <summary>
/// MAUI handler that maps <see cref="CsjBannerView"/> to a native Android View
/// containing the CSJ banner ad.
/// </summary>
internal sealed class CsjBannerViewHandler : ViewHandler<CsjBannerView, global::Android.Widget.FrameLayout>
{
    private Com.Csjads.Wrapper.CsjBannerAd? _nativeBannerAd;
    private BannerCallback? _bannerCallback;

    public static readonly IPropertyMapper<CsjBannerView, CsjBannerViewHandler> Mapper =
        new PropertyMapper<CsjBannerView, CsjBannerViewHandler>(ViewMapper)
        {
            [nameof(CsjBannerView.SlotId)] = MapSlotId,
            [nameof(CsjBannerView.AdSize)] = MapAdSize,
        };

    public CsjBannerViewHandler() : base(Mapper)
    {
    }

    protected override global::Android.Widget.FrameLayout CreatePlatformView()
    {
        var container = new global::Android.Widget.FrameLayout(Context);
        LoadAd(container);
        return container;
    }

    protected override void DisconnectHandler(global::Android.Widget.FrameLayout platformView)
    {
        _nativeBannerAd?.Destroy();
        _nativeBannerAd?.Dispose();
        _nativeBannerAd = null;
        _bannerCallback = null;
        platformView.RemoveAllViews();
        base.DisconnectHandler(platformView);
    }

    private static void MapSlotId(CsjBannerViewHandler handler, CsjBannerView view)
    {
        handler.ReloadAd();
    }

    private static void MapAdSize(CsjBannerViewHandler handler, CsjBannerView view)
    {
        handler.ReloadAd();
    }

    private void ReloadAd()
    {
        if (PlatformView == null || VirtualView == null) return;
        if (VirtualView.Handler != null && !ReferenceEquals(VirtualView.Handler, this)) return;

        _nativeBannerAd?.Destroy();
        _nativeBannerAd?.Dispose();
        _nativeBannerAd = null;
        PlatformView.RemoveAllViews();
        LoadAd(PlatformView);
    }

    private void LoadAd(global::Android.Widget.FrameLayout container)
    {
        if (VirtualView == null || string.IsNullOrEmpty(VirtualView.SlotId) || container == null)
            return;

        var ctx = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? Context
            ?? global::Android.App.Application.Context;
        if (ctx == null)
        {
            Debug.WriteLine("[CsjAds] Banner LoadAd skipped: no Context");
            return;
        }

        var adSize = VirtualView.AdSize;
        var width = adSize.IsAdaptive ? 0 : adSize.Width;
        var height = adSize.IsAdaptive ? 0 : adSize.Height;

        try
        {
            _nativeBannerAd = new Com.Csjads.Wrapper.CsjBannerAd(
                VirtualView.SlotId, width, height);

            _bannerCallback = new BannerCallback(VirtualView);
            _nativeBannerAd.Load(ctx, container, _bannerCallback);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CsjAds] Banner LoadAd failed: {ex}");
            _nativeBannerAd?.Dispose();
            _nativeBannerAd = null;
            _bannerCallback = null;
            MainThreadDispatcher.Dispatch(() =>
                VirtualView?.RaiseAdFailed(new AdError(-2000, ex.Message)));
        }
    }

    private sealed class BannerCallback : Java.Lang.Object, Com.Csjads.Wrapper.ICsjAdCallback
    {
        private readonly CsjBannerView _view;

        public BannerCallback(CsjBannerView view) => _view = view;

        public void OnAdLoaded() =>
            MainThreadDispatcher.Dispatch(() =>
            {
                try
                {
                    _view.RaiseAdLoaded();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CsjAds] Banner OnAdLoaded: {ex}");
                }
            });

        public void OnAdFailed(int code, string? message) =>
            MainThreadDispatcher.Dispatch(() =>
            {
                try
                {
                    _view.RaiseAdFailed(new AdError(code, message ?? "Unknown"));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CsjAds] Banner OnAdFailed: {ex}");
                }
            });

        public void OnAdClicked() =>
            MainThreadDispatcher.Dispatch(() =>
            {
                try
                {
                    _view.RaiseAdClicked();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CsjAds] Banner OnAdClicked: {ex}");
                }
            });

        public void OnAdClosed() =>
            MainThreadDispatcher.Dispatch(() =>
            {
                try
                {
                    _view.RaiseAdClosed();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CsjAds] Banner OnAdClosed: {ex}");
                }
            });

        public void OnAdShow() { }
        public void OnRewardVerified(string? rewardName, int rewardAmount, bool verified) { }
    }
}
#endif
