using System;
using UnityEngine;

namespace com.quago.mobile.sdk
{
#if UNITY_ANDROID
#pragma warning disable IDE0090 // Use 'new(...)'
public class QuagoAndroid
{
    private readonly static AndroidJavaClass javaQuagoClass = 
        new AndroidJavaClass("com.quago.mobile.sdk.Quago");

    /*.......................................Public.Methods.......................................*/

    public static void initialize(QuagoSettings settings)
    {
        if (settings == null)
            throw new Exception("Init() was called using null configurations!");
        
        /* Get Unity's main Activity */
        AndroidJavaObject javaUnityActivity =
            new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<AndroidJavaObject>("currentActivity");

        /* Prepare QuagoFlavor */
        AndroidJavaObject javaQuagoFlavor = new AndroidJavaClass
            ("com.quago.mobile.sdk.QuagoSettings$QuagoFlavor")
            .CallStatic<AndroidJavaObject>(
                "valueOf",
                (int)settings.getFlavor()
            );

        /* Prepare QuagoSettings */
        AndroidJavaObject javaSettingsBuilder = new AndroidJavaClass
            ("com.quago.mobile.sdk.QuagoSettings")
            .CallStatic<AndroidJavaObject>(
                "create",
                settings.getAppToken(), javaQuagoFlavor
            );

        /* Set MaxSegments */
        javaSettingsBuilder
            .Call<AndroidJavaObject>(
                "setMaxSegments",
                settings.getMaxSegments()
            );

        /* Set TrackingMotionAmount */
        javaSettingsBuilder
            .Call<AndroidJavaObject>(
                "setTrackingMotionAmount",
                settings.getAutoMotionAmount()
            );
        
        /* Set TrackingInterval */
        javaSettingsBuilder
            .Call<AndroidJavaObject>(
                "setTrackingInterval",
                settings.getAutoMotionIntervalMillis()
            );
        
        /* Set TrackingMaxDuration */
        javaSettingsBuilder
            .Call<AndroidJavaObject>(
                "setTrackingMaxDuration",
                settings.getAutoMaxDurationMillis()
            );
        
        /* Disabled Init Segment if needed */
        if(settings.isInitSegmentDisabled())
            javaSettingsBuilder.Call<AndroidJavaObject>("disableInitSegment");

        /* Set LogLevel */
        AndroidJavaObject javaLogLevel = new AndroidJavaClass
            ("com.quago.mobile.sdk.QuagoSettings$LogLevel")
            .CallStatic<AndroidJavaObject>(
                "getByPriority",
                (int)settings.getLogLevel()
            );

        javaSettingsBuilder
            .Call<AndroidJavaObject>(
                "setLogLevel",
                javaLogLevel
            );

        /* Update Native SDK with Unit's Version */
        javaSettingsBuilder
            .Call<AndroidJavaObject>(
                "setWrapperInfo",
                settings.getWrapper(), settings.getWrapperVersion()
            );

        /* Prepare Callbacks */
        if (settings.getCallback() != null)
            javaSettingsBuilder.Call<AndroidJavaObject>(
                "setJsonCallback",
                new QuagoCallback(settings.getCallback().onJsonSegment)
                );

        if (settings.GetLogger() != null)
            javaSettingsBuilder.Call<AndroidJavaObject>(
                "overrideLogger",
                new QuagoLoggerJava(settings.GetLogger())
                );

        /* Build an settings from the Builder */
        AndroidJavaObject javaConfig = javaSettingsBuilder
            .Call<AndroidJavaObject>("build");

        /* Initialise SDK */
        javaQuagoClass.CallStatic("initialize", javaUnityActivity, javaConfig);
    }

    /*......................................Segment.Methods.......................................*/

    public static void beginSegment(string name)
    {
        javaQuagoClass.CallStatic("beginSegment", name);
    }

    public static void endSegment()
    {
        javaQuagoClass.CallStatic("endSegment");
    }

    public static void beginTracking()
    {
        javaQuagoClass.CallStatic("beginTracking");
    }

    public static void beginTracking(string userId)
    {
        javaQuagoClass.CallStatic("beginTracking", userId);
    }

    public static void endTracking()
    {
        javaQuagoClass.CallStatic("endTracking");
    }

    /*......................................Meta.Data.Methods.....................................*/

    public static void setKeyValues(string key, string value)
    {
        javaQuagoClass.CallStatic("setKeyValues", key, value);
    }

    public static void setUserId(string userId)
    {
        javaQuagoClass.CallStatic("setUserId", userId);
    }

    public static void setAdditionalId(string additionalId)
    {
        javaQuagoClass.CallStatic("setAdditionalId", additionalId);
    }

    /*.......................................Getter.Methods......................................*/

    public static string getSessionId()
    {
        return javaQuagoClass.CallStatic<string>("getSessionId");
    }
}
#pragma warning restore IDE0090 // Use 'new(...)'
#endif
}