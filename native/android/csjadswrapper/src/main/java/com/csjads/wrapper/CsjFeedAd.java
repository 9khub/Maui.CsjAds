package com.csjads.wrapper;

import android.content.Context;
import android.view.View;
import android.view.ViewGroup;

import com.bytedance.sdk.openadsdk.AdSlot;
import com.bytedance.sdk.openadsdk.TTAdNative;
import com.bytedance.sdk.openadsdk.TTAdSdk;
import com.bytedance.sdk.openadsdk.TTFeedAd;
import com.bytedance.sdk.openadsdk.TTImage;
import com.bytedance.sdk.openadsdk.TTNativeExpressAd;
import com.bytedance.sdk.openadsdk.mediation.ad.MediationAdSlot;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Thin wrapper for CSJ Feed Ad.
 * 优先非 Express（自渲染：提取标题/图片数据），回退到 Express。
 */
public class CsjFeedAd {

    private final String slotId;
    private final int width;
    private final int height;
    private final int adCount;

    private List<TTFeedAd> loadedFeedAds;
    private List<TTNativeExpressAd> loadedExpressAds;
    private final List<View> renderedViews = new ArrayList<>();

    /** 自渲染模式提取的广告数据 */
    private final List<String> adTitles = new ArrayList<>();
    private final List<String> adImageUrls = new ArrayList<>();
    private final List<String> adSources = new ArrayList<>();
    /** 是否为自渲染模式（true=有标题/图片数据，false=有原生 View） */
    private boolean isSelfRender = false;

    private CsjAdCallback callback;

    public CsjFeedAd(String slotId, int adCount, int width, int height) {
        this.slotId = slotId;
        this.adCount = adCount > 0 ? adCount : 3;
        this.width = width;
        this.height = height;
    }

    public int getRenderedCount() {
        return isSelfRender ? adTitles.size() : renderedViews.size();
    }

    public View getRenderedView(int index) {
        if (index < 0 || index >= renderedViews.size()) return null;
        return renderedViews.get(index);
    }

    public boolean isSelfRenderMode() { return isSelfRender; }

    public String getAdTitle(int index) {
        if (index < 0 || index >= adTitles.size()) return null;
        return adTitles.get(index);
    }

    public String getAdImageUrl(int index) {
        if (index < 0 || index >= adImageUrls.size()) return null;
        return adImageUrls.get(index);
    }

    public String getAdSource(int index) {
        if (index < 0 || index >= adSources.size()) return null;
        return adSources.get(index);
    }

    public void load(Context context, final CsjAdCallback callback) {
        this.callback = callback;

        android.util.DisplayMetrics dm = context.getResources().getDisplayMetrics();
        int finalWidth = width > 0 ? width : (int) (dm.widthPixels / dm.density);
        int finalHeight = height > 0 ? height : 0;
        int imageWidthPx = finalWidth > 0 ? (int) (finalWidth * dm.density) : dm.widthPixels;
        int imageHeightPx = finalHeight > 0 ? (int) (finalHeight * dm.density) : 0;

        android.util.Log.d("CsjAdsWrapper",
                "Feed: requesting, slotId=" + slotId + ", count=" + adCount);

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
        AdSlot adSlot = slotBuilder.build();
        TTAdNative adNative = TTAdSdk.getAdManager().createAdNative(context);

        // 优先非 Express 自渲染
        try {
            android.util.Log.d("CsjAdsWrapper", "Feed: trying loadFeedAd (self-render)");
            adNative.loadFeedAd(adSlot, new TTAdNative.FeedAdListener() {
                @Override
                public void onFeedAdLoad(List<TTFeedAd> ads) {
                    if (ads == null || ads.isEmpty()) {
                        android.util.Log.w("CsjAdsWrapper", "Feed: loadFeedAd empty, fallback to Express");
                        loadExpressFeed(adNative, adSlot, context);
                        return;
                    }
                    loadedFeedAds = ads;
                    isSelfRender = true;
                    adTitles.clear();
                    adImageUrls.clear();
                    adSources.clear();

                    for (int i = 0; i < ads.size(); i++) {
                        TTFeedAd ad = ads.get(i);
                        String title = ad.getTitle();
                        String desc = ad.getDescription();
                        String source = ad.getSource();
                        int imageMode = ad.getImageMode();

                        // 提取图片 URL
                        String imageUrl = "";
                        List<TTImage> images = ad.getImageList();
                        if (images != null && !images.isEmpty()) {
                            imageUrl = images.get(0).getImageUrl();
                        }
                        // 如果主图为空，尝试视频封面图
                        if ((imageUrl == null || imageUrl.isEmpty()) && ad.getVideoCoverImage() != null) {
                            imageUrl = ad.getVideoCoverImage().getImageUrl();
                        }
                        // 如果仍为空，尝试 icon
                        if ((imageUrl == null || imageUrl.isEmpty()) && ad.getIcon() != null) {
                            imageUrl = ad.getIcon().getImageUrl();
                        }

                        android.util.Log.d("CsjAdsWrapper",
                                "Feed[" + i + "] title=" + title
                                + ", desc=" + desc
                                + ", source=" + source
                                + ", imageMode=" + imageMode
                                + ", images=" + (images != null ? images.size() : 0)
                                + ", imageUrl=" + imageUrl);

                        // 有图片就用自渲染，没有图片也记录（用空URL占位）
                        adTitles.add(title != null ? title : (desc != null ? desc : ""));
                        adImageUrls.add(imageUrl != null ? imageUrl : "");
                        adSources.add(source != null ? source : "广告");
                    }

                    // 只要加载成功就通知，即使图片为空（C#层会根据数据决定显示方式）
                    android.util.Log.d("CsjAdsWrapper",
                            "Feed: self-render extracted " + adTitles.size() + " ads");
                    if (!adTitles.isEmpty()) {
                        if (callback != null) callback.onAdLoaded();
                    } else {
                        android.util.Log.w("CsjAdsWrapper", "Feed: no ads extracted, fallback to Express");
                        isSelfRender = false;
                        loadExpressFeed(adNative, adSlot, context);
                    }
                }

                @Override
                public void onError(int code, String message) {
                    android.util.Log.w("CsjAdsWrapper",
                            "Feed: loadFeedAd failed (code=" + code + "), fallback to Express");
                    loadExpressFeed(adNative, adSlot, context);
                }
            });
        } catch (Throwable t) {
            android.util.Log.w("CsjAdsWrapper", "Feed: loadFeedAd threw: " + t.getMessage());
            loadExpressFeed(adNative, adSlot, context);
        }
    }

    private void loadExpressFeed(TTAdNative adNative, AdSlot adSlot, Context context) {
        android.util.Log.d("CsjAdsWrapper", "Feed: trying loadNativeExpressAd (express)");
        adNative.loadNativeExpressAd(adSlot, new TTAdNative.NativeExpressAdListener() {
            @Override
            public void onError(int code, String message) {
                android.util.Log.e("CsjAdsWrapper", "Feed express failed: code=" + code);
                if (callback != null) callback.onAdFailed(code, message);
            }

            @Override
            public void onNativeExpressAdLoad(List<TTNativeExpressAd> ads) {
                if (ads == null || ads.isEmpty()) {
                    if (callback != null) callback.onAdFailed(-1, "No express feed ads");
                    return;
                }
                loadedExpressAds = ads;
                isSelfRender = false;
                final int total = ads.size();
                final AtomicInteger pending = new AtomicInteger(total);

                for (int i = 0; i < total; i++) {
                    final int idx = i;
                    final TTNativeExpressAd ad = ads.get(i);
                    ad.setExpressInteractionListener(new TTNativeExpressAd.ExpressAdInteractionListener() {
                        @Override public void onAdClicked(View v, int t) { if (callback != null) callback.onAdClicked(); }
                        @Override public void onAdShow(View v, int t) {}
                        @Override public void onRenderFail(View v, String m, int c) {
                            if (pending.decrementAndGet() == 0) notifyComplete();
                        }
                        @Override public void onRenderSuccess(View view, float w, float h) {
                            if (view == null) { try { view = ad.getExpressAdView(); } catch (Throwable ignored) {} }
                            if (view != null) { synchronized (renderedViews) { renderedViews.add(view); } }
                            if (pending.decrementAndGet() == 0) notifyComplete();
                        }
                    });
                    new android.os.Handler(android.os.Looper.getMainLooper()).post(() -> {
                        try { ad.render(); } catch (Throwable t) { if (pending.decrementAndGet() == 0) notifyComplete(); }
                    });
                }
            }
        });
    }

    private void notifyComplete() {
        int count = renderedViews.size();
        android.util.Log.d("CsjAdsWrapper", "Feed express: rendered=" + count);
        if (callback != null) {
            if (count > 0) callback.onAdLoaded();
            else callback.onAdFailed(-1001, "All feed express ads failed to render");
        }
    }

    public void destroy() {
        if (loadedExpressAds != null) {
            for (TTNativeExpressAd ad : loadedExpressAds) { try { ad.destroy(); } catch (Throwable ignored) {} }
            loadedExpressAds = null;
        }
        loadedFeedAds = null;
        renderedViews.clear();
        adTitles.clear();
        adImageUrls.clear();
        adSources.clear();
    }
}
