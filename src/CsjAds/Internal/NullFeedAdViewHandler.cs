#if IOS || MACCATALYST
using Microsoft.Maui.Handlers;
using UIKit;

namespace CsjAds.Internal;

public sealed class NullFeedAdViewHandler : ViewHandler<CsjFeedAdView, UIView>
{
    public NullFeedAdViewHandler() : base(ViewMapper) { }
    protected override UIView CreatePlatformView() => new UIView();
}

#elif ANDROID
using Microsoft.Maui.Handlers;

namespace CsjAds.Internal;

public sealed class NullFeedAdViewHandler : ViewHandler<CsjFeedAdView, global::Android.Views.View>
{
    public NullFeedAdViewHandler() : base(ViewMapper) { }
    protected override global::Android.Views.View CreatePlatformView() => new global::Android.Views.View(Context!);
}

#else
using Microsoft.Maui.Handlers;

namespace CsjAds.Internal;

public sealed class NullFeedAdViewHandler : ViewHandler<CsjFeedAdView, object>
{
    public NullFeedAdViewHandler() : base(ViewMapper) { }
    protected override object CreatePlatformView() => new object();
}
#endif
