namespace CsjAds;

/// <summary>
/// An interstitial (full-screen video) ad displayed between content transitions.
/// </summary>
public interface ICsjInterstitialAd : IDisposable
{
    /// <summary>
    /// Load the ad from the network.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Show the loaded ad.
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
}
