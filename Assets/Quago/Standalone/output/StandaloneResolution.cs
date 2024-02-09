/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System.Collections.Generic;

public class StandaloneResolution : QuaggaSerializable
{
    public int window_pos_x, window_pos_y, refresh_rate;
    public float dpi;
    public bool is_full_screen;

    public StandaloneResolution(ResolutionDataPoint resolution)
    {
        this.window_pos_x = resolution.appWindowPosX;
        this.window_pos_y = resolution.appWindowPosY;
        this.refresh_rate = resolution.refreshRate;
        this.dpi = resolution.dpi;
        this.is_full_screen = resolution.isFullScreen;
    }

    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> map = new();
        map.Add("window_pos_x", window_pos_x);
        map.Add("window_pos_y", window_pos_y);
        map.Add("refresh_rate", refresh_rate);
        map.Add("dpi", dpi);
        map.Add("is_full_screen", is_full_screen);
        return map;
    }
}
#endif