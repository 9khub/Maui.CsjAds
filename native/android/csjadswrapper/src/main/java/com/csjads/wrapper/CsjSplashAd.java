package com.csjads.wrapper;

import android.app.Activity;
import android.content.Context;

import com.bytedance.sdk.openadsdk.AdSlot;
import com.bytedance.sdk.openadsdk.TTAdNative;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.TTSplashAd;

/**
 * Thin wrapper for CSJ Splash (Open Screen) Ad.
 */
public class CsjSplashAd {

    private final String slotId;
    private final int timeoutMs;
    private TTSplashAd loadedAd;
    private CsjAdCallback callback;

    /**
     * @param slotId    Ad slot ID from CSJ platform
     * @param timeoutMs Maximum wait time in milliseconds (0 for SDK default)
     */
    public CsjSplashAd(String slotId, int timeoutMs) {
        this.slotId = slotId;
        this.timeoutMs = timeoutMs > 0 ? timeoutMs : 3000;
    }

    public void load(Context context, final CsjAdCallback callback) {
        this.callback = callback;

        TTAdNative adNative = TTAdSdk.getAdManager().createAdNative(context);

        AdSlot adSlot = new AdSlot.Builder()
                .setCodeId(slotId)
                .build();

        adNative.loadSplashAd(adSlot, new TTAdNative.CSJSplashAdListener() {
            @Override
            public void onSplashLoadSuccess(TTSplashAd ad) {
                loadedAd = ad;
                // Don't notify yet — wait for onSplashRenderSuccess
            }

            @Override
            public void onSplashLoadFail(com.bytedance.sdk.openadsdk.CSJAdError error) {
                if (callback != null) {
                    int code = error != null ? error.getCode() : -1;
                    String msg = error != null ? error.getMsg() : "Unknown error";
                    callback.onAdFailed(code, msg);
                }
            }

            @Override
            public void onSplashRenderSuccess(TTSplashAd ad) {
                loadedAd = ad;
                if (callback != null) callback.onAdLoaded();
            }

            @Override
            public void onSplashRenderFail(TTSplashAd ad, com.bytedance.sdk.openadsdk.CSJAdError error) {
                if (callback != null) {
                    int code = error != null ? error.getCode() : -1;
                    String msg = error != null ? error.getMsg() : "Render failed";
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

        loadedAd.setSplashAdListener(new TTSplashAd.SplashAdListener() {
            @Override
            public void onSplashAdShow(TTSplashAd ad) {
                if (callback != null) callback.onAdShow();
            }

            @Override
            public void onSplashAdClick(TTSplashAd ad) {
                if (callback != null) callback.onAdClicked();
            }

            @Override
            public void onSplashAdClose(TTSplashAd ad, int closeType) {
                if (callback != null) callback.onAdClosed();
                loadedAd = null;
            }
        });

        loadedAd.showSplashView(activity.getWindow().getDecorView().findViewById(android.R.id.content));
    }
}
