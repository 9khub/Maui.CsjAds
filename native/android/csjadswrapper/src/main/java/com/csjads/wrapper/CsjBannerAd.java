package com.csjads.wrapper;

import android.content.Context;
import android.view.View;
import android.view.ViewGroup;

import com.bytedance.sdk.openadsdk.AdSlot;
import com.bytedance.sdk.openadsdk.TTAdNative;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.TTNativeExpressAd;

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
        this.width = width > 0 ? width : 320;
        this.height = height > 0 ? height : 50;
    }

    public void load(Context context, final ViewGroup container, final CsjAdCallback callback) {
        this.callback = callback;

        TTAdNative adNative = TTAdSdk.getAdManager().createAdNative(context);

        AdSlot adSlot = new AdSlot.Builder()
                .setCodeId(slotId)
                .setExpressViewAcceptedSize(width, height)
                .setAdCount(1)
                .build();

        adNative.loadBannerExpressAd(adSlot, new TTAdNative.NativeExpressAdListener() {
            @Override
            public void onError(int code, String message) {
                if (callback != null) {
                    callback.onAdFailed(code, message);
                }
            }

            @Override
            public void onNativeExpressAdLoad(List<TTNativeExpressAd> ads) {
                if (ads == null || ads.isEmpty()) {
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
                        if (callback != null) callback.onAdFailed(code, msg);
                    }

                    @Override
                    public void onRenderSuccess(View view, float w, float h) {
                        // Add the rendered ad view to the container
                        container.removeAllViews();
                        container.addView(view);

                        if (callback != null) callback.onAdLoaded();
                    }
                });

                loadedAd.setDislikeCallback(null, new TTNativeExpressAd.DislikeInteractionCallback() {
                    @Override
                    public void onShow() {
                    }

                    @Override
                    public void onSelected(int position, String value, boolean enforce) {
                        container.removeAllViews();
                        if (callback != null) callback.onAdClosed();
                    }

                    @Override
                    public void onCancel() {
                    }
                });

                loadedAd.render();
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
