#import "CsjSplashAd.h"
#import <BUAdSDK/BUAdSDK.h>

@interface CsjSplashAd () <BUSplashAdDelegate>
@property (nonatomic, strong) NSString *slotId;
@property (nonatomic, assign) NSInteger timeoutMs;
@property (nonatomic, strong) BUSplashAd *splashAd;
@property (nonatomic, weak) id<CsjAdCallback> callback;
@end

@implementation CsjSplashAd

- (instancetype)initWithSlotId:(NSString *)slotId timeout:(NSInteger)timeoutMs {
    self = [super init];
    if (self) {
        _slotId = slotId;
        _timeoutMs = timeoutMs > 0 ? timeoutMs : 3000;
    }
    return self;
}

- (void)loadWithCallback:(id<CsjAdCallback>)callback {
    self.callback = callback;

    self.splashAd = [[BUSplashAd alloc] initWithSlotID:self.slotId adSize:CGSizeMake(0, 0)];
    self.splashAd.delegate = self;
    self.splashAd.tolerateTimeout = self.timeoutMs / 1000.0;
    [self.splashAd loadAdData];
}

- (void)showInWindow:(UIWindow *)window {
    if (self.splashAd) {
        [self.splashAd showSplashViewInRootViewController:window.rootViewController];
    }
}

#pragma mark - BUSplashAdDelegate

- (void)splashAdLoadSuccess:(BUSplashAd *)splashAd {
    if ([self.callback respondsToSelector:@selector(adDidLoad)]) {
        [self.callback adDidLoad];
    }
}

- (void)splashAdLoadFail:(BUSplashAd *)splashAd error:(BUAdError *_Nullable)error {
    if ([self.callback respondsToSelector:@selector(adDidFailWithCode:message:)]) {
        NSInteger code = error ? error.code : -1;
        NSString *msg = error ? error.localizedDescription : @"Load failed";
        [self.callback adDidFailWithCode:code message:msg];
    }
}

- (void)splashAdWillShow:(BUSplashAd *)splashAd {
    if ([self.callback respondsToSelector:@selector(adDidShow)]) {
        [self.callback adDidShow];
    }
}

- (void)splashAdDidClick:(BUSplashAd *)splashAd {
    if ([self.callback respondsToSelector:@selector(adDidClick)]) {
        [self.callback adDidClick];
    }
}

- (void)splashAdDidClose:(BUSplashAd *)splashAd closeType:(BUSplashAdCloseType)closeType {
    if ([self.callback respondsToSelector:@selector(adDidClose)]) {
        [self.callback adDidClose];
    }
}

@end
