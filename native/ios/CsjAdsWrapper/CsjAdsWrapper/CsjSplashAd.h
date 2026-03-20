#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "CsjAdCallback.h"

NS_ASSUME_NONNULL_BEGIN

@interface CsjSplashAd : NSObject

- (instancetype)initWithSlotId:(NSString *)slotId timeout:(NSInteger)timeoutMs;
- (void)loadWithCallback:(id<CsjAdCallback>)callback;
- (void)showInWindow:(UIWindow *)window;

@end

NS_ASSUME_NONNULL_END
