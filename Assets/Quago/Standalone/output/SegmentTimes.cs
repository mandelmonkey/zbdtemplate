/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System.Collections.Generic;

public class SegmentTimes : QuaggaSerializable
{
    /* duration since 1970 gregorian calendar in millis */
    public long ts_start, ts_end;
    /* duration since device booted up in millis */
    public long uptime_start, uptime_end;
    /* duration since 1970 gregorian calendar in millis since SDK initialized */
    public long ts_sdk_init,
    /* duration since device booted up in millis since SDK initialized */
    uptime_sdk_init;

    public SegmentTimes(long ts_start, long ts_end, long uptime_start, long uptime_end, long ts_sdk_init, long uptime_sdk_init)
    {
        this.ts_start = ts_start;
        this.ts_end = ts_end;
        this.uptime_start = uptime_start;
        this.uptime_end = uptime_end;
        this.ts_sdk_init = ts_sdk_init;
        this.uptime_sdk_init = uptime_sdk_init;
    }

    public override string ToString()
    {
        return "Times{" +
                "ts_start=" + ts_start +
                ", ts_end=" + ts_end +
                ", uptime_start=" + uptime_start +
                ", uptime_end=" + uptime_end +
                ", ts_sdk_init=" + ts_sdk_init +
                ", uptime_sdk_init=" + uptime_sdk_init +
                '}';
    }

    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> map = new();
        map.Add("ts_start", ts_start);
        map.Add("ts_end", ts_end);
        map.Add("uptime_start", uptime_start);
        map.Add("uptime_end", uptime_end);
        map.Add("ts_sdk_init", ts_sdk_init);
        map.Add("uptime_sdk_init", uptime_sdk_init);
        return map;
    }
}
#endif