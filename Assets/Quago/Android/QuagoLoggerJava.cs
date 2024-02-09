using System;
using UnityEngine;

public class QuagoLoggerJava
#if UNITY_ANDROID
    : AndroidJavaProxy
#endif
{
    public delegate void OnLog(int priority, string tag, string msg, AndroidJavaObject Exception);
    private readonly QuagoLogger.OnLog listener;

    public QuagoLoggerJava(QuagoLogger.OnLog listener)
#if UNITY_ANDROID
        : base("com.quago.mobile.sdk.utils.QuagoLogger$IQuagoLog")
#endif
    {
        this.listener = listener;
    }

    /* Method name must be the same as in Java src */
#pragma warning disable IDE1006 // Naming Styles
    public void onLog(int priority, string tag, string msg, AndroidJavaObject Exception)
#pragma warning restore IDE1006 // Naming Styles
    {
        if (listener == null) return;
        Exception e;
        if (Exception != null)
            e = new Exception(Exception.Call<string>("getMessage"));
        else
            e = null;
        listener((QuagoSettings.LogLevel)priority, tag, msg, e);
    }
}
