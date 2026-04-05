package com.csjads.wrapper;

import android.app.Activity;
import android.content.Context;

import com.bytedance.sdk.openadsdk.AdSlot;
import com.bytedance.sdk.openadsdk.TTAdNative;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.TTFullScreenVideoAd;

/**
 * Thin wrapper for CSJ Interstitial (Full-Screen Video) Ad.
 * Note: CSJ SDK 4.9+ replaced interstitial with full-screen video ads.
 */
public class CsjInterstitialAd {

    private final String slotId;
    private TTFullScreenVideoAd loadedAd;
    private CsjAdCallback callback;

    public CsjInterstitialAd(String slotId) {
        this.slotId = slotId;
    }

    public void load(Context context, final CsjAdCallback callback) {
        this.callback = callback;

        TTAdNative adNative = TTAdSdk.getAdManager().createAdNative(context);

        AdSlot adSlot = new AdSlot.Builder()
                .setCodeId(slotId)
                .build();

        adNative.loadFullScreenVideoAd(adSlot, new TTAdNative.FullScreenVideoAdListener() {
            @Override
            public void onError(int code, String message) {
                if (callback != null) {
                    callback.onAdFailed(code, message);
                }
            }

            @Override
            public void onFullScreenVideoAdLoad(TTFullScreenVideoAd ad) {
                if (ad == null) {
                    android.util.Log.e("CsjAdsWrapper", "Interstitial onFullScreenVideoAdLoad: ad is null, slotId=" + slotId);
                    if (callback != null) {
                        callback.onAdFailed(-1, "Full screen ad instance is null");
                    }
                    return;
                }
                loadedAd = ad;
                if (callback != null) {
                    callback.onAdLoaded();
                }
            }

            @Override
            public void onFullScreenVideoCached() {
                // Video cached
            }

            @Override
            public void onFullScreenVideoCached(TTFullScreenVideoAd ad) {
                loadedAd = ad;
            }
        });
    }

    public void show(Activity activity) {
        if (activity == null) {
            if (callback != null) {
                callback.onAdFailed(-1, "Activity is null");
            }
            return;
        }
        if (loadedAd == null) {
            if (callback != null) {
                callback.onAdFailed(-1, "Ad not loaded");
            }
            return;
        }

        loadedAd.setFullScreenVideoAdInteractionListener(
                new TTFullScreenVideoAd.FullScreenVideoAdInteractionListener() {
                    @Override
                    public void onAdShow() {
                        if (callback != null) callback.onAdShow();
                    }

                    @Override
                    public void onAdVideoBarClick() {
                        if (callback != null) callback.onAdClicked();
                    }

                    @Override
                    public void onAdClose() {
                        if (callback != null) callback.onAdClosed();
                        loadedAd = null;
                    }

                    @Override
                    public void onVideoComplete() {
                    }

                    @Override
                    public void onSkippedVideo() {
                    }
                });

        try {
            loadedAd.showFullScreenVideoAd(activity);
        } catch (Throwable t) {
            android.util.Log.e("CsjAdsWrapper", "Interstitial show failed, slotId=" + slotId, t);
            if (callback != null) {
                String m = t.getMessage();
                callback.onAdFailed(-3, m != null ? m : "showFullScreenVideoAd failed");
            }
            loadedAd = null;
        }
    }
}
