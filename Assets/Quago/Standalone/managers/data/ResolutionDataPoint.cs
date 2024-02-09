/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;

public class ResolutionDataPoint : DataPoint
{
    public int screenWidth, screenHeight, appWidth, appHeight,
        appWindowPosX, appWindowPosY, refreshRate;
    public float dpi;
    public bool isFullScreen;

    public ResolutionDataPoint()
    {
    }

    public ResolutionDataPoint(ResolutionDataPoint data)
    {
        if (data == null) return;
        data.copy(this);
    }

    public ResolutionDataPoint(long eventTime,
        int screenWidth, int screenHeight,
        int appWidth, int appHeight,
        int appWindowPosX, int appWindowPosY,
        int refreshRate, float dpi, bool isFullScreen)
    {
        this.eventTime = eventTime;
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        this.appWidth = appWidth;
        this.appHeight = appHeight;
        this.appWindowPosX = appWindowPosX;
        this.appWindowPosY = appWindowPosY;
        this.refreshRate = refreshRate;
        this.dpi = dpi;
        this.isFullScreen = isFullScreen;
    }

    public override void copy(DataPoint to)
    {
        if (to == null || !to.GetType().IsAssignableFrom(typeof(ResolutionDataPoint))) return;
        ResolutionDataPoint p = (ResolutionDataPoint)to;
        p.eventTime = eventTime;
        p.screenWidth = screenWidth;
        p.screenHeight = screenHeight;
        p.appWidth = appWidth;
        p.appHeight = appHeight;
        p.appWindowPosX = appWindowPosX;
        p.appWindowPosY = appWindowPosY;
        p.refreshRate = refreshRate;
        p.dpi = dpi;
        p.isFullScreen = isFullScreen;
    }

    public override DataPoint clone()
    {
        ResolutionDataPoint data = new();
        copy(data);
        return data;
    }

    public override bool isEquals(DataPoint to)
    {
        return isSimilar(to) && eventTime == to.eventTime;
    }

    public override bool isSimilar(DataPoint to)
    {
        if (to == null || !to.GetType().IsAssignableFrom(typeof(ResolutionDataPoint))) return false;
        ResolutionDataPoint p = (ResolutionDataPoint)to;
        return screenWidth == p.screenWidth &&
                screenHeight == p.screenHeight &&
                appWidth == p.appWidth &&
                appHeight == p.appHeight &&
                appWindowPosX == p.appWindowPosX &&
                appWindowPosY == p.appWindowPosY &&
                refreshRate == p.refreshRate &&
                dpi == p.dpi;
    }

    public string StrCompare(ResolutionDataPoint obj)
    {
        string equal = "=", notEqual = "≠";
        return /* screenWidth */    $"\nscreenWidth: {screenWidth} {(screenWidth == obj.screenWidth ? equal : notEqual)} {obj.screenWidth}\n" +
               /* screenHeight */   $"screenHeight: {screenHeight} {(screenHeight == obj.screenHeight ? equal : notEqual)} {obj.screenHeight}\n" +
               /* appWidth */       $"appWidth: {appWidth} {(appWidth == obj.appWidth ? equal : notEqual)} {obj.appWidth}\n" +
               /* appHeight */      $"appHeight: {appHeight} {(appHeight == obj.appHeight ? equal : notEqual)} {obj.appHeight}\n" +
               /* appWindowPosX */  $"appWindowPosX: {appWindowPosX} {(appWindowPosX == obj.appWindowPosX ? equal : notEqual)} {obj.appWindowPosX}\n" +
               /* appWindowPosY */  $"appWindowPosY: {appWindowPosY} {(appWindowPosY == obj.appWindowPosY ? equal : notEqual)} {obj.appWindowPosY}\n" +
               /* refreshRate */    $"isFullScreen: {isFullScreen} {(isFullScreen == obj.isFullScreen ? equal : notEqual)} {obj.isFullScreen}\n" +
               /* refreshRate */    $"refreshRate: {refreshRate} {(refreshRate == obj.refreshRate ? equal : notEqual)} {obj.refreshRate}\n" +
               /* dpi */            $"dpi: {dpi} {(dpi == obj.dpi ? equal : notEqual)} {obj.dpi}";
    }

    public override string toString()
    {
        return $"{{timestamp={eventTime}, screenWidth={screenWidth}, screenHeight={screenHeight}, appWidth={appWidth}, appHeight={appHeight}, appWindowPosX={appWindowPosX}, appWindowPosY={appWindowPosY}, isFullScreen={isFullScreen}, refreshRate={refreshRate}, dpi={dpi}}}";
    }

    public override double[] exportValues()
    {
        return new double[]{
                eventTime,
                screenWidth,
                screenHeight,
                appWidth,
                appHeight,
                appWindowPosX,
                appWindowPosY,
                refreshRate,
                dpi,
                isFullScreen ? 1 : 0
        };
    }

    public override float distanceTo(DataPoint dataPoint)
    {
        return 0;
    }
}
#endif