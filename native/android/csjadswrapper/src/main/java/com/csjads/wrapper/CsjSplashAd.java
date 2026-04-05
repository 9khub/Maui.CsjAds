package com.csjads.wrapper;

import android.app.Activity;
import android.content.Context;
import android.view.View;
import android.view.ViewGroup;

import com.bytedance.sdk.openadsdk.AdSlot;
import com.bytedance.sdk.openadsdk.CSJAdError;
import com.bytedance.sdk.openadsdk.CSJSplashAd;
import com.bytedance.sdk.openadsdk.TTAdNative;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.mediation.ad.MediationAdSlot;

/**
 * Thin wrapper for CSJ Splash (Open Screen) Ad.
 * Uses CSJSplashAd (SDK 7.x API).
 * <p>
 * 展示与跳过等回调必须在主线程执行，否则 MAUI/DecorView 与穿山甲模板移除并发易导致崩溃。
 */
public class CsjSplashAd {

    private final String slotId;
    private final int timeoutMs;
    private CSJSplashAd loadedAd;
    private CsjAdCallback callback;
    /** 最近一次 show 的 Activity，用于将 SDK 回调派发到 UI 线程 */
    private Activity hostActivity;

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
                if (ad == null) {
                    android.util.Log.e("CsjAdsWrapper",
                            "Splash onSplashRenderSuccess: ad is null, slotId=" + slotId);
                    if (callback != null) {
                        callback.onAdFailed(-1, "Splash render success but ad instance is null");
                    }
                    return;
                }
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

    private boolean canUseActivity(Activity a) {
        if (a == null) return false;
        if (a.isFinishing()) return false;
        return !a.isDestroyed();
    }

    /**
     * 将开屏生命周期回调切到 Activity 主线程再进入 JNI/.NET，避免跳过按钮在非 UI 线程触发崩溃。
     */
    private void dispatchSplashCallback(Runnable r) {
        final Activity a = hostActivity;
        if (a != null && canUseActivity(a)) {
            a.runOnUiThread(() -> {
                try {
                    r.run();
                } catch (Throwable t) {
                    android.util.Log.e("CsjAdsWrapper", "Splash listener callback error", t);
                }
            });
        } else {
            try {
                r.run();
            } catch (Throwable t) {
                android.util.Log.e("CsjAdsWrapper", "Splash listener callback error (no activity)", t);
            }
        }
    }

    public void show(Activity activity) {
        if (activity == null) {
            if (callback != null) {
                callback.onAdFailed(-1, "Activity is null");
            }
            return;
        }

        hostActivity = activity;

        if (loadedAd == null) {
            if (callback != null) {
                callback.onAdFailed(-1, "Splash ad not loaded");
            }
            return;
        }

        activity.runOnUiThread(() -> {
            try {
                if (!canUseActivity(activity)) {
                    if (callback != null) {
                        callback.onAdFailed(-4, "Activity finishing or destroyed");
                    }
                    return;
                }

                final CSJSplashAd adRef = loadedAd;
                if (adRef == null) {
                    if (callback != null) {
                        callback.onAdFailed(-1, "Splash ad not loaded (race)");
                    }
                    return;
                }

                adRef.setSplashAdListener(new CSJSplashAd.SplashAdListener() {
                    @Override
                    public void onSplashAdShow(CSJSplashAd ad) {
                        dispatchSplashCallback(() -> {
                            if (callback != null) {
                                callback.onAdShow();
                            }
                        });
                    }

                    @Override
                    public void onSplashAdClick(CSJSplashAd ad) {
                        dispatchSplashCallback(() -> {
                            if (callback != null) {
                                callback.onAdClicked();
                            }
                        });
                    }

                    @Override
                    public void onSplashAdClose(CSJSplashAd ad, int closeType) {
                        android.util.Log.d("CsjAdsWrapper",
                                "onSplashAdClose slotId=" + slotId + " closeType=" + closeType);
                        dispatchSplashCallback(() -> {
                            try {
                                if (callback != null) {
                                    callback.onAdClosed();
                                }
                            } finally {
                                loadedAd = null;
                            }
                        });
                    }
                });

                View root = activity.getWindow().getDecorView().findViewById(android.R.id.content);
                if (!(root instanceof ViewGroup)) {
                    android.util.Log.e("CsjAdsWrapper", "Splash show: content root is not ViewGroup");
                    if (callback != null) {
                        callback.onAdFailed(-2, "Content root is not ViewGroup");
                    }
                    loadedAd = null;
                    return;
                }

                adRef.showSplashView((ViewGroup) root);
            } catch (Throwable t) {
                android.util.Log.e("CsjAdsWrapper", "Splash show failed", t);
                if (callback != null) {
                    String m = t.getMessage();
                    callback.onAdFailed(-3, m != null ? m : "show exception");
                }
                loadedAd = null;
            }
        });
    }
}
