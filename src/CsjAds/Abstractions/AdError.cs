namespace CsjAds;

/// <summary>
/// Represents an error from the CSJ ad SDK.
/// </summary>
public sealed class AdError
{
    public AdError(int code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// SDK-defined error code.
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Human-readable error description.
    /// </summary>
    public string Message { get; }

    public override string ToString() => $"AdError({Code}): {Message}";
}
