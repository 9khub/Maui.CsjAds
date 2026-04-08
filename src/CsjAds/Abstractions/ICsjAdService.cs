namespace CsjAds;

/// <summary>
/// Primary service for interacting with the CSJ ad SDK.
/// Follows the two-step initialization pattern for privacy compliance:
/// 1. <see cref="Configure"/> — sets up the SDK config (safe before user consent)
/// 2. <see cref="StartAsync"/> — starts the SDK (call only after user consent)
/// </summary>
public interface ICsjAdService
{
    /// <summary>
    /// Whether the SDK has been successfully initialized and started.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Step 1: Configure the SDK with the given settings.
    /// Safe to call before user privacy consent.
    /// </summary>
    void Configure(CsjAdConfiguration configuration);

    /// <summary>
    /// Step 2: Start the SDK. Call only after the user has granted privacy consent.
    /// </summary>
    /// <returns>True if initialization succeeded.</returns>
    Task<bool> StartAsync();

    /// <summary>
    /// Create a rewarded video ad loader for the given slot.
    /// </summary>
    ICsjRewardedVideoAd CreateRewardedVideoAd(string slotId);

    /// <summary>
    /// Create an interstitial (full-screen) ad loader for the given slot.
    /// </summary>
    ICsjInterstitialAd CreateInterstitialAd(string slotId);

    /// <summary>
    /// Create a splash (open screen) ad loader for the given slot.
    /// </summary>
    ICsjSplashAd CreateSplashAd(string slotId);

    /// <summary>
    /// Create a feed (native express) ad batch loader for the given slot.
    /// </summary>
    ICsjFeedAd CreateFeedAd(string slotId, int adCount = 3, int width = 0, int height = 0);

    /// <summary>
    /// Launch the CSJ Debug Tool (可视化测试工具).
    /// </summary>
    /// <param name="url">The URL obtained from the CSJ portal (usually via QR code).</param>
    void LaunchDebugTool(string url);
}
