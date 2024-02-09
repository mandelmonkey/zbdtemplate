/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System.Collections.Generic;
using System.Text;

public class DataSegment
{
    public string app_token;

    /**
     * The main common id with the customer
     */
    public string user_id;
    /**
     * Optional additions common id
     */
    public string additional_id;

    /**
     * 1 = Android, 2 = iOS, 3 = pc (unity standalone), etc.
     * Note: will never change.
     */
    public const int platform = 3;

    /**
     * 0 = Native
     */
    public int wrapper;
    /**
     * Holds the version name of the wrapper which uses this SDK (null for native).
     */
    public string wrapper_version;

    public string seg_name, seg_id, session_id;
    public string app_package_name;

    /**
     * in iOS -> release version.
     */
    public string app_version_name;
    public string sdk_version_name;

    public int seg_count;

    /**
     * basic = 1, tailored = 2
     */
    public int mode;
    /**
     * AUTHENTIC(0), UNAUTHENTIC(1), PRODUCTION(2), DEVELOPMENT(3)
     */
    public int flavor;

    public QuagoMetaInformation meta_unity_desktop;
    public SegmentTimes times;

    public int screen_width, screen_height;
    public int app_width, app_height;

    public StandaloneResolution resolution_unity_desktop;
    public List<string[]> key_values;
    public List<string[]> key_bindings;

    /**
     * what prompted the segment to be sent.
     */
    public int prompt;

    /**
     * Life Cycle
     */
    public List<double[]> seq_life;
    /**
     * Touch
     */
    public List<double[]> seq_mouse;
    /**
     * Keys
     */
    public List<double[]> seq_key;
    /**
     * Resolution
     */
    public List<double[]> seq_resolution;

    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> map = new();
        map.Add("app_token", app_token);
        map.Add("user_id", user_id);
        map.Add("additional_id", additional_id);
        map.Add("platform", platform);
        map.Add("wrapper", wrapper);
        map.Add("wrapper_version", wrapper_version);
        map.Add("seg_name", seg_name);
        map.Add("seg_id", seg_id);
        map.Add("session_id", session_id);
        map.Add("app_package_name", app_package_name);
        map.Add("app_version_name", app_version_name);
        map.Add("sdk_version_name", sdk_version_name);
        map.Add("seg_count", seg_count);
        map.Add("mode", mode);
        map.Add("flavor", flavor);
        map.Add("meta_unity_desktop", meta_unity_desktop.ToDictionary());
        map.Add("times", times.ToDictionary());
        map.Add("screen_width", screen_width);
        map.Add("screen_height", screen_height);
        map.Add("app_width", app_width);
        map.Add("app_height", app_height);
        map.Add("resolution_unity_desktop", resolution_unity_desktop.ToDictionary());
        map.Add("key_values", key_values);
        map.Add("key_bindings", key_bindings);
        map.Add("prompt", prompt);
        map.Add("seq_life", seq_life);
        map.Add("seq_mouse", seq_mouse);
        map.Add("seq_key", seq_key);
        map.Add("seq_resolution", seq_resolution);
        return map;
    }
    
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("name = ").Append(seg_name).Append("\n");
        sb.Append("appToken = ").Append(app_token).Append("\n");
        sb.Append("sessionId = ").Append(session_id).Append("\n");
        sb.Append("userId = ").Append(user_id).Append("\n");
        sb.Append("matchingId = ").Append(additional_id).Append("\n");
        sb.Append("mapKeyValues = ").Append(key_values).Append("\n");
        sb.Append("times = ").Append(times).Append("\n");
        sb.Append("platform = ").Append(platform).Append("\n");
        sb.Append("index = ").Append(seg_count).Append("\n");
        sb.Append("mode = ").Append(mode).Append("\n");
        sb.Append("flavor = ").Append(flavor).Append("\n");
        sb.Append("resolutionData = ").Append(resolution_unity_desktop).Append("\n");
        sb.Append("metaData = ").Append(meta_unity_desktop).Append("\n");
        sb.Append("seq_life = ");
        toStringHelper(sb, seq_life);
        sb.Append("seq_mouse = ");
        toStringHelper(sb, seq_mouse);
        sb.Append("seq_key = ");
        toStringHelper(sb, seq_key);
        sb.Append("seq_resolution = ");
        toStringHelper(sb, seq_resolution);
        return sb.ToString();
    }

    protected void toStringHelper(StringBuilder sb, List<double[]> list)
    {
        if (list == null) return;
        sb.Append(list.Count).Append("\n");
    }
}
#endif