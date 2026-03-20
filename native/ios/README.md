# iOS Native Wrapper

This is an Xcode project that builds a thin Objective-C wrapper framework around the CSJ iOS SDK (Ads-CN).

## Prerequisites

- Xcode 15+
- CocoaPods (`gem install cocoapods`)

## Build

```bash
cd CsjAdsWrapper

# Install CSJ SDK via CocoaPods
pod install

# Build the framework
xcodebuild -workspace CsjAdsWrapper.xcworkspace \
           -scheme CsjAdsWrapper \
           -sdk iphoneos \
           -configuration Release \
           BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# Build for simulator (for development)
xcodebuild -workspace CsjAdsWrapper.xcworkspace \
           -scheme CsjAdsWrapper \
           -sdk iphonesimulator \
           -configuration Release \
           BUILD_LIBRARY_FOR_DISTRIBUTION=YES
```

## Deploy to .NET Binding

1. Create an XCFramework from the built frameworks
2. Copy to `../../src/CsjAds.iOS.Binding/`
3. Reference in the binding `.csproj`

## Updating CSJ SDK Version

Edit `CsjAdsWrapper/Podfile`:

```ruby
pod 'Ads-CN', '~> 6.6'
```

Then run `pod update`.
