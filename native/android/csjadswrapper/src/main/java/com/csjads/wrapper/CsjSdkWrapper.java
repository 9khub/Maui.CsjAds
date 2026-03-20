package com.csjads.wrapper;

import android.content.Context;

import com.bytedance.sdk.openadsdk.TTAdConfig;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.TTCustomController;

/**
 * Thin wrapper around TTAdSdk for .NET binding.
 * Implements the two-step privacy-compliant initialization:
 * 1. init() — configure without starting (safe before consent)
 * 2. start() — start SDK (call after consent)
 */
public final class CsjSdkWrapper {

    private CsjSdkWrapper() {}

    /**
     * Step 1: Pre-consent configuration.
     * Sets up TTAdConfig but does NOT start the SDK or collect data.
     */
    public static void init(
            Context context,
            String appId,
            String appName,
            boolean debug,
            boolean allowPersonalizedAd,
            boolean allowLocation,
            boolean allowPhoneState,
            boolean allowWriteExternal,
            String customDeviceId) {

        TTCustomController customController = new TTCustomController() {
            @Override
            public boolean isCanUseLocation() {
                return allowLocation;
            }

            @Override
            public boolean isCanUsePhoneState() {
                return allowPhoneState;
            }

            @Override
            public boolean isCanUseWriteExternal() {
                return allowWriteExternal;
            }

            @Override
            public boolean isCanUsePermissionRecordAudio() {
                return false;
            }

            @Override
            public String getDevOaid() {
                return customDeviceId != null ? customDeviceId : "";
            }
        };

        TTAdConfig config = new TTAdConfig.Builder()
                .appId(appId)
                .appName(appName)
                .debug(debug)
                .supportMultiProcess(false)
                .customController(customController)
                .build();

        TTAdSdk.init(context, config);
    }

    /**
     * Step 2: Start the SDK. Call only after user has consented to privacy policy.
     */
    public static void start(final CsjSdkCallback callback) {
        TTAdSdk.start(new TTAdSdk.Callback() {
            @Override
            public void success() {
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
    }

    /**
     * Check if the SDK is initialized and started.
     */
    public static boolean isInitialized() {
        return TTAdSdk.isInitSuccess();
    }
}
