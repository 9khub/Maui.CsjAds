namespace CsjAds;

/// <summary>
/// Predefined banner ad sizes.
/// </summary>
public readonly struct AdSize
{
    public static readonly AdSize Banner320x50 = new(320, 50);
    public static readonly AdSize Banner300x250 = new(300, 250);
    public static readonly AdSize Banner728x90 = new(728, 90);

    /// <summary>
    /// Adaptive banner that fills the available width.
    /// Height is 0, meaning the SDK will calculate it automatically.
    /// </summary>
    public static readonly AdSize Adaptive = new(0, 0);

    public AdSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }

    public bool IsAdaptive => Width == 0 && Height == 0;

    public override string ToString() =>
        IsAdaptive ? "Adaptive" : $"{Width}x{Height}";
}
