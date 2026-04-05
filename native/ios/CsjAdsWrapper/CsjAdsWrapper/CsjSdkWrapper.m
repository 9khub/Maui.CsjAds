#import "CsjSdkWrapper.h"
#import <BUAdSDK/BUAdSDK.h>

#pragma mark - Privacy Provider

@interface CsjPrivacyProvider : NSObject <BUAdSDKPrivacyProvider>
@property (nonatomic, assign) BOOL locationEnabled;
@end

@implementation CsjPrivacyProvider
- (BOOL)canUseLocation {
    return self.locationEnabled;
}
@end

#pragma mark - CsjSdkWrapper

static BOOL _sdkStarted = NO;

@implementation CsjSdkWrapper

+ (void)configureWithAppId:(NSString *)appId
                   appName:(NSString *)appName
                     debug:(BOOL)debug
       allowPersonalizedAd:(BOOL)allowPersonalizedAd
             allowLocation:(BOOL)allowLocation {

    // Step 1: Set up BUAdSDKConfiguration only — do NOT call start.
    // Use the shared configuration instance so the SDK reads it during start.
    BUAdSDKConfiguration *config = [BUAdSDKConfiguration configuration];
    config.appID = appId;
    config.debugLog = debug ? @(1) : @(0);
    config.SDKDEBUG = debug;
    config.unionTTSDK = YES;

    // Privacy settings via provider (v6.9+ API)
    CsjPrivacyProvider *privacy = [[CsjPrivacyProvider alloc] init];
    privacy.locationEnabled = allowLocation;
    config.privacyProvider = privacy;

    [BUAdSDKManager setUserExtData:@""];

    NSLog(@"[CsjSdkWrapper] configureWithAppId: %@ (configure only, SDK not started yet)", appId);
}

+ (void)startWithCompletion:(CsjSdkStartCompletion)completion {
    if (_sdkStarted) {
        NSLog(@"[CsjSdkWrapper] SDK already started, skipping duplicate start");
        if (completion) {
            completion(YES, 0, nil);
        }
        return;
    }

    NSLog(@"[CsjSdkWrapper] startWithCompletion: starting SDK (appID=%@)...",
          [BUAdSDKConfiguration configuration].appID);
    [BUAdSDKManager startWithAsyncCompletionHandler:^(BOOL success, NSError * _Nullable error) {
        if (success) {
            _sdkStarted = YES;
        }
        NSLog(@"[CsjSdkWrapper] SDK start result: success=%d, error=%@", success, error);
        if (completion) {
            NSInteger code = error ? error.code : 0;
            NSString *message = error ? error.localizedDescription : nil;
            completion(success, code, message);
        }
    }];
}

+ (BOOL)isInitialized {
    return _sdkStarted;
}

@end
