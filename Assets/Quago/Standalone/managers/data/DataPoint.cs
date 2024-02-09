/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
public abstract class DataPoint{

    public long eventTime;

    public abstract void copy(DataPoint to);

    public abstract DataPoint clone();

    public abstract bool isEquals(DataPoint to);

    public abstract bool isSimilar(DataPoint to);

    public abstract string toString();

    public abstract double[] exportValues();

    public abstract float distanceTo(DataPoint dataPoint);
}
#endif