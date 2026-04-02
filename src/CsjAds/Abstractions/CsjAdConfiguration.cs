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
    /// GroMore 聚合（<c>TTAdConfig.useMediation</c>）。工程使用 <c>mediation-sdk</c> 且广告位为 GroMore 聚合位时应为 true（与后台瀑布一致）。
    /// 若代码位为纯穿山甲直连位且使用 <c>ads-sdk-pro</c>，则为 false。错配时易出现瀑布首层失败（如 602 / 20005）。
    /// </summary>
    public bool UseMediation { get; set; }

    /// <summary>
    /// Privacy-related configuration for user consent compliance.
    /// </summary>
    public CsjPrivacyConfiguration Privacy { get; set; } = new();
}
