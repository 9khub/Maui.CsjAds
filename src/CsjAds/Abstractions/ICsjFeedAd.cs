namespace CsjAds;

/// <summary>
/// A batch of feed (native express) ads for insertion into a scrolling list.
/// </summary>
public interface ICsjFeedAd : IDisposable
{
    /// <summary>Number of ads that rendered successfully after <see cref="LoadAsync"/>.</summary>
    int LoadedCount { get; }

    /// <summary>Load and render a batch of express ads from the network.</summary>
    Task LoadAsync();

    /// <summary>
    /// Get the rendered native view at the given index (platform-specific object).
    /// Returns null if the index is out of range or render failed.
    /// </summary>
    object? GetRenderedView(int index);

    /// <summary>Whether ads were loaded in self-render mode (image/title data).</summary>
    bool IsSelfRenderMode { get; }

    /// <summary>Get ad title by index (self-render mode).</summary>
    string? GetAdTitle(int index);

    /// <summary>Get ad image URL by index (self-render mode).</summary>
    string? GetAdImageUrl(int index);

    /// <summary>Destroy all loaded ads and release native resources.</summary>
    void Destroy();

    /// <summary>Raised when all ads have been rendered (some may have failed).</summary>
    event EventHandler<AdEventArgs>? OnAdsLoaded;

    /// <summary>Raised when the batch load fails entirely.</summary>
    event EventHandler<AdErrorEventArgs>? OnAdsFailed;

    /// <summary>Raised when any ad in the batch is clicked.</summary>
    event EventHandler<AdEventArgs>? OnAdClicked;
}
