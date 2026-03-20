#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

/**
 * Protocol for ad lifecycle callbacks.
 * The .NET iOS binding will generate a C# delegate/interface from this.
 */
@protocol CsjAdCallback <NSObject>

- (void)adDidLoad;
- (void)adDidFailWithCode:(NSInteger)code message:(NSString *)message;
- (void)adDidShow;
- (void)adDidClick;
- (void)adDidClose;

@optional
- (void)rewardDidVerifyWithName:(NSString *)rewardName
                         amount:(NSInteger)rewardAmount
                       verified:(BOOL)verified;

@end

NS_ASSUME_NONNULL_END
