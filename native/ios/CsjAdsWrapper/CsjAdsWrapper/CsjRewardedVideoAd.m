#import "CsjRewardedVideoAd.h"
#import <BUAdSDK/BUAdSDK.h>

@interface CsjRewardedVideoAd () <BUNativeExpressRewardedVideoAdDelegate>
@property (nonatomic, strong) NSString *slotId;
@property (nonatomic, strong) BUNativeExpressRewardedVideoAd *rewardedAd;
@property (nonatomic, weak) id<CsjAdCallback> callback;
@end

@implementation CsjRewardedVideoAd

- (instancetype)initWithSlotId:(NSString *)slotId {
    self = [super init];
    if (self) {
        _slotId = slotId;
    }
    return self;
}

- (void)loadWithCallback:(id<CsjAdCallback>)callback {
    self.callback = callback;

    BURewardedVideoModel *model = [[BURewardedVideoModel alloc] init];

    self.rewardedAd = [[BUNativeExpressRewardedVideoAd alloc] initWithSlotID:self.slotId
                                                            rewardedVideoModel:model];
    self.rewardedAd.delegate = self;
    [self.rewardedAd loadAdData];
}

- (void)showFromViewController:(UIViewController *)viewController {
    if (self.rewardedAd) {
        [self.rewardedAd showAdFromRootViewController:viewController];
    }
}

#pragma mark - BUNativeExpressRewardedVideoAdDelegate

- (void)nativeExpressRewardedVideoAdDidLoad:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
    if ([self.callback respondsToSelector:@selector(adDidLoad)]) {
        [self.callback adDidLoad];
    }
}

- (void)nativeExpressRewardedVideoAd:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd didFailWithError:(NSError *_Nullable)error {
    if ([self.callback respondsToSelector:@selector(adDidFailWithCode:message:)]) {
        [self.callback adDidFailWithCode:error.code message:error.localizedDescription ?: @"Unknown error"];
    }
}

- (void)nativeExpressRewardedVideoAdDidVisible:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
    if ([self.callback respondsToSelector:@selector(adDidShow)]) {
        [self.callback adDidShow];
    }
}

- (void)nativeExpressRewardedVideoAdDidClick:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
    if ([self.callback respondsToSelector:@selector(adDidClick)]) {
        [self.callback adDidClick];
    }
}

- (void)nativeExpressRewardedVideoAdDidClose:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
    if ([self.callback respondsToSelector:@selector(adDidClose)]) {
        [self.callback adDidClose];
    }
}

- (void)nativeExpressRewardedVideoAdServerRewardDidSucceed:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd verify:(BOOL)verify {
    if ([self.callback respondsToSelector:@selector(rewardDidVerifyWithName:amount:verified:)]) {
        [self.callback rewardDidVerifyWithName:@"reward" amount:1 verified:verify];
    }
}

@end
