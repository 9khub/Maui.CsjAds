#if IOS
using CsjAds.iOS.Binding;
using Foundation;

namespace CsjAds.Platforms.iOS;

/// <summary>
/// C# implementation of the native CsjAdCallback protocol.
/// Routes ObjC delegate callbacks to C# Actions.
/// </summary>
internal sealed class CsjAdCallbackProxy : NSObject, ICsjAdCallbackDelegate
{
    private readonly Action? _onLoaded;
    private readonly Action<nint, string?>? _onFailed;
    private readonly Action? _onShown;
    private readonly Action? _onClicked;
    private readonly Action? _onClosed;
    private readonly Action<string?, nint, bool>? _onReward;

    public CsjAdCallbackProxy(
        Action? onLoaded = null,
        Action<nint, string?>? onFailed = null,
        Action? onShown = null,
        Action? onClicked = null,
        Action? onClosed = null,
        Action<string?, nint, bool>? onReward = null)
    {
        _onLoaded = onLoaded;
        _onFailed = onFailed;
        _onShown = onShown;
        _onClicked = onClicked;
        _onClosed = onClosed;
        _onReward = onReward;
    }

    public void AdDidLoad() => _onLoaded?.Invoke();

    public void AdDidFail(nint code, string message) => _onFailed?.Invoke(code, message);

    public void AdDidShow() => _onShown?.Invoke();

    public void AdDidClick() => _onClicked?.Invoke();

    public void AdDidClose() => _onClosed?.Invoke();

    public void RewardDidVerify(string rewardName, nint rewardAmount, bool verified)
        => _onReward?.Invoke(rewardName, rewardAmount, verified);
}
#endif
