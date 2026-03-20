package com.csjads.wrapper;

/**
 * Unified callback interface for all ad lifecycle events.
 * The .NET binding will generate an ICsjAdCallback interface from this.
 */
public interface CsjAdCallback {
    void onAdLoaded();
    void onAdFailed(int code, String message);
    void onAdShow();
    void onAdClicked();
    void onAdClosed();
    void onRewardVerified(String rewardName, int rewardAmount, boolean verified);
}
