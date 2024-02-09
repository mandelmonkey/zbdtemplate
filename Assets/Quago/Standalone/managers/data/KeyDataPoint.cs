/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
public class KeyDataPoint : DataPoint
{

    public const int CODE_KEY_DOWN = 1;
    public const int CODE_KEY_UP = 2;

    public int type, keyCode;

    public override void copy(DataPoint to)
    {
        if (to == null || !to.GetType().IsAssignableFrom(typeof(KeyDataPoint))) return;
        KeyDataPoint p = (KeyDataPoint)to;
        p.type = type;
        p.keyCode = keyCode;
        p.eventTime = eventTime;
    }


    public override DataPoint clone()
    {
        KeyDataPoint data = new();
        copy(data);
        return data;
    }


    public override bool isEquals(DataPoint to)
    {
        return isSimilar(to) && eventTime == to.eventTime;
    }


    public override bool isSimilar(DataPoint to)
    {
        if (to == null || !to.GetType().IsAssignableFrom(typeof(KeyDataPoint))) return false;
        KeyDataPoint p = (KeyDataPoint)to;
        return type == p.type &&
            keyCode == p.keyCode;
    }


    public override string toString()
    {
        return $"[{typeToString(type)}, {eventTime}, {keyCode}]";
    }


    public override double[] exportValues()
    {
        return new double[]{
                eventTime,
                type,
                keyCode
        };
    }


    public override float distanceTo(DataPoint dataPoint)
    {
        return 0;
    }

    protected string typeToString(int type)
    {
        switch (type)
        {
            case CODE_KEY_DOWN:
                return "↓";
            case CODE_KEY_UP:
                return "↑";
            default:
                /* Error */
                return "?";
        }
    }

    public static int translateBiometricType(BiometricType type)
    {
        switch (type)
        {
            case BiometricType.KEY_DOWN:
                return CODE_KEY_DOWN;
            case BiometricType.KEY_UP:
                return CODE_KEY_UP;
            default:
                /* Error */
                return 0;
        }
    }

    public static BiometricType translateType(int type)
    {
        switch (type)
        {
            case CODE_KEY_DOWN:
                return BiometricType.KEY_DOWN;
            case CODE_KEY_UP:
                return BiometricType.KEY_UP;
            default:
                /* Error */
                return BiometricType.NULL;
        }
    }
}
#endif