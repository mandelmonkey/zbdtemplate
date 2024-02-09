/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using UnityEngine;

public class DataManager
{
    protected Logger LOG = new Logger("DataManager");

    public DataPointContainer<LifeCycleDataPoint> arrLifeCycle;
    public DataPointContainer<MouseDataPoint> arrMotion;
    public DataPointContainer<KeyDataPoint> arrKey;
    public DataPointContainer<ResolutionDataPoint> arrResolution;
    protected QuagoSettings settings;

    public DataManager(QuagoSettings settings)
    {
        this.settings = settings;
        /* Build Arrays for LifeCycle changes */
        arrLifeCycle = new DataPointContainer<LifeCycleDataPoint>("LifeCycle", BiometricType.NULL, 100);

        /* Build Arrays for Resolution changes */
        arrResolution = new DataPointContainer<ResolutionDataPoint>("Resolution", BiometricType.NULL, 100);

        /* Build Arrays for Motion and Key events */
        arrMotion = new DataPointContainer<MouseDataPoint>("Motion", BiometricType.MOTION_MOVE, 20000);
        arrKey = new DataPointContainer<KeyDataPoint>("Keys", BiometricType.NULL, 10000);
    }

    /**
     * Query a {@link DataSegment} instance that contains all the exported values of
     * all data points types, by time range and max amount of events per type.
     * The range values may derive from the tracked screen, when it started and until when to query.
     * Use {@link SystemClock#uptimeMillis()} for queries until current time.
     * <p>
     * Important: Not to be used on UI Thread - instead run on a worker thread in the background.
     *
     * @param fromMillis starting time for the query, duration in millis from device boot.
     * @param toMillis   end time for the query, duration in millis from device boot.
     * @return the results inside a {@link DataSegment} instance.
     */
    public DataSegment query(QuagoSegment segment, long fromMillis, long toMillis)
    {
        return QueryManager.queryDataSegment(this, segment, fromMillis, toMillis, settings);
    }

    public DataSegment query(long fromMillis, long toMillis)
    {
        return query(null, fromMillis, toMillis);
    }

    public void onLifeCycleChanged(LifeCycleDataPoint data)
    {
        if (data == null) return;
        int nextIndex = (arrLifeCycle.index + 1) % arrLifeCycle.arr.Length;
        lock (arrLifeCycle.arr[nextIndex])
        {
            data.copy(arrLifeCycle.arr[nextIndex]);
            arrLifeCycle.index = nextIndex;

            if (Logger.AskForPermission(QuagoSettings.LogLevel.VERBOSE))
                LOG.V("onLifeCycleChanged", "{0}{1}({2})",
                        arrLifeCycle.name,
                        arrLifeCycle.arr[nextIndex].toString(),
                        arrLifeCycle.index
                );
        }
    }

    public void onResolutionChange(ResolutionDataPoint data)
    {
        if (data == null) return;
        int nextIndex = (arrResolution.index + 1) % arrResolution.arr.Length;
        lock (arrResolution.arr[nextIndex])
        {
            data.copy(arrResolution.arr[nextIndex]);
            arrResolution.index = nextIndex;

            if (Logger.AskForPermission(QuagoSettings.LogLevel.VERBOSE))
                LOG.V("onResolutionChange", "{0}{1}({2})",
                        arrResolution.name,
                        arrResolution.arr[nextIndex].toString(),
                        arrResolution.index
                );
        }
    }

    public void dispatchKey(long eventTime, int type, int keyCode)
    {
        int nextIndex = (arrKey.index + 1) % arrKey.arr.Length;
        lock (arrKey.arr[nextIndex])
        {
            arrKey.arr[nextIndex].type = type;
            arrKey.arr[nextIndex].eventTime = eventTime;
            arrKey.arr[nextIndex].keyCode = keyCode;
            arrKey.index = nextIndex;

            if (Logger.AskForPermission(QuagoSettings.LogLevel.VERBOSE))
                LOG.V("dispatchKey", "{0}{1}({2})",
                        arrKey.name,
                        arrKey.arr[nextIndex].toString(),
                        arrKey.index
                );
        }
    }

    /**
     * Receives a MotionEvent (arrives from the APP) and stores its values.
     * In Production this method will be called only by the UIThread.
     *
     * @param event Don't store/cache this parameter as the OS recycles it with different values.
     */
    public void dispatchTouch(long eventTime, BiometricType type, float x, float y, int button)
    {
        int nextIndex = (arrMotion.index + 1) % arrMotion.arr.Length;
        lock (arrMotion.arr[nextIndex])
        {
            arrMotion.arr[nextIndex].type = (int)type;
            arrMotion.arr[nextIndex].eventTime = eventTime;
            arrMotion.arr[nextIndex].x = x;
            arrMotion.arr[nextIndex].y = y;
            arrMotion.arr[nextIndex].button = button;
            arrMotion.index = nextIndex;

            if (Logger.AskForPermission(QuagoSettings.LogLevel.VERBOSE))
                LOG.V("dispatchTouch", "{0}{1}({2})",
                        arrMotion.name,
                        arrMotion.arr[nextIndex].toString(),
                        arrMotion.index
                );
        }
    }
}
#endif