#import "Bridge_Quago.h"
#import "Quago.h"

@implementation QuagoSettings_Bridge

- (id)init{
    self = [super init];
    return self;
}

#pragma mark - Publicly available C methods
#ifdef __cplusplus
    extern "C" {
#endif
        
#pragma mark -
#pragma mark Initialisation
                                   
        void initializeWithSettings(const char * appToken, int flavor, int logLevel, int maxSegments,
                                    int wrapper, const char * version,
                                    bool enableManualMotionDispatcher, bool enableManualKeysDispatcher,
                                    QuagoUnityOnJsonCallback unityOnJsonCallback,
                                    QuagoUnityOnLogMessage unityOnLogMessage){
            
            QuagoFlavor qFlavor;
            if(flavor == PRODUCTION) qFlavor = PRODUCTION;
            else if(flavor == DEVELOPMENT) qFlavor = DEVELOPMENT;
            else if(flavor == UNAUTHENTIC) qFlavor = UNAUTHENTIC;
            else if(flavor == AUTHENTIC) qFlavor = AUTHENTIC;
            else return;
            
            LogLevel qLogLevel;
            if(logLevel == LOG_DISABLED) qLogLevel = LOG_DISABLED;
            else if(logLevel == LOG_VERBOSE) qLogLevel = LOG_VERBOSE;
            else if(logLevel == LOG_DEBUG) qLogLevel = LOG_DEBUG;
            else if(logLevel == LOG_INFO) qLogLevel = LOG_INFO;
            else if(logLevel == LOG_WARNING) qLogLevel = LOG_WARNING;
            else if(logLevel == LOG_ERROR) qLogLevel = LOG_ERROR;
            else return;
            
            QuagoSettingsBuilder* settings = [QuagoSettingsBuilder create : [NSString stringWithUTF8String : appToken] : qFlavor];
            [settings setLogLevel : qLogLevel];
            [settings setMaxSegments : maxSegments];
            if(version != nil)[settings setWrapperInfo : wrapper : [NSString stringWithUTF8String : version]];
            if(enableManualMotionDispatcher)[settings enableManualTouchDispatcher];
            if(enableManualKeysDispatcher)[settings enableManualKeysDispatcher];
            
            /* Callbacks */
            if(unityOnJsonCallback != nil)
                [settings setJsonCallback:^(NSString * _Nonnull headers, NSString * _Nonnull payload) {
                    unityOnJsonCallback([headers UTF8String],[payload UTF8String]);
                }];
            
            if(unityOnLogMessage != nil)
                [settings overrideLogger:^(LogLevel logLevel, NSString * _Nonnull msg, NSException * _Nonnull throwable) {
                    if(throwable)
                        unityOnLogMessage((int)logLevel, [msg UTF8String], [[throwable description] UTF8String]);
                    else
                        unityOnLogMessage((int)logLevel, [msg UTF8String], nil);
                }];
            
            [Quago initialize : settings];
        }
        
#pragma mark -
#pragma mark begin/end Segment Methods
        
        void beginSegment(const char * name){
            [Quago beginSegment : [NSString stringWithUTF8String: name]];
        }

        void endSegment(){
            [Quago endSegment];
        }
        
#pragma mark -
#pragma mark Set user identifier
        
        void setUserId(const char * userId){
            [Quago setUserId : [NSString stringWithUTF8String : userId]];
        }

        void setAdditionalId(const char * additionalId){
            [Quago setAdditionalId : [NSString stringWithUTF8String : additionalId]];
        }
        
#pragma mark -
#pragma mark Send KeyValues and others

        /* Set custom metadata key,value for running context */
        void setKeyValues(const char * value, const char* key){
            [Quago setKeyValues : [NSString stringWithUTF8String : value]
                         forKey : [NSString stringWithUTF8String : key]];
        }

#pragma mark -
#pragma mark Getters
        
        const char * getSessionId(){
            return [[Quago getSessionId] UTF8String];
        }
        
#ifdef __cplusplus
    }
#endif
@end
