namespace CsjAds;

/// <summary>
/// A splash (open screen) ad shown during app launch.
/// </summary>
public interface ICsjSplashAd : IDisposable
{
    /// <summary>
    /// Load the splash ad from the network.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Show the splash ad. Typically renders over the current window.
    /// </summary>
    void Show();

    /// <summary>
    /// Whether the ad has been loaded and is ready to show.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Maximum time in milliseconds to wait for the ad to load.
    /// Default is 3000ms. Set to 0 for no timeout.
    /// </summary>
    int TimeoutMilliseconds { get; set; }

    /// <summary>Raised when the ad is successfully loaded.</summary>
    event EventHandler<AdEventArgs>? OnAdLoaded;

    /// <summary>Raised when the ad fails to load or show.</summary>
    event EventHandler<AdErrorEventArgs>? OnAdFailed;

    /// <summary>Raised when the ad is displayed to the user.</summary>
    event EventHandler<AdEventArgs>? OnAdShown;

    /// <summary>Raised when the user clicks the ad.</summary>
    event EventHandler<AdEventArgs>? OnAdClicked;

    /// <summary>Raised when the ad is closed (skipped or timed out).</summary>
    event EventHandler<AdEventArgs>? OnAdClosed;
}
