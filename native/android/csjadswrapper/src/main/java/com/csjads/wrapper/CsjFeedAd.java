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

/**
 * Feed (信息流) 广告。使用 Express 模板渲染。
 * 广告数据先加载并缓存，render 延迟到容器 attach 到 window 后执行。
 * （与 Banner 的 ViewAttachedToWindow 修复策略一致）
 */
public class CsjFeedAd {

    private final String slotId;
    private final int width;
    private final int height;
    private final int adCount;

    /** 已加载但尚未 render 的 Express 广告对象 */
    private List<TTNativeExpressAd> loadedAds;
    private CsjAdCallback callback;

    public CsjFeedAd(String slotId, int adCount, int width, int height) {
        this.slotId = slotId;
        this.adCount = adCount > 0 ? adCount : 3;
        this.width = width;
        this.height = height;
    }

    /** 已加载的广告数量（尚未 render） */
    public int getRenderedCount() {
        return loadedAds != null ? loadedAds.size() : 0;
    }

    /**
     * 将第 index 条广告渲染到指定容器中。
     * 必须在主线程、容器已 attach 到 window 后调用。
     */
    public void renderIntoContainer(int index, ViewGroup container, CsjAdCallback renderCallback) {
        if (loadedAds == null || index < 0 || index >= loadedAds.size()) {
            if (renderCallback != null) renderCallback.onAdFailed(-1, "Invalid ad index");
            return;
        }

        TTNativeExpressAd ad = loadedAds.get(index);
        ad.setExpressInteractionListener(new TTNativeExpressAd.ExpressAdInteractionListener() {
            @Override public void onAdClicked(View v, int t) { if (callback != null) callback.onAdClicked(); }
            @Override public void onAdShow(View v, int t) {}
            @Override public void onRenderFail(View v, String msg, int code) {
                android.util.Log.e("CsjAdsWrapper", "Feed render fail[" + index + "]: code=" + code + ", msg=" + msg);
                if (renderCallback != null) renderCallback.onAdFailed(code, msg);
            }
            @Override
            public void onRenderSuccess(View view, float w, float h) {
                if (view == null) {
                    try { view = ad.getExpressAdView(); } catch (Throwable ignored) {}
                }
                if (view == null) {
                    android.util.Log.e("CsjAdsWrapper", "Feed render[" + index + "]: null view");
                    if (renderCallback != null) renderCallback.onAdFailed(-1001, "Express render null view");
                    return;
                }
                try {
                    if (view.getParent() instanceof ViewGroup) {
                        ((ViewGroup) view.getParent()).removeView(view);
                    }
                    container.removeAllViews();

                    // 强制用 MATCH_PARENT × MATCH_PARENT — 确保广告 View 占满整个容器
                    android.widget.FrameLayout.LayoutParams lp =
                            new android.widget.FrameLayout.LayoutParams(
                                    android.widget.FrameLayout.LayoutParams.MATCH_PARENT,
                                    android.widget.FrameLayout.LayoutParams.MATCH_PARENT);
                    container.addView(view, lp);

                    // 立即记录广告 View 的实际状态（同步，不等 post）
                    android.util.Log.d("CsjAdsWrapper",
                            "Feed render[" + index + "] addView done: sdk_size=" + w + "x" + h
                            + ", view.class=" + view.getClass().getSimpleName()
                            + ", view.visibility=" + view.getVisibility()
                            + ", view.alpha=" + view.getAlpha()
                            + ", container.childCount=" + container.getChildCount());

                    // 延迟 500ms 后再次记录（等 WebView 有机会渲染内容）
                    final View finalView = view;
                    container.postDelayed(() -> {
                        android.util.Log.d("CsjAdsWrapper",
                                "Feed render[" + index + "] +500ms: view.actual="
                                + finalView.getWidth() + "x" + finalView.getHeight()
                                + ", container.size=" + container.getWidth() + "x" + container.getHeight()
                                + ", view.isShown=" + finalView.isShown()
                                + (finalView instanceof ViewGroup ? ", subChildCount=" + ((ViewGroup)finalView).getChildCount() : ""));
                    }, 500);

                    if (renderCallback != null) renderCallback.onAdLoaded();
                } catch (Exception e) {
                    android.util.Log.e("CsjAdsWrapper", "Feed render[" + index + "] addView: " + e.getMessage());
                    if (renderCallback != null) renderCallback.onAdFailed(-1003, e.getMessage());
                }
            }
        });

        // 错峰 render：不同 index 延迟不同时间，避免多个 WebView 同时实例化
        // 导致内存峰值（穿山甲 Express 在部分设备上并发会 native 崩溃）
        long delayMs = index * 400L;
        new android.os.Handler(android.os.Looper.getMainLooper()).postDelayed(() -> {
            try {
                ad.render();
            } catch (Throwable t) {
                android.util.Log.e("CsjAdsWrapper", "Feed render[" + index + "] threw: " + t.getMessage());
                if (renderCallback != null) renderCallback.onAdFailed(-1005, t.getMessage());
            }
        }, delayMs);
    }

    /** 不再使用 — 保留接口兼容。渲染改为 renderIntoContainer。 */
    public View getRenderedView(int index) { return null; }
    public boolean isSelfRenderMode() { return false; }
    public String getAdTitle(int index) { return null; }
    public String getAdImageUrl(int index) { return null; }
    public String getAdSource(int index) { return null; }

    public void load(Context context, final CsjAdCallback callback) {
        this.callback = callback;

        android.util.DisplayMetrics dm = context.getResources().getDisplayMetrics();
        int finalWidth = width > 0 ? width : (int) (dm.widthPixels / dm.density);
        int finalHeight = height > 0 ? height : 0;
        int imageWidthPx = finalWidth > 0 ? (int) (finalWidth * dm.density) : dm.widthPixels;
        int imageHeightPx = finalHeight > 0 ? (int) (finalHeight * dm.density) : 0;

        android.util.Log.d("CsjAdsWrapper",
                "Feed: loading express, slotId=" + slotId + ", count=" + adCount);

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
                android.util.Log.e("CsjAdsWrapper", "Feed load failed: code=" + code + ", msg=" + message);
                if (callback != null) callback.onAdFailed(code, message);
            }

            @Override
            public void onNativeExpressAdLoad(List<TTNativeExpressAd> ads) {
                if (ads == null || ads.isEmpty()) {
                    if (callback != null) callback.onAdFailed(-1, "No feed ads returned");
                    return;
                }
                loadedAds = ads;
                android.util.Log.d("CsjAdsWrapper", "Feed: loaded " + ads.size() + " ads (render deferred)");
                if (callback != null) callback.onAdLoaded();
            }
        });
    }

    public void destroy() {
        if (loadedAds != null) {
            for (TTNativeExpressAd ad : loadedAds) {
                try { ad.destroy(); } catch (Throwable ignored) {}
            }
            loadedAds = null;
        }
    }
}
