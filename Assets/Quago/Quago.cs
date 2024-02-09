using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_ANDROID || UNITY_IOS
using com.quago.mobile.sdk;
#endif

public class Quago
{
    public const string VERSION_NAME = "1.3.1";
    public const int VERSION_CODE = 7;
    
    /*......................................Public.Methods.......................................*/

    public static void initialize(QuagoSettings.Builder builder) {
        #if UNITY_ANDROID || UNITY_IOS
            initialize(builder.build());
        #endif
    }

    public static void initialize(QuagoSettings settings) {
        Debug.Log("Quago.initialize()");
        #if UNITY_EDITOR
            return;
        #elif UNITY_ANDROID
            QuagoAndroid.initialize(settings);
        #elif UNITY_IOS
            QuagoiOS.initialize(settings);
        #elif UNITY_STANDALONE //Windows/MacOSx/Linux
        #endif
    }

    public static void beginSegment(string name) {
        #if UNITY_EDITOR
            return;
        #elif UNITY_ANDROID
            QuagoAndroid.beginSegment(name);
        #elif UNITY_IOS
            QuagoiOS.beginSegment(name);
        #elif UNITY_STANDALONE //Windows/MacOSx/Linux
        #endif
    }

    public static void endSegment() {
        #if UNITY_EDITOR
            return;
        #elif UNITY_ANDROID
            QuagoAndroid.endSegment();
        #elif UNITY_IOS
            QuagoiOS.endSegment();
        #elif UNITY_STANDALONE //Windows/MacOSx/Linux
        #endif
    }

    public static void beginTracking() {
        #if UNITY_EDITOR
            return;
        #elif UNITY_ANDROID
            QuagoAndroid.beginTracking();
        #elif UNITY_IOS
            //Not supported yet
        #elif UNITY_STANDALONE //Windows/MacOSx/Linux
        #endif
    }

    public static void beginTracking(string userId) {
        #if UNITY_EDITOR
            return;
        #elif UNITY_ANDROID
            QuagoAndroid.beginTracking(userId);
        #elif UNITY_IOS
            //Not supported yet
        #elif UNITY_STANDALONE //Windows/MacOSx/Linux
        #endif
    }

    public static void endTracking() {
        #if UNITY_EDITOR
            return;
        #elif UNITY_ANDROID
            QuagoAndroid.endTracking();
        #elif UNITY_IOS
            //Not supported yet
        #elif UNITY_STANDALONE //Windows/MacOSx/Linux
        #endif
    }

    public static void setKeyValues(string key,  string value) {
        #if UNITY_EDITOR
            return;
        #elif UNITY_ANDROID
            QuagoAndroid.setKeyValues(key,value);
        #elif UNITY_IOS
            QuagoiOS.setKeyValues(key,value);
        #elif UNITY_STANDALONE //Windows/MacOSx/Linux
        #endif
    }

    public static void setUserId(string userId) {
        #if UNITY_EDITOR
            return;
        #elif UNITY_ANDROID
            QuagoAndroid.setUserId(userId);
        #elif UNITY_IOS
            QuagoiOS.setUserId(userId);
        #elif UNITY_STANDALONE //Windows/MacOSx/Linux
        #endif
    }

    public static void setAdditionalId(string additionalId) {
        #if UNITY_EDITOR
            return;
        #elif UNITY_ANDROID
            QuagoAndroid.setAdditionalId(additionalId);
        #elif UNITY_IOS
            QuagoiOS.setAdditionalId(additionalId);
        #elif UNITY_STANDALONE //Windows/MacOSx/Linux
        #endif
    }

    /*......................................Optional.Methods......................................*/

    public static string getSessionId() {
        #if UNITY_EDITOR
            return null;
        #elif UNITY_ANDROID
            return QuagoAndroid.getSessionId();
        #elif UNITY_IOS
            return QuagoiOS.getSessionId();
        #elif UNITY_STANDALONE //Windows/MacOSx/Linux
            return null;
        #else
            return null;
        #endif
    }

    /*......................................Private.Classes.......................................*/
}
