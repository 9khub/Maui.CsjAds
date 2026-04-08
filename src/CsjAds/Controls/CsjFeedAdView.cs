namespace CsjAds;

/// <summary>
/// Cross-platform MAUI control for displaying a single feed ad from a pre-loaded batch.
/// Set <see cref="NativeAdReference"/> to the rendered native view object.
/// </summary>
public class CsjFeedAdView : View
{
    public static readonly BindableProperty NativeAdReferenceProperty =
        BindableProperty.Create(nameof(NativeAdReference), typeof(object), typeof(CsjFeedAdView), null);

    /// <summary>
    /// The pre-rendered native ad view (Android.Views.View on Android).
    /// Set by the service layer after batch loading.
    /// </summary>
    public object? NativeAdReference
    {
        get => GetValue(NativeAdReferenceProperty);
        set => SetValue(NativeAdReferenceProperty, value);
    }

    public event EventHandler<AdEventArgs>? OnAdLoaded;
    public event EventHandler<AdErrorEventArgs>? OnAdFailed;
    public event EventHandler<AdEventArgs>? OnAdClicked;

    internal void RaiseAdLoaded() => SafeRaise(() => OnAdLoaded?.Invoke(this, new AdEventArgs()));
    internal void RaiseAdFailed(AdError error) => SafeRaise(() => OnAdFailed?.Invoke(this, new AdErrorEventArgs(error)));
    internal void RaiseAdClicked() => SafeRaise(() => OnAdClicked?.Invoke(this, new AdEventArgs()));

    private static void SafeRaise(Action invoke)
    {
        try { invoke(); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CsjAds] CsjFeedAdView subscriber error: {ex}");
        }
    }
}
