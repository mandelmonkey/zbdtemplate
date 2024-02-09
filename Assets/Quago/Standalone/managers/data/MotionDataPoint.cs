/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;

public class MouseDataPoint : DataPoint
{
    public float x, y;
    public int type, button;

    public MouseDataPoint()
    {
    }

    public override void copy(DataPoint to)
    {
        if (to == null || !to.GetType().IsAssignableFrom(typeof(MouseDataPoint))) return;
        MouseDataPoint p = (MouseDataPoint)to;
        p.eventTime = eventTime;
        p.type = type;
        p.x = x;
        p.y = y;
        p.button = button;
    }


    public override DataPoint clone()
    {
        MouseDataPoint clone = new();
        copy(clone);
        return clone;
    }

    /**
     * Checks with the given param if both have equal values.
     *
     * @param to
     * @return
     */

    public override bool isEquals(DataPoint to)
    {
        return isSimilar(to) && eventTime == to.eventTime;
    }


    public override string toString()
    {
        return $"[{eventTime}, {typeToString(type)}, {x}, {y}, {button}]";
    }


    public override double[] exportValues()
    {
        return new double[]{
                eventTime,
                type,
                x,
                y,
                button
        };
    }


    public override float distanceTo(DataPoint dataPoint)
    {
        if (!dataPoint.GetType().IsAssignableFrom(typeof(MouseDataPoint))) return 0;
        MouseDataPoint p = ((MouseDataPoint)dataPoint);
        return Math.Abs(x - p.x) + Math.Abs(y - p.y);
    }

    protected string typeToString(int type)
    {
        switch ((BiometricType)type)
        {
            case BiometricType.MOTION_DOWN:
                return "↓";
            case BiometricType.MOTION_POINTER_DOWN:
                return "↘";
            case BiometricType.MOTION_MOVE:
                return "∞";
            case BiometricType.MOTION_POINTER_UP:
                return "↗";
            case BiometricType.MOTION_UP:
                return "↑";
            case BiometricType.MOUSE_WHEEL:
                return "¤";
            default:
                return "?";
        }
    }

    /**
     * Checks with the given param if both have equal values except {@link MotionDataPoint#eventTime}.
     *
     * @param to
     * @return
     */

    public override bool isSimilar(DataPoint to)
    {
        if (to == null || !to.GetType().IsAssignableFrom(typeof(MouseDataPoint))) return false;
        MouseDataPoint p = (MouseDataPoint)to;
        return type == p.type &&
            x == p.x &&
            y == p.y &&
            button == p.button;
    }
}
#endif