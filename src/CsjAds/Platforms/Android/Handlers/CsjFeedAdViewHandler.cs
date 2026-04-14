#if ANDROID
using Microsoft.Maui.Handlers;

namespace CsjAds.Platforms.Android.Handlers;

/// <summary>
/// MAUI handler for <see cref="CsjFeedAdView"/>.
/// Express render 延迟到容器 attach 到 window 后执行（与 Banner ViewAttachedToWindow 策略一致）。
/// </summary>
internal sealed class CsjFeedAdViewHandler : ViewHandler<CsjFeedAdView, global::Android.Widget.FrameLayout>
{
    private bool _rendered;

    public static readonly IPropertyMapper<CsjFeedAdView, CsjFeedAdViewHandler> Mapper =
        new PropertyMapper<CsjFeedAdView, CsjFeedAdViewHandler>(ViewMapper)
        {
            [nameof(CsjFeedAdView.NativeAdReference)] = MapNativeAdReference,
            [nameof(CsjFeedAdView.AdIndex)] = MapAdIndex,
        };

    public CsjFeedAdViewHandler() : base(Mapper) { }

    protected override global::Android.Widget.FrameLayout CreatePlatformView()
    {
        var container = new global::Android.Widget.FrameLayout(Context);
        container.ViewAttachedToWindow += OnContainerAttached;
        container.LayoutChange += OnContainerLayoutChange;
        return container;
    }

    protected override void DisconnectHandler(global::Android.Widget.FrameLayout platformView)
    {
        platformView.ViewAttachedToWindow -= OnContainerAttached;
        platformView.LayoutChange -= OnContainerLayoutChange;
        platformView.RemoveAllViews();
        _rendered = false;
        base.DisconnectHandler(platformView);
    }

    private void OnContainerAttached(object? sender, global::Android.Views.View.ViewAttachedToWindowEventArgs e)
    {
        Console.WriteLine("[CsjAds] FeedAdView: container attached to window");
        TryRender();
    }

    private void OnContainerLayoutChange(object? sender, global::Android.Views.View.LayoutChangeEventArgs e)
    {
        // 容器大小变化时重试 — attach 时 size 可能是 0x0，layout 完成后才有实际尺寸
        if (!_rendered && sender is global::Android.Widget.FrameLayout fl && fl.Width > 0 && fl.Height > 0)
        {
            Console.WriteLine($"[CsjAds] FeedAdView: layout changed to {fl.Width}x{fl.Height}");
            TryRender();
        }
    }

    private static void MapNativeAdReference(CsjFeedAdViewHandler handler, CsjFeedAdView view)
    {
        Console.WriteLine($"[CsjAds] FeedAdView.MapNativeAdRef: type={view.NativeAdReference?.GetType().Name}, adIndex={view.AdIndex}");
        handler.TryRender();
    }

    private static void MapAdIndex(CsjFeedAdViewHandler handler, CsjFeedAdView view)
    {
        Console.WriteLine($"[CsjAds] FeedAdView.MapAdIndex: adIndex={view.AdIndex}, hasRef={view.NativeAdReference != null}");
        handler.TryRender();
    }

    private void TryRender()
    {
        try
        {
            TryRenderCore();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CsjAds] FeedAdView.TryRender EXCEPTION: {ex.Message}");
        }
    }

    private void TryRenderCore()
    {
        if (_rendered) return;
        if (PlatformView == null || VirtualView == null)
        {
            Console.WriteLine("[CsjAds] FeedAdView.TryRender: PlatformView/VirtualView is null");
            return;
        }
        if (VirtualView.AdIndex < 0)
        {
            Console.WriteLine($"[CsjAds] FeedAdView.TryRender: AdIndex={VirtualView.AdIndex}, waiting");
            return;
        }
        if (VirtualView.NativeAdReference == null)
        {
            Console.WriteLine("[CsjAds] FeedAdView.TryRender: NativeAdReference is null, waiting");
            return;
        }
        if (!PlatformView.IsAttachedToWindow)
        {
            Console.WriteLine("[CsjAds] FeedAdView.TryRender: container not attached, waiting");
            return;
        }
        if (PlatformView.Width <= 0 || PlatformView.Height <= 0)
        {
            Console.WriteLine($"[CsjAds] FeedAdView.TryRender: container size {PlatformView.Width}x{PlatformView.Height}, waiting for layout");
            return;
        }

        if (VirtualView.NativeAdReference is not CsjFeedAdImpl feedAdImpl)
        {
            Console.WriteLine($"[CsjAds] FeedAdView.TryRender: NativeAdReference type mismatch: {VirtualView.NativeAdReference.GetType().Name}");
            return;
        }

        _rendered = true;
        var index = VirtualView.AdIndex;
        Console.WriteLine($"[CsjAds] FeedAdView.TryRender: rendering index={index}");

        feedAdImpl.RenderIntoContainer(index, PlatformView,
            onSuccess: () =>
            {
                Console.WriteLine($"[CsjAds] FeedAdView: render SUCCESS index={index}");
                VirtualView?.RaiseAdLoaded();
            },
            onFailed: (msg) =>
            {
                Console.WriteLine($"[CsjAds] FeedAdView: render FAILED index={index}: {msg}");
                VirtualView?.RaiseAdFailed(new AdError(-1001, msg));
            });
    }
}
#endif
