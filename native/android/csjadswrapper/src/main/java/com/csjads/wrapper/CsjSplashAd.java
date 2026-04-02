package com.csjads.wrapper;

import android.app.Activity;
import android.content.Context;

import com.bytedance.sdk.openadsdk.AdSlot;
import com.bytedance.sdk.openadsdk.CSJAdError;
import com.bytedance.sdk.openadsdk.CSJSplashAd;
import com.bytedance.sdk.openadsdk.TTAdNative;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.mediation.ad.MediationAdSlot;

/**
 * Thin wrapper for CSJ Splash (Open Screen) Ad.
 * Uses CSJSplashAd (SDK 7.x API).
 */
public class CsjSplashAd {

    private final String slotId;
    private final int timeoutMs;
    private CSJSplashAd loadedAd;
    private CsjAdCallback callback;

    /**
     * @param slotId    Ad slot ID from CSJ platform
     * @param timeoutMs Maximum wait time in milliseconds (0 for SDK default)
     */
    public CsjSplashAd(String slotId, int timeoutMs) {
        this.slotId = slotId;
        this.timeoutMs = timeoutMs > 0 ? timeoutMs : 5000;
    }

    public void load(Context context, final CsjAdCallback callback) {
        this.callback = callback;

        TTAdNative adNative = TTAdSdk.getAdManager().createAdNative(context);

        // Use context-specific metrics for better accuracy
        android.util.DisplayMetrics dm = context.getResources().getDisplayMetrics();
        float widthDp = (float) dm.widthPixels / dm.density;
        float heightDp = (float) dm.heightPixels / dm.density;

        android.util.Log.d("CsjAdsWrapper",
                "Requesting Splash ad, slotId=" + slotId + ", size=" + widthDp + "x" + heightDp
                        + " dp (" + dm.widthPixels + "x" + dm.heightPixels + " px, density=" + dm.density + ")");

        AdSlot.Builder slotBuilder = new AdSlot.Builder()
                .setCodeId(slotId)
                .setImageAcceptedSize(dm.widthPixels, dm.heightPixels)
                .setExpressViewAcceptedSize(widthDp, heightDp)
                .setAdCount(1);
        if (CsjSdkWrapper.isUseMediation()) {
            MediationAdSlot mediation = new MediationAdSlot.Builder()
                    .setExtraObject("show_adn_load_error_detail", Boolean.TRUE)
                    .build();
            slotBuilder.setMediationAdSlot(mediation);
        }
        AdSlot adSlot = slotBuilder.build();

        adNative.loadSplashAd(adSlot, new TTAdNative.CSJSplashAdListener() {
            @Override
            public void onSplashLoadSuccess(CSJSplashAd ad) {
                loadedAd = ad;
                // Don't notify yet — wait for onSplashRenderSuccess
            }

            @Override
            public void onSplashLoadFail(CSJAdError error) {
                if (callback != null) {
                    int code = error != null ? error.getCode() : -1;
                    String msg = error != null ? error.getMsg() : "Unknown error";
                    android.util.Log.e("CsjAdsWrapper",
                            "Splash load failed, slotId=" + slotId + ", code=" + code + ", message=" + msg);
                    callback.onAdFailed(code, msg);
                }
            }

            @Override
            public void onSplashRenderSuccess(CSJSplashAd ad) {
                loadedAd = ad;
                if (callback != null) callback.onAdLoaded();
            }

            @Override
            public void onSplashRenderFail(CSJSplashAd ad, CSJAdError error) {
                if (callback != null) {
                    int code = error != null ? error.getCode() : -1;
                    String msg = error != null ? error.getMsg() : "Render failed";
                    android.util.Log.e("CsjAdsWrapper",
                            "Splash render failed, slotId=" + slotId + ", code=" + code + ", message=" + msg);
                    callback.onAdFailed(code, msg);
                }
            }
        }, timeoutMs);
    }

    public void show(Activity activity) {
        if (loadedAd == null) {
            if (callback != null) {
                callback.onAdFailed(-1, "Splash ad not loaded");
            }
            return;
        }

        loadedAd.setSplashAdListener(new CSJSplashAd.SplashAdListener() {
            @Override
            public void onSplashAdShow(CSJSplashAd ad) {
                if (callback != null) callback.onAdShow();
            }

            @Override
            public void onSplashAdClick(CSJSplashAd ad) {
                if (callback != null) callback.onAdClicked();
            }

            @Override
            public void onSplashAdClose(CSJSplashAd ad, int closeType) {
                if (callback != null) callback.onAdClosed();
                loadedAd = null;
            }
        });

        loadedAd.showSplashView(activity.getWindow().getDecorView().findViewById(android.R.id.content));
    }
}
