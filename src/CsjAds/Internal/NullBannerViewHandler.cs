#if IOS || MACCATALYST
using Microsoft.Maui.Handlers;
using UIKit;

namespace CsjAds.Internal;

/// <summary>
/// No-op handler for <see cref="CsjBannerView"/> when native CSJ SDK is unavailable.
/// Renders an empty UIView so MAUI doesn't throw HandlerNotFoundException.
/// </summary>
public sealed class NullBannerViewHandler : ViewHandler<CsjBannerView, UIView>
{
    public NullBannerViewHandler() : base(ViewMapper) { }

    protected override UIView CreatePlatformView() => new UIView();
}

#elif ANDROID
using Android.Views;
using Microsoft.Maui.Handlers;

namespace CsjAds.Internal;

public sealed class NullBannerViewHandler : ViewHandler<CsjBannerView, global::Android.Views.View>
{
    public NullBannerViewHandler() : base(ViewMapper) { }

    protected override global::Android.Views.View CreatePlatformView() => new global::Android.Views.View(Context!);
}

#else
using Microsoft.Maui.Handlers;

namespace CsjAds.Internal;

public sealed class NullBannerViewHandler : ViewHandler<CsjBannerView, object>
{
    public NullBannerViewHandler() : base(ViewMapper) { }

    protected override object CreatePlatformView() => new object();
}
#endif
