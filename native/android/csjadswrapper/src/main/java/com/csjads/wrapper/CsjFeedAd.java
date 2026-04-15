package com.csjads.wrapper;

import android.content.Context;
import android.view.View;
import android.view.ViewGroup;

import com.bytedance.sdk.openadsdk.AdSlot;
import com.bytedance.sdk.openadsdk.TTAdNative;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.TTNativeExpressAd;
import com.bytedance.sdk.openadsdk.mediation.ad.MediationAdSlot;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Feed (信息流) 广告。
 * 策略：load 阶段就完成 render，只保留渲染成功的 View。
 * C# 层通过 getRenderedCount/getRenderedView 知道有多少广告可插入。
 * 渲染失败的广告不会被插入到 feed，避免显示空占位。
 */
public class CsjFeedAd {

    private final String slotId;
    private final int width;
    private final int height;
    private final int adCount;

    private List<TTNativeExpressAd> loadedAds;
    private final List<View> renderedViews = new ArrayList<>();
    private CsjAdCallback callback;
    private boolean loadCompleted = false;

    public CsjFeedAd(String slotId, int adCount, int width, int height) {
        this.slotId = slotId;
        this.adCount = adCount > 0 ? adCount : 1;
        this.width = width;
        this.height = height;
    }

    /** 已成功渲染的广告数量 */
    public int getRenderedCount() {
        return renderedViews.size();
    }

    /** 获取已渲染的 View，供 C# Handler 添加到容器 */
    public View getRenderedView(int index) {
        if (index < 0 || index >= renderedViews.size()) return null;
        return renderedViews.get(index);
    }

    public boolean isLoadCompleted() { return loadCompleted; }

    public void load(Context context, final CsjAdCallback callback) {
        this.callback = callback;

        android.util.DisplayMetrics dm = context.getResources().getDisplayMetrics();
        int finalWidth = width > 0 ? width : (int) (dm.widthPixels / dm.density);
        int finalHeight = height > 0 ? height : 0;
        int imageWidthPx = finalWidth > 0 ? (int) (finalWidth * dm.density) : dm.widthPixels;
        int imageHeightPx = finalHeight > 0 ? (int) (finalHeight * dm.density) : 0;

        android.util.Log.d("CsjAdsWrapper",
                "Feed: load+render flow starts, slotId=" + slotId + ", count=" + adCount);

        AdSlot.Builder slotBuilder = new AdSlot.Builder()
                .setCodeId(slotId)
                .setImageAcceptedSize(imageWidthPx, imageHeightPx)
                .setExpressViewAcceptedSize(finalWidth, finalHeight)
                .setAdCount(adCount);
        if (CsjSdkWrapper.isUseMediation()) {
            slotBuilder.setMediationAdSlot(new MediationAdSlot.Builder()
                    .setExtraObject("show_adn_load_error_detail", Boolean.TRUE)
                    .build());
        }

        TTAdNative adNative = TTAdSdk.getAdManager().createAdNative(context);
        adNative.loadNativeExpressAd(slotBuilder.build(), new TTAdNative.NativeExpressAdListener() {
            @Override
            public void onError(int code, String message) {
                android.util.Log.e("CsjAdsWrapper",
                        "Feed load failed: code=" + code + ", msg=" + message);
                loadCompleted = true;
                if (callback != null) callback.onAdFailed(code, message);
            }

            @Override
            public void onNativeExpressAdLoad(List<TTNativeExpressAd> ads) {
                if (ads == null || ads.isEmpty()) {
                    loadCompleted = true;
                    if (callback != null) callback.onAdFailed(-1, "No feed ads returned");
                    return;
                }
                loadedAds = ads;
                final int total = ads.size();
                final AtomicInteger pendingRenders = new AtomicInteger(total);

                android.util.Log.d("CsjAdsWrapper",
                        "Feed: loaded " + total + " ads, pre-rendering all");

                for (int i = 0; i < total; i++) {
                    final int adIndex = i;
                    final TTNativeExpressAd ad = ads.get(i);
                    ad.setExpressInteractionListener(new TTNativeExpressAd.ExpressAdInteractionListener() {
                        @Override public void onAdClicked(View v, int t) { if (callback != null) callback.onAdClicked(); }
                        @Override public void onAdShow(View v, int t) {}
                        @Override public void onRenderFail(View v, String msg, int code) {
                            android.util.Log.e("CsjAdsWrapper",
                                    "Feed render fail idx=" + adIndex + " code=" + code + " msg=" + msg);
                            if (pendingRenders.decrementAndGet() == 0) finalizeLoad();
                        }
                        @Override public void onRenderSuccess(View view, float w, float h) {
                            if (view == null) {
                                try { view = ad.getExpressAdView(); } catch (Throwable ignored) {}
                            }
                            if (view != null) {
                                synchronized (renderedViews) { renderedViews.add(view); }
                                android.util.Log.d("CsjAdsWrapper",
                                        "Feed render success idx=" + adIndex + ", size=" + w + "x" + h
                                                + ", total=" + renderedViews.size());
                            } else {
                                android.util.Log.e("CsjAdsWrapper",
                                        "Feed render idx=" + adIndex + ": null view after fallback");
                            }
                            if (pendingRenders.decrementAndGet() == 0) finalizeLoad();
                        }
                    });

                    // 错峰渲染：index × 300ms 延迟，避免多个 WebView 同时实例化导致 native 崩溃
                    long delayMs = adIndex * 300L;
                    new android.os.Handler(android.os.Looper.getMainLooper()).postDelayed(() -> {
                        try { ad.render(); }
                        catch (Throwable t) {
                            android.util.Log.e("CsjAdsWrapper", "Feed render threw idx=" + adIndex, t);
                            if (pendingRenders.decrementAndGet() == 0) finalizeLoad();
                        }
                    }, delayMs);
                }
            }
        });
    }

    private void finalizeLoad() {
        loadCompleted = true;
        int count = renderedViews.size();
        android.util.Log.d("CsjAdsWrapper", "Feed all renders done: " + count + " succeeded");
        if (callback != null) {
            if (count > 0) callback.onAdLoaded();
            else callback.onAdFailed(-1001, "All feed ads failed to render");
        }
    }

    /** 不再使用 — 保留兼容性。渲染已在 load 阶段完成。 */
    public void renderIntoContainer(int index, ViewGroup container, CsjAdCallback renderCallback) {
        if (index < 0 || index >= renderedViews.size()) {
            if (renderCallback != null) renderCallback.onAdFailed(-1, "Invalid ad index");
            return;
        }
        View view = renderedViews.get(index);
        if (view == null) {
            if (renderCallback != null) renderCallback.onAdFailed(-1, "No rendered view");
            return;
        }
        try {
            if (view.getParent() instanceof ViewGroup) {
                ((ViewGroup) view.getParent()).removeView(view);
            }
            container.removeAllViews();
            android.widget.FrameLayout.LayoutParams lp =
                    new android.widget.FrameLayout.LayoutParams(
                            android.widget.FrameLayout.LayoutParams.MATCH_PARENT,
                            android.widget.FrameLayout.LayoutParams.MATCH_PARENT);
            container.addView(view, lp);
            if (renderCallback != null) renderCallback.onAdLoaded();
        } catch (Exception e) {
            if (renderCallback != null) renderCallback.onAdFailed(-1003, e.getMessage());
        }
    }

    public boolean isSelfRenderMode() { return false; }
    public String getAdTitle(int index) { return null; }
    public String getAdImageUrl(int index) { return null; }
    public String getAdSource(int index) { return null; }

    public void destroy() {
        if (loadedAds != null) {
            for (TTNativeExpressAd ad : loadedAds) {
                try { ad.destroy(); } catch (Throwable ignored) {}
            }
            loadedAds = null;
        }
        renderedViews.clear();
    }
}
