#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

typedef void (^CsjSdkStartCompletion)(BOOL success, NSInteger code, NSString * _Nullable message);

/**
 * Thin wrapper around BUAdSDKManager for .NET binding.
 * Two-step initialization for privacy compliance.
 */
@interface CsjSdkWrapper : NSObject

/**
 * Step 1: Configure the SDK (safe before user consent).
 */
+ (void)configureWithAppId:(NSString *)appId
                   appName:(NSString *)appName
                     debug:(BOOL)debug
       allowPersonalizedAd:(BOOL)allowPersonalizedAd
             allowLocation:(BOOL)allowLocation;

/**
 * Step 2: Start the SDK (call after user consent).
 */
+ (void)startWithCompletion:(CsjSdkStartCompletion)completion;

/**
 * Check if the SDK is initialized.
 */
+ (BOOL)isInitialized;

@end

NS_ASSUME_NONNULL_END
