#import "CsjInterstitialAd.h"
#import <BUAdSDK/BUAdSDK.h>

@interface CsjInterstitialAd () <BUNativeExpressFullscreenVideoAdDelegate>
@property (nonatomic, strong) NSString *slotId;
@property (nonatomic, strong) BUNativeExpressFullscreenVideoAd *fullscreenAd;
@property (nonatomic, weak) id<CsjAdCallback> callback;
@end

@implementation CsjInterstitialAd

- (instancetype)initWithSlotId:(NSString *)slotId {
    self = [super init];
    if (self) {
        _slotId = slotId;
    }
    return self;
}

- (void)loadWithCallback:(id<CsjAdCallback>)callback {
    self.callback = callback;

    self.fullscreenAd = [[BUNativeExpressFullscreenVideoAd alloc] initWithSlotID:self.slotId];
    self.fullscreenAd.delegate = self;
    [self.fullscreenAd loadAdData];
}

- (void)showFromViewController:(UIViewController *)viewController {
    if (self.fullscreenAd) {
        [self.fullscreenAd showAdFromRootViewController:viewController];
    }
}

#pragma mark - BUNativeExpressFullscreenVideoAdDelegate

- (void)nativeExpressFullscreenVideoAdDidLoad:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
    if ([self.callback respondsToSelector:@selector(adDidLoad)]) {
        [self.callback adDidLoad];
    }
}

- (void)nativeExpressFullscreenVideoAd:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd didFailWithError:(NSError *_Nullable)error {
    if ([self.callback respondsToSelector:@selector(adDidFailWithCode:message:)]) {
        [self.callback adDidFailWithCode:error.code message:error.localizedDescription ?: @"Unknown error"];
    }
}

- (void)nativeExpressFullscreenVideoAdDidVisible:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
    if ([self.callback respondsToSelector:@selector(adDidShow)]) {
        [self.callback adDidShow];
    }
}

- (void)nativeExpressFullscreenVideoAdDidClick:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
    if ([self.callback respondsToSelector:@selector(adDidClick)]) {
        [self.callback adDidClick];
    }
}

- (void)nativeExpressFullscreenVideoAdDidClose:(BUNativeExpressFullscreenVideoAd *)fullscreenVideoAd {
    if ([self.callback respondsToSelector:@selector(adDidClose)]) {
        [self.callback adDidClose];
    }
}

@end
