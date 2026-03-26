#if IOS && !CSJ_NO_NATIVE
using CsjAds.Internal;
using CsjAds.iOS.Binding;
using Microsoft.Maui.Handlers;
using UIKit;

namespace CsjAds.Platforms.iOS.Handlers;

/// <summary>
/// MAUI handler that maps <see cref="CsjBannerView"/> to a native iOS UIView
/// containing the CSJ banner ad.
/// </summary>
internal sealed class CsjBannerViewHandler : ViewHandler<CsjBannerView, UIView>
{
    private CsjBannerAd? _nativeBannerAd;
    private CsjAdCallbackProxy? _callbackProxy;

    public static readonly IPropertyMapper<CsjBannerView, CsjBannerViewHandler> Mapper =
        new PropertyMapper<CsjBannerView, CsjBannerViewHandler>(ViewMapper)
        {
            [nameof(CsjBannerView.SlotId)] = MapSlotId,
            [nameof(CsjBannerView.AdSize)] = MapAdSize,
        };

    public CsjBannerViewHandler() : base(Mapper)
    {
    }

    protected override UIView CreatePlatformView()
    {
        var container = new UIView();
        LoadAd(container);
        return container;
    }

    protected override void DisconnectHandler(UIView platformView)
    {
        CleanUp();
        foreach (var subview in platformView.Subviews)
            subview.RemoveFromSuperview();
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
        if (PlatformView == null) return;

        CleanUp();
        foreach (var subview in PlatformView.Subviews)
            subview.RemoveFromSuperview();
        LoadAd(PlatformView);
    }

    private void LoadAd(UIView container)
    {
        if (VirtualView == null || string.IsNullOrEmpty(VirtualView.SlotId)) return;

        var adSize = VirtualView.AdSize;
        var width = adSize.IsAdaptive ? 0 : adSize.Width;
        var height = adSize.IsAdaptive ? 0 : adSize.Height;

        _callbackProxy = new CsjAdCallbackProxy(
            onLoaded: () => MainThreadDispatcher.Dispatch(() => VirtualView?.RaiseAdLoaded()),
            onFailed: (code, msg) => MainThreadDispatcher.Dispatch(
                () => VirtualView?.RaiseAdFailed(new AdError((int)code, msg ?? "Unknown"))),
            onClicked: () => MainThreadDispatcher.Dispatch(() => VirtualView?.RaiseAdClicked()),
            onClosed: () => MainThreadDispatcher.Dispatch(() => VirtualView?.RaiseAdClosed()));

        var viewController = GetRootViewController();
        _nativeBannerAd = new CsjBannerAd(VirtualView.SlotId, width, height);
        _nativeBannerAd.LoadIn(container, viewController, _callbackProxy);
    }

    private void CleanUp()
    {
        _nativeBannerAd?.Destroy();
        _nativeBannerAd?.Dispose();
        _nativeBannerAd = null;
        _callbackProxy?.Dispose();
        _callbackProxy = null;
    }

    private static UIViewController? GetRootViewController()
    {
        var window = UIApplication.SharedApplication?.KeyWindow
            ?? UIApplication.SharedApplication?.Windows?.FirstOrDefault();
        return window?.RootViewController;
    }
}
#endif
