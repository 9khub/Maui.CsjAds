#import "CsjBannerAd.h"
#import <BUAdSDK/BUAdSDK.h>

@interface CsjBannerAd () <BUNativeExpressBannerViewDelegate>
@property (nonatomic, strong) NSString *slotId;
@property (nonatomic, assign) NSInteger adWidth;
@property (nonatomic, assign) NSInteger adHeight;
@property (nonatomic, strong) BUNativeExpressBannerView *bannerView;
@property (nonatomic, weak) id<CsjAdCallback> callback;
@property (nonatomic, weak) UIView *container;
@end

@implementation CsjBannerAd

- (instancetype)initWithSlotId:(NSString *)slotId width:(NSInteger)width height:(NSInteger)height {
    self = [super init];
    if (self) {
        _slotId = slotId;
        _adWidth = width > 0 ? width : 320;
        _adHeight = height > 0 ? height : 50;
    }
    return self;
}

- (void)loadInView:(UIView *)container
    viewController:(UIViewController *)viewController
          callback:(id<CsjAdCallback>)callback {
    self.callback = callback;
    self.container = container;

    CGSize adSize = CGSizeMake(self.adWidth, self.adHeight);

    self.bannerView = [[BUNativeExpressBannerView alloc] initWithSlotID:self.slotId
                                                    rootViewController:viewController
                                                                adSize:adSize];
    self.bannerView.delegate = self;
    [self.bannerView loadAdData];
}

- (void)destroy {
    if (self.bannerView) {
        [self.bannerView removeFromSuperview];
        self.bannerView.delegate = nil;
        self.bannerView = nil;
    }
}

#pragma mark - BUNativeExpressBannerViewDelegate

- (void)nativeExpressBannerAdViewDidLoad:(BUNativeExpressBannerView *)bannerAdView {
    // Ad loaded, render it
    [bannerAdView render];
}

- (void)nativeExpressBannerAdViewRenderSuccess:(BUNativeExpressBannerView *)bannerAdView {
    // Add to container
    if (self.container) {
        for (UIView *subview in self.container.subviews) {
            [subview removeFromSuperview];
        }
        bannerAdView.frame = self.container.bounds;
        bannerAdView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
        [self.container addSubview:bannerAdView];
    }

    if ([self.callback respondsToSelector:@selector(adDidLoad)]) {
        [self.callback adDidLoad];
    }
}

- (void)nativeExpressBannerAdViewRenderFail:(BUNativeExpressBannerView *)bannerAdView error:(NSError *_Nullable)error {
    if ([self.callback respondsToSelector:@selector(adDidFailWithCode:message:)]) {
        [self.callback adDidFailWithCode:error.code message:error.localizedDescription ?: @"Render failed"];
    }
}

- (void)nativeExpressBannerAdView:(BUNativeExpressBannerView *)bannerAdView didLoadFailWithError:(NSError *_Nullable)error {
    if ([self.callback respondsToSelector:@selector(adDidFailWithCode:message:)]) {
        [self.callback adDidFailWithCode:error.code message:error.localizedDescription ?: @"Load failed"];
    }
}

- (void)nativeExpressBannerAdViewDidClick:(BUNativeExpressBannerView *)bannerAdView {
    if ([self.callback respondsToSelector:@selector(adDidClick)]) {
        [self.callback adDidClick];
    }
}

- (void)nativeExpressBannerAdViewDislike:(BUNativeExpressBannerView *)bannerAdView {
    if ([self.callback respondsToSelector:@selector(adDidClose)]) {
        [self.callback adDidClose];
    }
}

@end
