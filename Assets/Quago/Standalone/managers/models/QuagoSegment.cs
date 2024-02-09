/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System.Collections.Generic;

public class QuagoSegment
{
    public string name, segmentId;
    public Dictionary<string, string> mapKeyValues;
    public long timeStart, timeEnd;
    public long uptimeStart, uptimeEnd;
    public QuagoMetaInformation metaData;
    public ResolutionDataPoint resolutionData;
    public string userId, additionalId;
    public int segmentCounter;

    public override string ToString()
    {
        return "QuagoSegment{" +
                "name='" + name + '\'' +
                ", mapKeyValues=" + mapKeyValues +
                ", startTime=" + timeStart +
                ", endTime=" + timeEnd +
                ", uptimeStart=" + uptimeStart +
                ", uptimeEnd=" + uptimeEnd +
                '}';
    }

    public QuagoSegment(string name, string segmentId,
                        QuagoMetaInformation metaData,
                        ResolutionDataPoint resolutionData) : this(name, null, segmentId, metaData, resolutionData)
    { }

    public QuagoSegment(string name, string userId,
                        string segmentId,
                        QuagoMetaInformation metaData,
                        ResolutionDataPoint resolutionData)
    {
        this.name = name;
        this.userId = userId;
        this.segmentId = segmentId;
        this.metaData = metaData;
        this.mapKeyValues = new();
        this.resolutionData = resolutionData;
    }

    public void setStartingTimes(long startTime, long startBootTime)
    {
        this.timeStart = startTime;
        this.uptimeStart = startBootTime;
    }

    public void setEndingTimes(long endTime, long uptimeEnd)
    {
        this.timeEnd = endTime;
        this.uptimeEnd = uptimeEnd;
    }

    public string getName()
    {
        return name;
    }

    public Dictionary<string, string> getMapKeyValues()
    {
        return mapKeyValues;
    }

    public long getTimeStart()
    {
        return timeStart;
    }

    public long getUptimeStart()
    {
        return uptimeStart;
    }

    public long getTimeEnd()
    {
        return timeEnd;
    }

    public long getUptimeEnd()
    {
        return uptimeEnd;
    }

    public void setKeyValue(string key, string value)
    {
        mapKeyValues.Add(key, value);
    }
}
#endif