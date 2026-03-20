#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "CsjAdCallback.h"

NS_ASSUME_NONNULL_BEGIN

@interface CsjRewardedVideoAd : NSObject

- (instancetype)initWithSlotId:(NSString *)slotId;
- (void)loadWithCallback:(id<CsjAdCallback>)callback;
- (void)showFromViewController:(UIViewController *)viewController;

@end

NS_ASSUME_NONNULL_END
