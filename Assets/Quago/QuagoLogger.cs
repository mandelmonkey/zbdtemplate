using System;
using UnityEngine;

public class QuagoLogger
{
    public delegate void OnLog(QuagoSettings.LogLevel priority, string tag, string msg, Exception Exception);
    public readonly OnLog action;

    public QuagoLogger(QuagoLogger.OnLog action)
    {
        this.action = action;
    }
}
