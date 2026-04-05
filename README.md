# CsjAds.Maui

Cross-platform .NET MAUI binding library for the **CSJ (穿山甲/Pangle) Ads SDK** by ByteDance.

Supports **Android** and **iOS** with a unified C# API.

## Features

- **Rewarded Video** — highest eCPM, user watches full video for in-app reward
- **Interstitial (Full-Screen Video)** — scene transitions
- **Banner** — inline banner ads via XAML control
- **Splash (Open Screen)** — app launch ads
- Privacy-compliant two-step initialization
- MAUI dependency injection integration
- Cross-platform `CsjBannerView` XAML control

## Quick Start

### 1. Install

```
dotnet add package CsjAds.Maui
```

### 2. Register in MauiProgram.cs

```csharp
builder.UseCsjAds(config =>
{
    config.AppId = "your_csj_app_id";
    config.AppName = "YourApp";
    config.IsDebug = true; // false in production

    config.Privacy.AllowPersonalizedAd = true;
    config.Privacy.AllowLocation = false;
});
```

### 3. Initialize SDK (after user privacy consent)

```csharp
var adService = serviceProvider.GetRequiredService<ICsjAdService>();
bool success = await adService.StartAsync();
```

### 4. Show Ads

**Rewarded Video:**
```csharp
var ad = adService.CreateRewardedVideoAd("your_slot_id");
ad.OnRewardVerified += (_, args) =>
    Console.WriteLine($"Reward: {args.Reward.RewardName} x{args.Reward.RewardAmount}");
ad.OnAdClosed += (_, _) => ad.Dispose();

await ad.LoadAsync();
ad.Show();
```

**Banner (XAML):**
```xml
<csj:CsjBannerView SlotId="your_slot_id" HeightRequest="50" />
```

**Interstitial:**
```csharp
var ad = adService.CreateInterstitialAd("your_slot_id");
await ad.LoadAsync();
ad.Show();
```

**Splash:**
```csharp
var ad = adService.CreateSplashAd("your_slot_id");
ad.TimeoutMilliseconds = 3000;
await ad.LoadAsync();
ad.Show();
```

## Architecture

This library uses the **Native Library Interop** approach:

```
┌──────────────────────────────────────────────────┐
│  Your MAUI App                                   │
│  └── ICsjAdService (DI)                          │
│      └── CsjBannerView (XAML)                    │
├──────────────────────────────────────────────────┤
│  CsjAds (net10.0 + net10.0-android + net10.0-ios)  │
│  ├── Abstractions/ (shared interfaces)           │
│  ├── Platforms/Android/ (Android impl)           │
│  └── Platforms/iOS/ (iOS impl)                   │
├──────────────────────────────────────────────────┤
│  Native Wrappers (thin layer)                    │
│  ├── Java wrapper → CsjAds.Android.Binding       │
│  └── ObjC wrapper → CsjAds.iOS.Binding           │
├──────────────────────────────────────────────────┤
│  CSJ SDK (com.pangle.cn:ads-sdk-pro / Ads-CN)   │
└──────────────────────────────────────────────────┘
```

Instead of binding the entire CSJ SDK (which has hundreds of classes and heavy obfuscation), we bind a thin native wrapper that exposes only the APIs needed for each ad format. This makes the binding maintainable and resilient to SDK version updates.

## Building from Source

### Prerequisites

- .NET 10.0 SDK
- Android SDK (API 34)
- Xcode 15+ (for iOS, macOS only)
- JDK 11+ (for Android native wrapper)

### Build

```bash
# Build the .NET library (net10.0 target only — shared abstractions)
dotnet build src/CsjAds/CsjAds.csproj -p:TargetFramework=net10.0

# Build the Android native wrapper (requires Android SDK + Gradle)
cd native/android && ./gradlew :csjadswrapper:assembleRelease

# Build the iOS native wrapper (requires Xcode, macOS only)
cd native/ios/CsjAdsWrapper && pod install && xcodebuild -workspace CsjAdsWrapper.xcworkspace -scheme CsjAdsWrapper -sdk iphoneos

# Build everything for Android
dotnet build src/CsjAds/CsjAds.csproj -f net10.0-android

# Pack NuGet
dotnet pack src/CsjAds/CsjAds.csproj -c Release
```

## SDK Versions

| Platform | SDK | Version | Source |
|----------|-----|---------|--------|
| Android | `com.pangle.cn:ads-sdk-pro` | 7.4.2.2 | [ByteDance Maven](https://artifact.bytedance.com/repository/pangle) |
| iOS | `Ads-CN` | ~> 6.6 | [CocoaPods](https://cocoapods.org/pods/Ads-CN) |

## Android Permissions

Add to your `AndroidManifest.xml`:

```xml
<!-- Required -->
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />

<!-- Optional: improves ad targeting -->
<uses-permission android:name="android.permission.ACCESS_WIFI_STATE" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.WAKE_LOCK" />
```

## Privacy Compliance

The SDK follows a **two-step initialization** pattern:

1. `Configure()` — set up config, safe to call before user consent
2. `StartAsync()` — start the SDK, call **only after** user accepts privacy policy

Control data collection via `CsjPrivacyConfiguration`:
- `AllowPersonalizedAd` — personalized recommendations
- `AllowLocation` — location data usage
- `AllowPhoneState` — device identifier access
- `CustomDeviceId` — custom OAID for tracking

## License

MIT
