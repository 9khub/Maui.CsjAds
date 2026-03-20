namespace CsjAds;

/// <summary>
/// Configuration for the CSJ ad SDK.
/// </summary>
public sealed class CsjAdConfiguration
{
    /// <summary>
    /// The app ID assigned by the CSJ platform (7-digit number).
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// The app name registered on the CSJ platform.
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// Enable debug/test mode. Set to false in production.
    /// </summary>
    public bool IsDebug { get; set; }

    /// <summary>
    /// Privacy-related configuration for user consent compliance.
    /// </summary>
    public CsjPrivacyConfiguration Privacy { get; set; } = new();
}
