/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using UnityEngine;

public class Logger
{
    private static readonly string tagFormat = "{0}: {1}";
    private static readonly string strFormat = "{0} -> {1}() : {2}";
    private static readonly string TAG = "QUAGO-SDK";
    private static readonly string EMPTY = "";
    private readonly string className;
    private static QuagoSettings.LogLevel priorityLevel = QuagoSettings.LogLevel.VERBOSE;
    private static QuagoLogger.OnLog listener;

    //public delegate void OnLoggerListener(QuagoSettings.LogLevel priority, string tag, string msg, Exception exception);

    /*....................................Constructor.Methods.....................................*/

    /**
     * Set log level from the given levels:
     * <p>
     * {@link QuagoSettings.LogLevel#VERBOSE}  // enable all logging
     * {@link QuagoSettings.LogLevel#DEBUG}    // enable debug logging
     * {@link QuagoSettings.LogLevel#INFO}     // the default
     * {@link QuagoSettings.LogLevel#WARNING}  // disable info logging
     * {@link QuagoSettings.LogLevel#ERROR}    // disable warnings as well
     * {@link QuagoSettings.LogLevel#DISABLED} // disable all logging
     *
     * @param level
     */
    public static void SetLoggerLevel(QuagoSettings.LogLevel level)
    {
        priorityLevel = level;
    }

    public static int GetLoggerLevel()
    {
        return (int)priorityLevel;
    }

    /**
     * Check if the log message you about to create should be created and passed to the {@url Logger}
     *
     * @param priority
     * @return true if you can create/send the log message.
     */
    public static bool AskForPermission(QuagoSettings.LogLevel priority)
    {
        return (int)priority >= (int)priorityLevel;
    }

    public static void SetOnLoggerListener(QuagoLogger.OnLog listener)
    {
        Logger.listener = listener;
    }

    public Logger(Type clazz)
    {
        this.className = clazz.Name;
    }

    public Logger(string className)
    {
        this.className = className;
    }

    /*.......................................Public.Methods.......................................*/

    public void V(string method, string msg, params object[] args)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.VERBOSE)) return;
        Log(QuagoSettings.LogLevel.VERBOSE, method, msg, null, args);
    }

    public void V(string method, string msg, Exception t)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.VERBOSE)) return;
        Log(QuagoSettings.LogLevel.VERBOSE, method, msg, t);
    }

    public void V(string method, Exception t)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.VERBOSE)) return;
        Log(QuagoSettings.LogLevel.VERBOSE, method, EMPTY, t);
    }


    public void D(string method, string msg, params object[] args)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.DEBUG)) return;
        Log(QuagoSettings.LogLevel.DEBUG, method, msg, null, args);
    }

    public void D(string method, string msg, Exception t)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.DEBUG)) return;
        Log(QuagoSettings.LogLevel.DEBUG, method, msg, t);
    }

    public void D(string method, Exception t)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.DEBUG)) return;
        Log(QuagoSettings.LogLevel.DEBUG, method, EMPTY, t);
    }


    public void I(string method, string msg, params object[] args)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.INFO)) return;
        Log(QuagoSettings.LogLevel.INFO, method, msg, null, args);
    }

    public void I(string method, string msg, Exception t)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.INFO)) return;
        Log(QuagoSettings.LogLevel.INFO, method, msg, t);
    }

    public void I(string method, Exception t)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.INFO)) return;
        Log(QuagoSettings.LogLevel.INFO, method, EMPTY, t);
    }


    public void W(string method, string msg, params object[] args)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.WARNING)) return;
        Log(QuagoSettings.LogLevel.WARNING, method, msg, null, args);
    }

    public void W(string method, string msg, Exception t)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.WARNING)) return;
        Log(QuagoSettings.LogLevel.WARNING, method, msg, t);
    }

    public void W(string method, Exception t)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.WARNING)) return;
        Log(QuagoSettings.LogLevel.WARNING, method, EMPTY, t);
    }


    public void E(string method, string msg, params object[] args)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.ERROR)) return;
        Log(QuagoSettings.LogLevel.ERROR, method, msg, null, args);
    }

    public void E(string method, string msg, Exception t)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.ERROR)) return;
        Log(QuagoSettings.LogLevel.ERROR, method, msg, t);
    }

    public void E(string method, Exception t)
    {
        if (!AskForPermission(QuagoSettings.LogLevel.ERROR)) return;
        Log(QuagoSettings.LogLevel.ERROR, method, EMPTY, t);
    }

    /*.......................................Private.Methods......................................*/

    private void Log(QuagoSettings.LogLevel priority, string methodName, string msg)
    {
        Log(priority, methodName, msg, null, null);
    }

    private void Log(QuagoSettings.LogLevel priority, string method, string msg, Exception t, params object[] args)
    {
        string formatedMSG = args == null || args.Length == 0 ? msg : string.Format(msg, args);
        string text =
            method == null ?
                formatedMSG :
                string.Format(
                    strFormat, className, method, formatedMSG
                );

        if (listener != null)
        {
            listener(priority, TAG, text, t);
            return;
        }

        /* Print msg */
        switch (priority)
        {
            case QuagoSettings.LogLevel.VERBOSE:
            case QuagoSettings.LogLevel.DEBUG:
            case QuagoSettings.LogLevel.INFO:
                Debug.LogFormat(tagFormat, TAG, text);
                break;
            case QuagoSettings.LogLevel.WARNING:
                Debug.LogWarningFormat(tagFormat, TAG, text);
                break;
            case QuagoSettings.LogLevel.ERROR:
                Debug.LogErrorFormat(tagFormat, TAG, text);
                break;
        }

        /* Print Exception msg */
        if (t == null) return;
        Debug.LogException(t);
    }
}
#endif