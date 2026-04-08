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
 * GroMore 聚合模式仅支持 Express Banner，无非 Express 替代。
 */
public class CsjBannerAd {

    private final String slotId;
    private final int width;
    private final int height;
    private TTNativeExpressAd loadedAd;
    private CsjAdCallback callback;

    public CsjBannerAd(String slotId, int width, int height) {
        this.slotId = slotId;
        this.width = width;
        this.height = height;
    }

    public void load(Context context, final ViewGroup container, final CsjAdCallback callback) {
        this.callback = callback;

        TTAdNative adNative = TTAdSdk.getAdManager().createAdNative(context);

        android.util.DisplayMetrics dm = context.getResources().getDisplayMetrics();
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
                if (callback != null) callback.onAdFailed(code, message);
            }

            @Override
            public void onNativeExpressAdLoad(List<TTNativeExpressAd> ads) {
                if (ads == null || ads.isEmpty()) {
                    if (callback != null) callback.onAdFailed(-1, "No ad returned");
                    return;
                }

                loadedAd = ads.get(0);

                loadedAd.setExpressInteractionListener(new TTNativeExpressAd.ExpressAdInteractionListener() {
                    @Override public void onAdClicked(View view, int type) { if (callback != null) callback.onAdClicked(); }
                    @Override public void onAdShow(View view, int type) { if (callback != null) callback.onAdShow(); }
                    @Override public void onRenderFail(View view, String msg, int code) {
                        android.util.Log.e("CsjAdsWrapper", "Banner render failed: code=" + code + ", msg=" + msg);
                        if (callback != null) callback.onAdFailed(code, msg);
                    }

                    @Override
                    public void onRenderSuccess(View view, float w, float h) {
                        if (view == null) {
                            try { view = loadedAd.getExpressAdView(); } catch (Throwable ignored) {}
                        }
                        if (view == null) {
                            android.util.Log.e("CsjAdsWrapper",
                                    "Banner: Express render null view, slotId=" + slotId
                                    + " container.attached=" + container.isAttachedToWindow()
                                    + " container.size=" + container.getWidth() + "x" + container.getHeight());
                            if (callback != null) callback.onAdFailed(-1001, "Express render returned null view");
                            return;
                        }
                        try {
                            android.view.ViewParent parent = view.getParent();
                            if (parent instanceof ViewGroup) ((ViewGroup) parent).removeView(view);
                            container.removeAllViews();
                            container.addView(view);
                            if (callback != null) callback.onAdLoaded();
                        } catch (Exception e) {
                            android.util.Log.e("CsjAdsWrapper", "Banner addView failed: " + e.getMessage(), e);
                            if (callback != null) callback.onAdFailed(-1003, e.getMessage() != null ? e.getMessage() : "addView failed");
                        }
                    }
                });

                loadedAd.setDislikeCallback(null, new com.bytedance.sdk.openadsdk.TTAdDislike.DislikeInteractionCallback() {
                    @Override public void onShow() {}
                    @Override public void onSelected(int pos, String val, boolean enforce) {
                        try { if (container != null) container.removeAllViews(); } catch (Exception ignored) {}
                        if (callback != null) callback.onAdClosed();
                    }
                    @Override public void onCancel() {}
                });

                new android.os.Handler(android.os.Looper.getMainLooper()).post(() -> {
                    TTNativeExpressAd ad = loadedAd;
                    if (ad == null) {
                        if (callback != null) callback.onAdFailed(-1004, "Express ad destroyed before render");
                        return;
                    }
                    try { ad.render(); }
                    catch (Throwable t) {
                        if (callback != null) callback.onAdFailed(-1005, t.getMessage() != null ? t.getMessage() : "render() failed");
                    }
                });
            }
        });
    }

    public void destroy() {
        if (loadedAd != null) { loadedAd.destroy(); loadedAd = null; }
    }
}
