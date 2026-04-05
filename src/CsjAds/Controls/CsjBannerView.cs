namespace CsjAds;

/// <summary>
/// Cross-platform MAUI control for displaying a CSJ banner ad.
/// Use in XAML: <![CDATA[<csj:CsjBannerView SlotId="your_slot_id" AdSize="{x:Static csj:AdSize.Banner300x250}" />]]>
/// </summary>
public class CsjBannerView : View
{
    public static readonly BindableProperty SlotIdProperty =
        BindableProperty.Create(nameof(SlotId), typeof(string), typeof(CsjBannerView), string.Empty);

    public static readonly BindableProperty AdSizeProperty =
        BindableProperty.Create(nameof(AdSize), typeof(AdSize), typeof(CsjBannerView), AdSize.Banner320x50);

    public static readonly BindableProperty RefreshIntervalSecondsProperty =
        BindableProperty.Create(nameof(RefreshIntervalSeconds), typeof(int), typeof(CsjBannerView), 30);

    /// <summary>
    /// The ad slot ID from the CSJ platform.
    /// </summary>
    public string SlotId
    {
        get => (string)GetValue(SlotIdProperty);
        set => SetValue(SlotIdProperty, value);
    }

    /// <summary>
    /// The desired banner size. Defaults to 320x50.
    /// </summary>
    public AdSize AdSize
    {
        get => (AdSize)GetValue(AdSizeProperty);
        set => SetValue(AdSizeProperty, value);
    }

    /// <summary>
    /// Auto-refresh interval in seconds. Set to 0 to disable auto-refresh.
    /// Default is 30 seconds.
    /// </summary>
    public int RefreshIntervalSeconds
    {
        get => (int)GetValue(RefreshIntervalSecondsProperty);
        set => SetValue(RefreshIntervalSecondsProperty, value);
    }

    // Events
    public event EventHandler<AdEventArgs>? OnAdLoaded;
    public event EventHandler<AdErrorEventArgs>? OnAdFailed;
    public event EventHandler<AdEventArgs>? OnAdClicked;
    public event EventHandler<AdEventArgs>? OnAdClosed;

    // Internal methods for handlers to raise events（订阅方异常不得冒泡至 JNI/原生回调线程）
    internal void RaiseAdLoaded() => SafeRaise(() => OnAdLoaded?.Invoke(this, new AdEventArgs()));
    internal void RaiseAdFailed(AdError error) => SafeRaise(() => OnAdFailed?.Invoke(this, new AdErrorEventArgs(error)));
    internal void RaiseAdClicked() => SafeRaise(() => OnAdClicked?.Invoke(this, new AdEventArgs()));
    internal void RaiseAdClosed() => SafeRaise(() => OnAdClosed?.Invoke(this, new AdEventArgs()));

    private static void SafeRaise(Action invoke)
    {
        try
        {
            invoke();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CsjAds] CsjBannerView subscriber error: {ex}");
        }
    }
}
