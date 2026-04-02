namespace CsjAds;

/// <summary>
/// Privacy settings controlling what data the CSJ SDK can collect.
/// All defaults are set to the most privacy-preserving option.
/// </summary>
public sealed class CsjPrivacyConfiguration
{
    /// <summary>
    /// Whether the SDK is allowed to use location data for ad targeting.
    /// </summary>
    public bool AllowLocation { get; set; }

    /// <summary>
    /// Whether the SDK is allowed to read phone state (IMEI, etc.).
    /// Only relevant on Android.
    /// </summary>
    public bool AllowPhoneState { get; set; }

    /// <summary>
    /// Whether the SDK is allowed to write to external storage.
    /// Only relevant on Android.
    /// </summary>
    public bool AllowWriteExternal { get; set; }

    /// <summary>
    /// Whether the SDK may use Wi-Fi state (matches <c>TTCustomController.isCanUseWifiState</c>).
    /// </summary>
    public bool AllowWifiState { get; set; } = true;

    /// <summary>
    /// Whether the SDK may use Android ID (matches <c>TTCustomController.isCanUseAndroidId</c>).
    /// </summary>
    public bool AllowAndroidId { get; set; } = true;

    /// <summary>
    /// Optional override for <c>TTCustomController.getAndroidId()</c> (e.g. 与穿山甲后台测试设备 Android ID 一致).
    /// 为空则使用系统默认采集逻辑。
    /// </summary>
    public string? AndroidIdOverride { get; set; }

    /// <summary>
    /// Whether to allow personalized ad recommendations.
    /// </summary>
    public bool AllowPersonalizedAd { get; set; } = true;

    /// <summary>
    /// Custom OAID (Open Anonymous Device Identifier) for ad tracking.
    /// Used as an alternative to IMEI on newer Android devices.
    /// </summary>
    public string? CustomDeviceId { get; set; }
}
