package com.csjads.wrapper;

import android.content.Context;
import android.view.View;
import android.view.ViewGroup;

import com.bytedance.sdk.openadsdk.AdSlot;
import com.bytedance.sdk.openadsdk.TTAdNative;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.TTNativeExpressAd;
import com.bytedance.sdk.openadsdk.mediation.ad.MediationAdSlot;

import java.util.List;

/**
 * Thin wrapper for CSJ Banner (Express) Ad.
 * Returns a native Android View for embedding in the MAUI handler.
 */
public class CsjBannerAd {

    private final String slotId;
    private final int width;
    private final int height;
    private TTNativeExpressAd loadedAd;
    private CsjAdCallback callback;

    /**
     * @param slotId Ad slot ID from CSJ platform
     * @param width  Desired width in dp (0 for adaptive)
     * @param height Desired height in dp (0 for adaptive)
     */
    public CsjBannerAd(String slotId, int width, int height) {
        this.slotId = slotId;
        // Keep 0 values for adaptive mode (SDK computes the best size).
        this.width = width;
        this.height = height;
    }

    public void load(Context context, final ViewGroup container, final CsjAdCallback callback) {
        this.callback = callback;

        TTAdNative adNative = TTAdSdk.getAdManager().createAdNative(context);

        // Use system metrics for DIP calculations.
        android.util.DisplayMetrics dm = android.content.res.Resources.getSystem().getDisplayMetrics();
        int finalWidth = width > 0 ? width : (int)(dm.widthPixels / dm.density);
        int finalHeight = height > 0 ? height : 0;

        android.util.Log.d("CsjAdsWrapper",
                "Requesting Banner ad, slotId=" + slotId + ", size=" + finalWidth + "x" + finalHeight + "dp");

        int imageWidthPx = finalWidth > 0 ? (int) (finalWidth * dm.density) : dm.widthPixels;
        int imageHeightPx = finalHeight > 0 ? (int) (finalHeight * dm.density) : 0;

        AdSlot.Builder slotBuilder = new AdSlot.Builder()
                .setCodeId(slotId)
                .setImageAcceptedSize(imageWidthPx, imageHeightPx)
                .setExpressViewAcceptedSize(finalWidth, finalHeight)
                .setAdCount(1);
        if (CsjSdkWrapper.isUseMediation()) {
            MediationAdSlot mediation = new MediationAdSlot.Builder()
                    .setExtraObject("show_adn_load_error_detail", Boolean.TRUE)
                    .build();
            slotBuilder.setMediationAdSlot(mediation);
        }
        AdSlot adSlot = slotBuilder.build();

        adNative.loadBannerExpressAd(adSlot, new TTAdNative.NativeExpressAdListener() {
            @Override
            public void onError(int code, String message) {
                android.util.Log.e("CsjAdsWrapper",
                        "Banner load failed, slotId=" + slotId + ", code=" + code + ", message=" + message);
                if (callback != null) {
                    callback.onAdFailed(code, message);
                }
            }

            @Override
            public void onNativeExpressAdLoad(List<TTNativeExpressAd> ads) {
                if (ads == null || ads.isEmpty()) {
                    android.util.Log.e("CsjAdsWrapper",
                            "Banner load returned empty ads, slotId=" + slotId);
                    if (callback != null) {
                        callback.onAdFailed(-1, "No ad returned");
                    }
                    return;
                }

                loadedAd = ads.get(0);

                loadedAd.setExpressInteractionListener(new TTNativeExpressAd.ExpressAdInteractionListener() {
                    @Override
                    public void onAdClicked(View view, int type) {
                        if (callback != null) callback.onAdClicked();
                    }

                    @Override
                    public void onAdShow(View view, int type) {
                        if (callback != null) callback.onAdShow();
                    }

                    @Override
                    public void onRenderFail(View view, String msg, int code) {
                        android.util.Log.e("CsjAdsWrapper",
                                "Banner render failed, slotId=" + slotId + ", code=" + code + ", message=" + msg);
                        if (callback != null) callback.onAdFailed(code, msg);
                    }

                    @Override
                    public void onRenderSuccess(View view, float w, float h) {
                        // 部分机型/SDK（如 Android 16 + 聚合）会在 onRenderSuccess 传入 null，
                        // 直接 addView 会抛出 IllegalArgumentException 导致进程崩溃。
                        if (view == null) {
                            android.util.Log.e("CsjAdsWrapper",
                                    "Banner onRenderSuccess: express view is null, slotId=" + slotId);
                            if (callback != null) {
                                callback.onAdFailed(-1001, "Express render returned null view");
                            }
                            return;
                        }
                        if (container == null) {
                            android.util.Log.e("CsjAdsWrapper",
                                    "Banner onRenderSuccess: container is null, slotId=" + slotId);
                            if (callback != null) {
                                callback.onAdFailed(-1002, "Banner container is null");
                            }
                            return;
                        }
                        try {
                            if (!container.isAttachedToWindow()) {
                                android.util.Log.w("CsjAdsWrapper",
                                        "Banner container not attached, slotId=" + slotId + " — addView may still proceed");
                            }
                            android.view.ViewParent parent = view.getParent();
                            if (parent instanceof ViewGroup) {
                                ((ViewGroup) parent).removeView(view);
                            }
                            container.removeAllViews();
                            container.addView(view);
                            if (callback != null) {
                                callback.onAdLoaded();
                            }
                        } catch (Exception e) {
                            android.util.Log.e("CsjAdsWrapper",
                                    "Banner addView failed, slotId=" + slotId + ": " + e.getMessage(), e);
                            if (callback != null) {
                                String msg = e.getMessage() != null ? e.getMessage() : "addView failed";
                                callback.onAdFailed(-1003, msg);
                            }
                        }
                    }
                });

                loadedAd.setDislikeCallback(null, new com.bytedance.sdk.openadsdk.TTAdDislike.DislikeInteractionCallback() {
                    @Override
                    public void onShow() {
                    }

                    @Override
                    public void onSelected(int position, String value, boolean enforce) {
                        try {
                            if (container != null) {
                                container.removeAllViews();
                            }
                        } catch (Exception e) {
                            android.util.Log.e("CsjAdsWrapper", "Banner dislike removeAllViews: " + e.getMessage());
                        }
                        if (callback != null) callback.onAdClosed();
                    }

                    @Override
                    public void onCancel() {
                    }
                });

                // Render on main thread to avoid possible thread-scheduling issues.
                new android.os.Handler(android.os.Looper.getMainLooper()).post(() -> {
                    TTNativeExpressAd ad = loadedAd;
                    if (ad == null) {
                        android.util.Log.e("CsjAdsWrapper",
                                "Banner render skipped: loadedAd is null, slotId=" + slotId);
                        if (callback != null) {
                            callback.onAdFailed(-1004, "Express ad destroyed before render");
                        }
                        return;
                    }
                    try {
                        ad.render();
                    } catch (Throwable t) {
                        android.util.Log.e("CsjAdsWrapper", "Banner render() threw, slotId=" + slotId, t);
                        if (callback != null) {
                            String m = t.getMessage();
                            callback.onAdFailed(-1005, m != null ? m : "render() failed");
                        }
                    }
                });
            }
        });
    }

    public void destroy() {
        if (loadedAd != null) {
            loadedAd.destroy();
            loadedAd = null;
        }
    }
}
