/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;

public class DataPointContainer<T> where T : new()
{
    protected Logger LOG = new Logger("DataPointContainer");

    public BiometricType type;
    public string name;
    /**
     * Index to move inside the array
     */
    public int index = -1;
    /**
     * The array the data is store in a loop
     */
    public T[] arr;

    public DataPointContainer(string name, BiometricType type, int ofSize)
    {
        this.name = name;
        this.type = type;
        try
        {
            arr = new T[ofSize];
            for (int i = 0; i < ofSize; i++)
                arr[i] = new T();
        }
        catch (Exception e)
        {
            LOG.E("construct", e);
        }
    }
}
#endif