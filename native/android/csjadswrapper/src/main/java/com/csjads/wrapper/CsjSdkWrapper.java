package com.csjads.wrapper;

import android.content.Context;
import android.os.Build;

import com.bytedance.sdk.openadsdk.LocationProvider;
import com.bytedance.sdk.openadsdk.TTAdConfig;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.TTCustomController;
import com.bytedance.sdk.openadsdk.mediation.init.IMediationPrivacyConfig;
import com.bytedance.sdk.openadsdk.mediation.init.MediationPrivacyConfig;

/**
 * Thin wrapper around TTAdSdk for .NET binding.
 * Implements the two-step privacy-compliant initialization:
 * 1. init() — configure without starting (safe before consent)
 * 2. start() — start SDK (call after consent)
 *
 * Includes platform capability checks and crash-safe error handling
 * to prevent the CSJ SDK from crashing the host app on unsupported devices.
 */
public final class CsjSdkWrapper {

    private static volatile boolean sInitAttempted = false;
    private static volatile boolean sInitSuccess = false;
    private static volatile boolean sUseMediation = false;
    private static Thread.UncaughtExceptionHandler sOriginalHandler;

    private CsjSdkWrapper() {}

    /** Mirrors last successful {@link #init} <code>useMediation</code> for ad request builders. */
    public static boolean isUseMediation() {
        return sUseMediation;
    }

    /**
     * Check if the current device supports CSJ ads.
     * Returns true only if the primary ABI is arm-based (not an x86 emulator
     * with ARM translation, which causes CSJ native plugin crashes).
     */
    public static boolean isDeviceSupported() {
        String[] abis = Build.SUPPORTED_ABIS;
        if (abis == null || abis.length == 0) return false;

        // Primary ABI is the first entry; if the device is x86/x86_64 with arm translation,
        // the primary ABI will be x86_64 and arm64-v8a will only appear as secondary.
        String primaryAbi = abis[0];
        if (primaryAbi.startsWith("x86")) {
            android.util.Log.w("CsjAdsWrapper",
                    "Primary ABI is " + primaryAbi + " (emulator?), CSJ ads not supported");
            return false;
        }
        return "arm64-v8a".equals(primaryAbi) || "armeabi-v7a".equals(primaryAbi);
    }

    /**
     * Step 1: Pre-consent configuration.
     * Sets up TTAdConfig but does NOT start the SDK or collect data.
     * Returns false if the device is not supported or init fails.
     */
    public static boolean init(
            Context context,
            String appId,
            String appName,
            boolean debug,
            boolean allowPersonalizedAd,
            boolean allowLocation,
            boolean allowPhoneState,
            boolean allowWriteExternal,
            boolean allowWifiState,
            boolean allowAndroidId,
            String androidIdOverride,
            boolean useMediation,
            String customDeviceId) {

        if (!isDeviceSupported()) {
            android.util.Log.w("CsjAdsWrapper", "Device ABI not supported for CSJ SDK, skipping init");
            return false;
        }

        android.util.Log.d("CsjAdsWrapper", "SDK Init: AppId=" + appId + ", useMediation=" + useMediation);
        sUseMediation = useMediation;

        try {
            // Install a safety handler to catch CSJ SDK internal thread crashes
            installCrashGuard();

            final String oaidForSdk = customDeviceId != null ? customDeviceId : "";
            final String androidIdTrimmed = (androidIdOverride != null && !androidIdOverride.trim().isEmpty())
                    ? androidIdOverride.trim()
                    : null;

            TTCustomController customController = new TTCustomController() {
                @Override
                public boolean isCanUseLocation() {
                    return allowLocation;
                }

                @Override
                public LocationProvider getTTLocation() {
                    if (!allowLocation) {
                        return null;
                    }
                    // 与官方聚合示例一致：在授权位置时提供坐标（测试/合规场景可由上层关闭 allowLocation）
                    return new LocationProvider() {
                        @Override
                        public double getLongitude() {
                            return 116.4074;
                        }

                        @Override
                        public double getLatitude() {
                            return 39.9042;
                        }
                    };
                }

                @Override
                public boolean isCanUsePhoneState() {
                    return allowPhoneState;
                }

                @Override
                public boolean isCanUseWifiState() {
                    return allowWifiState;
                }

                @Override
                public boolean isCanUseWriteExternal() {
                    return allowWriteExternal;
                }

                @Override
                public boolean isCanUseAndroidId() {
                    return allowAndroidId;
                }

                @Override
                public String getAndroidId() {
                    if (!allowAndroidId) {
                        return super.getAndroidId();
                    }
                    return androidIdTrimmed != null ? androidIdTrimmed : super.getAndroidId();
                }

                @Override
                public boolean isCanUsePermissionRecordAudio() {
                    return false;
                }

                @Override
                public String getDevOaid() {
                    return oaidForSdk;
                }

                /** 融合 SDK（mediation-sdk）聚合隐私配置，与官方 GroMore 初始化示例一致。 */
                @Override
                public IMediationPrivacyConfig getMediationPrivacyConfig() {
                    return new MediationPrivacyConfig() {
                        @Override
                        public boolean isLimitPersonalAds() {
                            return !allowPersonalizedAd;
                        }

                        @Override
                        public boolean isProgrammaticRecommend() {
                            return allowPersonalizedAd;
                        }
                    };
                }
            };

            TTAdConfig config = new TTAdConfig.Builder()
                    .appId(appId)
                    .appName(appName)
                    .debug(debug)
                    .useMediation(useMediation)
                    .themeStatus(0)
                    .supportMultiProcess(false)
                    .customController(customController)
                    .build();

            TTAdSdk.init(context, config);
            sInitAttempted = true;
            return true;
        } catch (Throwable t) {
            android.util.Log.e("CsjAdsWrapper", "CSJ SDK init failed: " + t.getMessage(), t);
            return false;
        }
    }

    /**
     * Step 2: Start the SDK. Call only after user has consented to privacy policy.
     */
    public static void start(final CsjSdkCallback callback) {
        if (!sInitAttempted) {
            if (callback != null) {
                callback.onFailed(-100, "SDK was not initialized (device not supported or init failed)");
            }
            return;
        }

        try {
            TTAdSdk.start(new TTAdSdk.Callback() {
                @Override
                public void success() {
                    sInitSuccess = true;
                    if (callback != null) {
                        callback.onSuccess();
                    }
                }

                @Override
                public void fail(int code, String msg) {
                    if (callback != null) {
                        callback.onFailed(code, msg);
                    }
                }
            });
        } catch (Throwable t) {
            android.util.Log.e("CsjAdsWrapper", "CSJ SDK start failed: " + t.getMessage(), t);
            if (callback != null) {
                callback.onFailed(-101, "SDK start threw exception: " + t.getMessage());
            }
        }
    }

    /**
     * Check if the SDK is initialized and started.
     */
    public static boolean isInitialized() {
        if (!sInitAttempted) return false;
        try {
            return TTAdSdk.isInitSuccess();
        } catch (Throwable t) {
            return sInitSuccess;
        }
    }

    /**
     * Install a crash guard that catches uncaught exceptions from CSJ SDK internal threads
     * (e.g. "csj_api_main") and logs them instead of crashing the app.
     */
    private static void installCrashGuard() {
        sOriginalHandler = Thread.getDefaultUncaughtExceptionHandler();
        Thread.setDefaultUncaughtExceptionHandler(new Thread.UncaughtExceptionHandler() {
            @Override
            public void uncaughtException(Thread thread, Throwable throwable) {
                String threadName = thread != null ? thread.getName() : "unknown";

                // Only intercept crashes from CSJ SDK internal threads
                if (threadName.startsWith("csj_") || threadName.startsWith("pangle")
                        || threadName.startsWith("byazt") || threadName.startsWith("bytedance")) {
                    android.util.Log.e("CsjAdsWrapper",
                            "Caught CSJ SDK crash on thread [" + threadName + "]: "
                            + throwable.getMessage(), throwable);
                    // Swallow — do NOT propagate to the app
                    return;
                }

                // For non-CSJ threads, delegate to the original handler
                if (sOriginalHandler != null) {
                    sOriginalHandler.uncaughtException(thread, throwable);
                }
            }
        });
    }
}
