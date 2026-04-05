package com.csjads.wrapper;

import android.app.Activity;
import android.content.Context;

import com.bytedance.sdk.openadsdk.AdSlot;
import com.bytedance.sdk.openadsdk.TTAdNative;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.TTRewardVideoAd;

/**
 * Thin wrapper for CSJ Rewarded Video Ad.
 * Exposes only the load/show pattern with a unified callback.
 */
public class CsjRewardedVideoAd {

    private final String slotId;
    private TTRewardVideoAd loadedAd;
    private CsjAdCallback callback;

    public CsjRewardedVideoAd(String slotId) {
        this.slotId = slotId;
    }

    public void load(Context context, final CsjAdCallback callback) {
        this.callback = callback;

        TTAdNative adNative = TTAdSdk.getAdManager().createAdNative(context);

        AdSlot adSlot = new AdSlot.Builder()
                .setCodeId(slotId)
                .build();

        adNative.loadRewardVideoAd(adSlot, new TTAdNative.RewardVideoAdListener() {
            @Override
            public void onError(int code, String message) {
                if (callback != null) {
                    callback.onAdFailed(code, message);
                }
            }

            @Override
            public void onRewardVideoAdLoad(TTRewardVideoAd ad) {
                if (ad == null) {
                    android.util.Log.e("CsjAdsWrapper", "Reward onRewardVideoAdLoad: ad is null, slotId=" + slotId);
                    if (callback != null) {
                        callback.onAdFailed(-1, "Reward video ad instance is null");
                    }
                    return;
                }
                loadedAd = ad;
                if (callback != null) {
                    callback.onAdLoaded();
                }
            }

            @Override
            public void onRewardVideoCached() {
                // Video cached, ready for smooth playback
            }

            @Override
            public void onRewardVideoCached(TTRewardVideoAd ad) {
                // Updated cached callback with ad reference
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

        loadedAd.setRewardAdInteractionListener(new TTRewardVideoAd.RewardAdInteractionListener() {
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
                // Video playback completed
            }

            @Override
            public void onVideoError() {
                if (callback != null) callback.onAdFailed(-2, "Video playback error");
            }

            @Override
            public void onRewardVerify(boolean rewardVerify, int rewardAmount,
                                       String rewardName, int errorCode, String errorMsg) {
                if (callback != null) {
                    callback.onRewardVerified(rewardName, rewardAmount, rewardVerify);
                }
            }

            @Override
            public void onRewardArrived(boolean isRewardValid, int rewardType, android.os.Bundle extraInfo) {
                // Reward arrived — onRewardVerify is the primary callback
            }

            @Override
            public void onSkippedVideo() {
                // User skipped the video
            }
        });

        try {
            loadedAd.showRewardVideoAd(activity);
        } catch (Throwable t) {
            android.util.Log.e("CsjAdsWrapper", "Reward show failed, slotId=" + slotId, t);
            if (callback != null) {
                String m = t.getMessage();
                callback.onAdFailed(-3, m != null ? m : "showRewardVideoAd failed");
            }
            loadedAd = null;
        }
    }
}
