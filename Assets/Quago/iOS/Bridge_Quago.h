@interface QuagoSettings_Bridge : NSObject

#ifdef __cplusplus
    extern "C" {
#endif

#pragma mark -
#pragma mark Callbacks
        
        typedef void (*QuagoUnityOnJsonCallback)(const char *, const char *);
        typedef void (*QuagoUnityOnLogMessage)(const int, const char *, const char *);
        
#pragma mark -
#pragma mark_Nullable Initialisation
        
        void initializeWithSettings(const char * appToken, int flavor, int logLevel, int maxSegments,
                                    int wrapper, const char * version,
                                    bool enableManualMotionDispatcher, bool enableManualKeysDispatcher,
                                    QuagoUnityOnJsonCallback unityOnJsonCallback,
                                    QuagoUnityOnLogMessage unityOnLogMessage);
        
#pragma mark -
#pragma mark begin/end Segment Methods
        
        void beginSegment(const char * name);
        void endSegment();
        
#pragma mark -
#pragma mark Set user identifier
        
        void setUserId(const char * userId);
        void setAdditionalId(const char * additionalId);
        
#pragma mark -
#pragma mark Send KeyValues and others

        /* Set custom metadata key,value for running context */
        void setKeyValues(const char * value, const char* key);

#pragma mark -
#pragma mark Getters
        
        const char* getSessionId();
        
#ifdef __cplusplus
    }
#endif
@end
