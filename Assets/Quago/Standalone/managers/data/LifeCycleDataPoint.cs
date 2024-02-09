/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCycleDataPoint : DataPoint
{
    public const int CODE_ON_RESUME = 1;
    public const int CODE_ON_PAUSE = 2;
    public const int CODE_ON_STOP = 3;

    public int type;

    public LifeCycleDataPoint()
    {
    }

    public LifeCycleDataPoint(BiometricType type)
    {
        this.type = translateBiometricType(type);
        this.eventTime = QuagoStandalone.ElapsedMilliseconds();
    }

    public override void copy(DataPoint to)
    {
        if (to == null || !to.GetType().IsAssignableFrom(typeof(LifeCycleDataPoint))) return;
        LifeCycleDataPoint p = (LifeCycleDataPoint)to;
        p.type = type;
        p.eventTime = eventTime;
    }

    public override DataPoint clone()
    {
        LifeCycleDataPoint data = new();
        copy(data);
        return data;
    }

    public override bool isEquals(DataPoint to)
    {
        return isSimilar(to) && eventTime == to.eventTime;
    }

    public override bool isSimilar(DataPoint to)
    {
        if (to == null || !to.GetType().IsAssignableFrom(typeof(LifeCycleDataPoint))) return false;
        LifeCycleDataPoint p = (LifeCycleDataPoint)to;
        return type == p.type;
    }

    public override string toString()
    {
        return $"[{eventTime}, {translateType(type)}]";
    }

    public override double[] exportValues()
    {
        return new double[] { eventTime, type };
    }

    public override float distanceTo(DataPoint dataPoint)
    {
        return 0;
    }

    public static int translateBiometricType(BiometricType type)
    {
        switch (type)
        {
            case BiometricType.ON_RESUME:
                return CODE_ON_RESUME;
            case BiometricType.ON_PAUSE:
                return CODE_ON_PAUSE;
            case BiometricType.ON_STOP:
                return CODE_ON_STOP;
            default:
                /* Error */
                return 0;
        }
    }

    public static BiometricType translateType(int type)
    {
        switch (type)
        {
            case CODE_ON_RESUME:
                return BiometricType.ON_RESUME;
            case CODE_ON_PAUSE:
                return BiometricType.ON_PAUSE;
            case CODE_ON_STOP:
                return BiometricType.ON_STOP;
            default:
                /* Error */
                return BiometricType.NULL;
        }
    }
}
#endif