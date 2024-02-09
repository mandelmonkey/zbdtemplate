/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueryManager
{
    protected static Logger LOG = new Logger("QueryManager");

    /* Constants */
    public static long ITERATE_BY_MILLIS = 1000L;
    public static int MAX_EVENTS_PER_ITERATION = 1000;

    private QueryManager()
    {
    }

    /**
     * Query a list of {@link DataPoint}s and insert them into the exportTo list, makes sure their
     * are no consecutive duplicated data points.
     *
     * @param source     from where to read the DataPoints
     * @param exportTo   where to export the relevant DataPoints
     * @param fromMillis start timestamp
     * @param toMillis   end timestamp
     * @param maximum    amount of relevant data points to insert to the exportTo list.
     */
    public static void querySensors<T>(DataPointContainer<T> source,
                                    List<Double[]> exportTo,
                                    long fromMillis, long toMillis,
                                    int maximum) where T : DataPoint, new()
    {
        /* DataPointContainer doesn't contain any DataPoints */
        if (source == null || source.index == -1 || exportTo == null) return;
        List<DataPoint> list = new();

        /* Start with Motion DataPoints then move to the other types */
        int nextIndex = source.index, maxIterations = source.arr.Length;
        DataPoint currDataPoint = source.arr[0].clone();
        DataPoint lastDataPoint = null;

        while (maxIterations > 0)
        {
            /* Synchronized read into a temporary local copy */
            lock (source.arr[nextIndex])
            {
                source.arr[nextIndex].copy(currDataPoint);
            }

            /* Insertion conditions */
            if (currDataPoint.eventTime != 0 &&
                    currDataPoint.eventTime >= fromMillis &&
                    currDataPoint.eventTime <= toMillis &&
                    (maximum == 0 || list.Count < maximum))
            {
                /*.....................Eliminate Duplications Pre-Processing......................*/
                /*.........................For Touch Move DataPoints only.........................*/

                /* Copy of the temporary data for saving in the output list and in the maps */
                if (lastDataPoint == null || !lastDataPoint.isSimilar(currDataPoint))
                    list.Add(lastDataPoint = currDataPoint.clone());
            }

            /* Iterations */
            if (--nextIndex < 0) nextIndex = source.arr.Length - 1;
            maxIterations--;
        }

        /* Normalize export and reverse order */
        prepareList(list, exportTo);
    }

    public static long countTouches(DataPointContainer<MouseDataPoint> source,
                                    long fromMillis, long toMillis,
                                    int maximumCount)
    {
        if (source.index == -1) return 0;
        long count = 0;
        int nextIndex = source.index, maxIterations = source.arr.Length;

        bool isInTimeRange;
        while (maxIterations > 0)
        {
            /* lock read into a temporary local copy */
            lock (source.arr[nextIndex])
            {
                isInTimeRange = source.arr[nextIndex].eventTime >= fromMillis &&
                        source.arr[nextIndex].eventTime <= toMillis;
            }
            if (isInTimeRange) count++;
            if (count >= maximumCount) break;

            /* Iterations */
            if (--nextIndex < 0) nextIndex = source.arr.Length - 1;
            maxIterations--;
        }

        return count;
    }

    /**
     * Queries a list of {@link MouseDataPoint} after pre-processing (removal of duplications)
     * Due to the fact that touch events are not passing any sampling filter then there is no
     * need to delete the nulls (duplicates) in the current method, instead they can be removed
     * at the calling method that will iterate and export the values into a Segment.
     *
     * @param source     the source from which to query.
     * @param fromMillis start timestamp
     * @param toMillis   end timestamp
     * @param maximum    the maximum preprocessed events to return (0 = unlimited)
     * @return list of preprocessed and filtered events.
     */
    public static List<MouseDataPoint> queryTouch(DataPointContainer<MouseDataPoint> source,
                                                   long fromMillis, long toMillis,
                                                   int maximum)
    {
        List<MouseDataPoint> list = new();
        /* DataPointContainer doesn't contain any DataPoints */
        if (source.index == -1) return list;

        /* Start with Touch DataPoints then move to the other types */
        int nextIndex = source.index, maxIterations = source.arr.Length;

        MouseDataPoint currTouchData = new MouseDataPoint();
        int noneNulls = 0;

        while (maxIterations > 0)
        {
            /* lock read into a temporary local copy */
            lock (source.arr[nextIndex])
            {
                source.arr[nextIndex].copy(currTouchData);
            }

            /* Insertion conditions */
            if (currTouchData.eventTime >= fromMillis &&
                    currTouchData.eventTime <= toMillis &&
                    (maximum == 0 || noneNulls < maximum))
            {
                /* Insert to list, worst case we remove (null) it in later iterations */
                list.Add((MouseDataPoint)currTouchData.clone());
                noneNulls++;
            }

            /* Iterations */
            if (--nextIndex < 0) nextIndex = source.arr.Length - 1;
            maxIterations--;
        }
        return list;
    }

    /**
     * Queries a list of {@link KeyDataPoint} in their none exported representation and in reverse.
     *
     * @param source     the source from which to query.
     * @param fromMillis start timestamp
     * @param toMillis   end timestamp
     * @return list of preprocessed and filtered events.
     */
    public static List<KeyDataPoint> queryKeys(DataPointContainer<KeyDataPoint> source,
                                               long fromMillis, long toMillis,
                                               int maximum)
    {
        List<KeyDataPoint> list = new();
        /* DataPointContainer doesn't contain any DataPoints */
        if (source.index == -1) return list;

        /* Start with Touch DataPoints then move to the other types */
        int nextIndex = source.index, maxIterations = source.arr.Length;
        KeyDataPoint data = new();

        while (maxIterations > 0)
        {
            /* lock read into a temporary local copy */
            lock (source.arr[nextIndex])
            {
                source.arr[nextIndex].copy(data);
            }
            

            /* Insertion conditions: Check for time and capacity */
            if (data.eventTime >= fromMillis &&
                    data.eventTime <= toMillis &&
                    (maximum == 0 || list.Count < maximum))
            {
                /*.....................Eliminate Duplications Pre-Processing......................*/
                /*.........................For Touch Move DataPoints only.........................*/
                /* Insert to list, worst case we remove it later */
                list.Add((KeyDataPoint)data.clone());
            }

            /* Iterations */
            if (--nextIndex < 0) nextIndex = source.arr.Length - 1;
            maxIterations--;
        }

        return list;
    }

    /**
     * Queries a list of {@link KeyDataPoint} in their exported representation (array of values).
     *
     * @param source     the source from which to query.
     * @param fromMillis start timestamp
     * @param toMillis   end timestamp
     * @return list of preprocessed and filtered events.
     */
    public static List<double[]> queryKeysExported(DataPointContainer<KeyDataPoint> source,
                                                   long fromMillis, long toMillis,
                                                   int maximum)
    {
        List<double[]> list = new();
        /* DataPointContainer doesn't contain any DataPoints */
        if (source.index == -1) return list;

        /* Start with Touch DataPoints then move to the other types */
        int nextIndex = source.index, maxIterations = source.arr.Length;
        KeyDataPoint data = new();

        while (maxIterations > 0)
        {
            /* lock read into a temporary local copy */
            lock (source.arr[nextIndex])
            {
                source.arr[nextIndex].copy(data);
            }

            /* Insertion conditions: Check for time and capacity */
            if (data.eventTime >= fromMillis &&
                    data.eventTime <= toMillis &&
                    (maximum == 0 || list.Count < maximum))
            {
                list.Add(data.exportValues());
            }

            /* Iterations */
            if (--nextIndex < 0) nextIndex = source.arr.Length - 1;
            maxIterations--;
        }

        /* Because we were reading from back to front, reverse is in order */
        list.Reverse();
        return list;
    }

    public static DataSegment queryDataSegment(DataManager manager,
                                               QuagoSegment segment,
                                               long fromMillis, long toMillis,
                                               QuagoSettings settings)
    {
        DataSegment data = new();

        /* Export the values of all the queried DataPoints into our segment */
        /* Because we were reading from back to front, reverse is in order */
        /* Also we are not using filters so we have to remove null DataPoints (duplicates) */
        data.seq_life = new();
        /* Touch */
        data.seq_mouse = new();
        /* Keys */
        data.seq_key = new();
        /* Resolution */
        data.seq_resolution = new();

        List<MouseDataPoint> touchList = queryTouch(
                manager.arrMotion,
                fromMillis,
                toMillis,
                settings.getQueryMaxCount(QuagoSettings.QuagoQueryMaxCount.MOUSE)
        );

        /* We want to query DataPoints that correlate as much as possible with the touches */
        /* Hence we need to find the last to earliest touch DataPoint's eventTime */
        /* In case there are no touches, query maximum sensor events between the from/to millis */
        long fromTouchMillis =
                touchList.Count == 0
                        ? fromMillis
                        : touchList[touchList.Count - 1].eventTime;

        long toTouchMillis =
                touchList.Count == 0
                        ? toMillis
                        : touchList[0].eventTime;

        /* Key events will be queried until toTouchMillis or until their maximum */
        List<KeyDataPoint> keysList = queryKeys(
                manager.arrKey,
                fromTouchMillis, toMillis,
                settings.getQueryMaxCount(QuagoSettings.QuagoQueryMaxCount.KEYS)
        );

        /* Update metadata with the new queried data */
        data.meta_unity_desktop = segment.metaData.Clone();

        /* Export and reverse order for Motion events */
        prepareList(touchList, data.seq_mouse);
        prepareList(keysList, data.seq_key);

        /* sensor events will be queried fromTouchMillis to toTouchMillis, until their maximum */

        /* Resolution */
        querySensors(
                manager.arrResolution, data.seq_resolution,
                fromMillis, toTouchMillis,
                settings.getQueryMaxCount(QuagoSettings.QuagoQueryMaxCount.RESOLUTION)
        );
        /* LifeCycle */
        querySensors(
                manager.arrLifeCycle, data.seq_life,
                fromTouchMillis, toTouchMillis,
                settings.getQueryMaxCount(QuagoSettings.QuagoQueryMaxCount.LIFECYCLE)
        );

        normalizeEventTime(fromMillis, data.seq_mouse);
        normalizeEventTime(fromMillis, data.seq_key);
        normalizeEventTime(fromMillis, data.seq_life);
        normalizeEventTime(fromMillis, data.seq_resolution);

        return data;
    }

    /**
     * Iterates on the list and does the following:
     * <p>
     * 1) Export the values in array form instead of DataPoint form.
     * 2) Reverse the order of the list.
     *
     * @param from
     * @param to
     */
    protected static void prepareList<T>(List<T> from, List<double[]> to) where T : DataPoint
    {
        if (from == null) return;
        while (!(from.Count == 0))
        {
            int lastIndex = from.Count - 1;
            T p = from[lastIndex];
            /* Export and add in reverse */
            if (p != null) to.Add(p.exportValues());
            from.RemoveAt(lastIndex);
        }
    }

    protected static void normalizeEventDownTime(long fromMillis, List<double[]> list)
    {
        if (list == null) return;
        foreach (double[] obj in list)
        {
            if (!obj.GetType().IsAssignableFrom(typeof(double[]))) continue;
            double[] objects = (double[])obj;
            /* Normalize eventTime and downTime */
            objects[0] = Math.Max(0.0, (double)objects[0] - (double)fromMillis);
            objects[1] = Math.Max(0.0, (double)objects[1] - (double)fromMillis);
        }
    }

    protected static void normalizeEventTime(long fromMillis, List<double[]> list)
    {
        if (list == null) return;
        foreach (double[] obj in list)
        {
            if (!obj.GetType().IsAssignableFrom(typeof(double[]))) continue;
            double[] objects = (double[])obj;
            /* Normalize eventTime */
            objects[0] = Math.Max(0, (double)objects[0] - (double)fromMillis);
        }
    }
}
#endif