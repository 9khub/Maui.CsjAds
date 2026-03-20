namespace CsjAds;

/// <summary>
/// A banner ad displayed inline within the app UI.
/// This interface is used internally by the banner view handler.
/// Consumers should use <see cref="CsjBannerView"/> in XAML instead.
/// </summary>
public interface ICsjBannerAd : IDisposable
{
    /// <summary>
    /// Load the banner ad.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Destroy and clean up the banner ad.
    /// </summary>
    void Destroy();

    /// <summary>Raised when the ad is successfully loaded.</summary>
    event EventHandler<AdEventArgs>? OnAdLoaded;

    /// <summary>Raised when the ad fails to load.</summary>
    event EventHandler<AdErrorEventArgs>? OnAdFailed;

    /// <summary>Raised when the user clicks the ad.</summary>
    event EventHandler<AdEventArgs>? OnAdClicked;

    /// <summary>Raised when the ad is closed by the user.</summary>
    event EventHandler<AdEventArgs>? OnAdClosed;
}
