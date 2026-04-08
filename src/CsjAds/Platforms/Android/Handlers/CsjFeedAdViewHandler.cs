#if ANDROID
using Microsoft.Maui.Handlers;

namespace CsjAds.Platforms.Android.Handlers;

/// <summary>
/// MAUI handler that maps <see cref="CsjFeedAdView"/> to a native Android FrameLayout
/// containing a pre-rendered express ad view.
/// </summary>
internal sealed class CsjFeedAdViewHandler : ViewHandler<CsjFeedAdView, global::Android.Widget.FrameLayout>
{
    public static readonly IPropertyMapper<CsjFeedAdView, CsjFeedAdViewHandler> Mapper =
        new PropertyMapper<CsjFeedAdView, CsjFeedAdViewHandler>(ViewMapper)
        {
            [nameof(CsjFeedAdView.NativeAdReference)] = MapNativeAdReference,
        };

    public CsjFeedAdViewHandler() : base(Mapper) { }

    protected override global::Android.Widget.FrameLayout CreatePlatformView()
    {
        return new global::Android.Widget.FrameLayout(Context);
    }

    protected override void DisconnectHandler(global::Android.Widget.FrameLayout platformView)
    {
        platformView.RemoveAllViews();
        base.DisconnectHandler(platformView);
    }

    private static void MapNativeAdReference(CsjFeedAdViewHandler handler, CsjFeedAdView view)
    {
        handler.EmbedNativeView();
    }

    private void EmbedNativeView()
    {
        if (PlatformView == null || VirtualView == null) return;

        PlatformView.RemoveAllViews();

        if (VirtualView.NativeAdReference is not global::Android.Views.View nativeView)
            return;

        // Remove from previous parent if any
        if (nativeView.Parent is global::Android.Views.ViewGroup oldParent)
            oldParent.RemoveView(nativeView);

        try
        {
            PlatformView.AddView(nativeView);
            VirtualView.RaiseAdLoaded();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CsjAds] FeedAdView AddView failed: {ex.Message}");
            VirtualView.RaiseAdFailed(new AdError(-2000, ex.Message));
        }
    }
}
#endif
