# Android Native Wrapper

This is an Android Studio / Gradle project that builds a thin Java wrapper AAR around the CSJ SDK.

## Build

```bash
# From this directory
./gradlew :csjadswrapper:assembleRelease
```

The output AAR will be at:
```
csjadswrapper/build/outputs/aar/csjadswrapper-release.aar
```

## Deploy to .NET Binding

Copy the built AAR to the .NET binding project:

```bash
cp csjadswrapper/build/outputs/aar/csjadswrapper-release.aar \
   ../../src/CsjAds.Android.Binding/Jars/csjadswrapper.aar
```

Then set the Build Action to `AndroidLibrary` in the binding `.csproj`:

```xml
<ItemGroup>
  <AndroidLibrary Include="Jars\csjadswrapper.aar" />
</ItemGroup>
```

## Updating CSJ SDK Version

Edit `csjadswrapper/build.gradle.kts` and update the version:

```kotlin
implementation("com.pangle.cn:ads-sdk-pro:7.4.2.2")
```

Check latest versions at: https://www.csjplatform.com/supportcenter/5395
