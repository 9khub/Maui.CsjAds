namespace CsjAds;

/// <summary>
/// A rewarded video ad — the user watches a full video in exchange for an in-app reward.
/// Typically offers the highest eCPM among all ad formats.
/// </summary>
public interface ICsjRewardedVideoAd : IDisposable
{
    /// <summary>
    /// Load the ad from the network. Must be called before <see cref="Show"/>.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Show the loaded ad. The ad must be successfully loaded first.
    /// </summary>
    void Show();

    /// <summary>
    /// Whether the ad has been loaded and is ready to show.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>Raised when the ad is successfully loaded.</summary>
    event EventHandler<AdEventArgs>? OnAdLoaded;

    /// <summary>Raised when the ad fails to load or show.</summary>
    event EventHandler<AdErrorEventArgs>? OnAdFailed;

    /// <summary>Raised when the ad is displayed to the user.</summary>
    event EventHandler<AdEventArgs>? OnAdShown;

    /// <summary>Raised when the user clicks the ad.</summary>
    event EventHandler<AdEventArgs>? OnAdClicked;

    /// <summary>Raised when the ad is closed.</summary>
    event EventHandler<AdEventArgs>? OnAdClosed;

    /// <summary>Raised when the reward is verified (user watched enough of the video).</summary>
    event EventHandler<RewardEventArgs>? OnRewardVerified;
}
