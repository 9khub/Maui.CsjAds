using Foundation;
using ObjCRuntime;
using UIKit;

namespace CsjAds.iOS.Binding;

// CsjAdCallback protocol
[Protocol, Model]
[BaseType(typeof(NSObject))]
interface CsjAdCallbackDelegate
{
    [Abstract]
    [Export("adDidLoad")]
    void AdDidLoad();

    [Abstract]
    [Export("adDidFailWithCode:message:")]
    void AdDidFail(nint code, string message);

    [Abstract]
    [Export("adDidShow")]
    void AdDidShow();

    [Abstract]
    [Export("adDidClick")]
    void AdDidClick();

    [Abstract]
    [Export("adDidClose")]
    void AdDidClose();

    [Export("rewardDidVerifyWithName:amount:verified:")]
    void RewardDidVerify(string rewardName, nint rewardAmount, bool verified);
}

// CsjSdkWrapper
delegate void CsjSdkStartCompletion(bool success, nint code, [NullAllowed] string message);

[BaseType(typeof(NSObject))]
interface CsjSdkWrapper
{
    [Static]
    [Export("configureWithAppId:appName:debug:allowPersonalizedAd:allowLocation:")]
    void Configure(string appId, string appName, bool debug, bool allowPersonalizedAd, bool allowLocation);

    [Static]
    [Export("startWithCompletion:")]
    void Start(CsjSdkStartCompletion completion);

    [Static]
    [Export("isInitialized")]
    bool IsInitialized { get; }
}

// CsjRewardedVideoAd
[BaseType(typeof(NSObject))]
interface CsjRewardedVideoAd
{
    [Export("initWithSlotId:")]
    NativeHandle Constructor(string slotId);

    [Export("loadWithCallback:")]
    void LoadWithCallback(CsjAdCallbackDelegate callback);

    [Export("showFromViewController:")]
    void ShowFrom(UIViewController viewController);
}

// CsjInterstitialAd
[BaseType(typeof(NSObject))]
interface CsjInterstitialAd
{
    [Export("initWithSlotId:")]
    NativeHandle Constructor(string slotId);

    [Export("loadWithCallback:")]
    void LoadWithCallback(CsjAdCallbackDelegate callback);

    [Export("showFromViewController:")]
    void ShowFrom(UIViewController viewController);
}

// CsjBannerAd
[BaseType(typeof(NSObject))]
interface CsjBannerAd
{
    [Export("initWithSlotId:width:height:")]
    NativeHandle Constructor(string slotId, nint width, nint height);

    [Export("loadInView:viewController:callback:")]
    void LoadIn(UIView container, [NullAllowed] UIViewController viewController, CsjAdCallbackDelegate callback);

    [Export("destroy")]
    void Destroy();
}

// CsjSplashAd
[BaseType(typeof(NSObject))]
interface CsjSplashAd
{
    [Export("initWithSlotId:timeout:")]
    NativeHandle Constructor(string slotId, nint timeoutMs);

    [Export("loadWithCallback:")]
    void LoadWithCallback(CsjAdCallbackDelegate callback);

    [Export("showInWindow:")]
    void ShowInWindow(UIWindow window);
}
