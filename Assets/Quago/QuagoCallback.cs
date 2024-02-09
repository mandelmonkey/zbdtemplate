using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuagoCallback
#if UNITY_ANDROID
    : AndroidJavaProxy
#endif
{
    public delegate void OnJsonSegment(string headers, string payloa);
    private readonly OnJsonSegment action;

    public QuagoCallback(QuagoCallback.OnJsonSegment action)
#if UNITY_ANDROID
        : base("com.quago.mobile.sdk.QuagoSettings$QuagoCallback")
#endif
    {
        this.action = action;
    }

    /* Method name must be the same as in Java src */
#pragma warning disable IDE1006 // Naming Styles
    public void onJsonSegment(string headers, string payload)
#pragma warning restore IDE1006 // Naming Styles
    {
        if (action == null)return;
        action(headers, payload);
    }    
}
