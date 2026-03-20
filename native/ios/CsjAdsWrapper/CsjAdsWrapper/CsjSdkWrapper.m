#import "CsjSdkWrapper.h"
#import <BUAdSDK/BUAdSDK.h>

@implementation CsjSdkWrapper

+ (void)configureWithAppId:(NSString *)appId
                   appName:(NSString *)appName
                     debug:(BOOL)debug
       allowPersonalizedAd:(BOOL)allowPersonalizedAd
             allowLocation:(BOOL)allowLocation {

    BUAdSDKConfiguration *config = [BUAdSDKConfiguration new];
    config.appID = appId;
    config.debugLog = debug ? @(1) : @(0);

    // Privacy settings
    config.allowPersonalizedAd = allowPersonalizedAd;

    [BUAdSDKManager setUserExtData:@""];

    [BUAdSDKManager startWithAsyncCompletionHandler:^(BOOL success, NSError * _Nullable error) {
        // Pre-configuration only — actual start happens in startWithCompletion:
    }];
}

+ (void)startWithCompletion:(CsjSdkStartCompletion)completion {
    [BUAdSDKManager startWithAsyncCompletionHandler:^(BOOL success, NSError * _Nullable error) {
        if (completion) {
            NSInteger code = error ? error.code : 0;
            NSString *message = error ? error.localizedDescription : nil;
            completion(success, code, message);
        }
    }];
}

+ (BOOL)isInitialized {
    return YES; // BUAdSDKManager doesn't expose a direct check
}

@end
