#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "CsjAdCallback.h"

NS_ASSUME_NONNULL_BEGIN

@interface CsjBannerAd : NSObject

- (instancetype)initWithSlotId:(NSString *)slotId width:(NSInteger)width height:(NSInteger)height;
- (void)loadInView:(UIView *)container
    viewController:(nullable UIViewController *)viewController
          callback:(id<CsjAdCallback>)callback;
- (void)destroy;

@end

NS_ASSUME_NONNULL_END
