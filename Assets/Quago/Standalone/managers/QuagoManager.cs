/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

[assembly: InternalsVisibleTo("QuagoEditMode")]
public class QuagoManager
{
    protected Logger LOG;
    protected object _lock = new object();

    protected bool initialized;
    protected QuagoSettings settings;

    protected QuagoSerializer serializer = new();
    protected QuagoUUID uuid = new QuagoUUID();
    protected string sessionId;
    protected int segmentCounter = 0;
    protected bool maxSegmentsReached;
    protected bool maxTrackingSegmentsReached;

    protected DataManager dataManager;
    protected QuagoMetaInformation quagoMetaData = null;
    protected QuagoNetworkModule quagoNetworkModule;

    protected HandlerThread handler;

    protected ResolutionDataPoint lastResolutionData;
    protected bool appIsDebug;
    protected long sdkInitUptime, sdkInitTime;

    protected QuagoSegment currSegment, trackingCurrSegment;

    internal enum COMMAND : int
    {
        INIT = 1,
        ON_RESUME = 2,
        ON_PAUSE = 3,
        ON_STOP = 4,
        SEGMENT_BEGIN = 5,
        SEGMENT_END = 6,
        TRACKING_BEGIN = 7,
        TRACKING_CHECK = 8,
        TRACKING_END = 9,
        SET_KEY_VALUE = 10,
        RELEASE = 11,
        SET_USER_ID = 12,
        SET_ADDITIONAL_ID = 13,
        SEGMENT_INIT_BEGIN = 14
    }

    public QuagoManager()
    {
        LOG = new Logger(GetType().Name);
        this.sessionId = uuid.generate();
    }

    public void Initialize(QuagoSettings settings, QuagoMetaInformation quagoMetaData)
    {
        lock (_lock)
        {
            if (initialized) return;
            if (settings == null) throw new ArgumentNullException("settings is null");

            sdkInitUptime = QuagoStandalone.ElapsedMilliseconds();
            sdkInitTime = QuagoStandalone.CurrentTimestampMilliseconds();

            this.settings = settings;
            this.quagoMetaData = quagoMetaData;

            HandlerThread tempHandlerThread = handler;

            /* Creating Handler and Thread */
            handler = new HandlerThread(
                "QuagoManagerThread", new QuagoManagerHandler(this), HandlerException
            );

            /* removes old thread and handler in case of SDK reuse */
            if (tempHandlerThread != null) tempHandlerThread.QuitSafely();


            /* Network Manager */
            quagoNetworkModule = new(
                    QuagoNetworkSettings.BuildNetConfig()
                            .SetUrl("https://api.quago.io/v1/record")
                            .Build()
            );

            /* Create DataManager then check resolution then notify the Init */
            dataManager = new(settings);
            currSegment = null;
            initialized = true;
            checkResolutionChange();

            handler.SendEmptyMessage((int)COMMAND.INIT);

            if (!settings.isInitSegmentDisabled())
            {
                beginInitSegment();
                endSegment();
            }
        };
    }

    protected void HandlerException(string threadName, Exception exception)
    {
        if (Logger.AskForPermission(QuagoSettings.LogLevel.ERROR))
            LOG.E("start", "Thread = " + threadName, exception);
    }

    public bool isInitialized()
    {
        return initialized;
    }

    public string getSessionId()
    {
        return sessionId;
    }

    protected void beginInitSegment()
    {
        if (!initialized)
        {
            LOG.W("beginInitSegment", "Method called before SDK initialized!");
            return;
        }
        handler.SendMessage(
                new Message((int)COMMAND.SEGMENT_INIT_BEGIN,
                0, 0, new QuagoSegment(
                                "INIT", uuid.generate(),
                                quagoMetaData, new ResolutionDataPoint(lastResolutionData)
                        )));
    }

    public void beginSegment(string name)
    {
        if (!initialized)
        {
            LOG.W("beginSegment", "Method called before SDK initialized!");
            return;
        }
        if ("INIT".Equals(name))
        {
            LOG.E("beginSegment", "Can't use INIT as a name for segment!");
            return;
        }
        handler.SendMessage(
                new Message((int)COMMAND.SEGMENT_BEGIN,
                        0, 0, new QuagoSegment(
                                name, uuid.generate(),
                                quagoMetaData, new ResolutionDataPoint(lastResolutionData)
                        )));
    }

    public void endSegment()
    {
        if (!initialized)
        {
            LOG.W("endSegment", "Method called before SDK initialized!");
            return;
        }
        handler.SendEmptyMessage((int)COMMAND.SEGMENT_END);
    }

    public void beginTracking(string userId)
    {
        if (!initialized)
        {
            LOG.W("beginTracking", "Method called before SDK initialized!");
            return;
        }
        handler.SendMessage(
                new Message((int)COMMAND.TRACKING_BEGIN,
                        0, 0, new QuagoSegment(
                                null, userId, uuid.generate(),
                                quagoMetaData, new ResolutionDataPoint(lastResolutionData)
                        )));
    }

    public void endTracking()
    {
        if (!initialized)
        {
            LOG.W("endTracking", "Method called before SDK initialized!");
            return;
        }
        handler.SendEmptyMessage((int)COMMAND.TRACKING_END);
    }

    public void setKeyValues(string key, string value)
    {
        if (!initialized)
        {
            LOG.W("setKeyValues", "Method called before SDK initialized!");
            return;
        }
        handler.SendMessage(new Message((int)COMMAND.SET_KEY_VALUE, 0, 0, new string[] { key, value }));
    }

    public void setUserId(string userId)
    {
        if (!initialized)
        {
            LOG.W("setUserId", "Method called before SDK initialized!");
            return;
        }
        handler.SendMessage(new Message((int)COMMAND.SET_USER_ID, 0, 0, userId));
    }

    public void setAdditionalId(string additionalId)
    {
        if (!initialized)
        {
            LOG.W("setAdditionalId", "Method called before SDK initialized!");
            return;
        }
        handler.SendMessage(new Message((int)COMMAND.SET_ADDITIONAL_ID, 0, 0, additionalId));
    }

    public void dispatchKey(long timestamp, int type, int keyCode)
    {
        if (!initialized) return;
        dataManager.dispatchKey(timestamp, type, keyCode);
    }

    public void dispatchTouch(long timestamp, BiometricType type, float x, float y, int button)
    {
        if (!initialized) return;
        dataManager.dispatchTouch(timestamp, type, x, y, button);
    }

    public void onResume()
    {
        if (!initialized)
        {
            LOG.W("onResume", "Method called before SDK initialized!");
            return;
        }
        LOG.I("onResume", "Called");

        /* Save State */
        dataManager.onLifeCycleChanged(new LifeCycleDataPoint(BiometricType.ON_RESUME));
        checkResolutionChange();
    }

    public void onPause()
    {
        if (!initialized)
        {
            LOG.W("onPause", "Method called before SDK initialized!");
            return;
        }
        LOG.I("onPause", "Called");

        /* Save State */
        dataManager.onLifeCycleChanged(new LifeCycleDataPoint(BiometricType.ON_PAUSE));
    }

    public void onStop()
    {
        if (!initialized)
        {
            LOG.W("onStop", "Method called before SDK initialized!");
            return;
        }
        LOG.I("onStop", "Called");

        /* Save State */
        dataManager.onLifeCycleChanged(new LifeCycleDataPoint(BiometricType.ON_STOP));
    }

    protected void checkResolutionChange()
    {
        if (!initialized) return;
        long timestamp = QuagoStandalone.ElapsedMilliseconds();

        /* Screen */
        Resolution screenResolution = Screen.currentResolution;

        /* Window */
        int appScreenWidth = Screen.width;
        int appScreenHeight = Screen.height;

        Vector2Int windowPosition = Screen.mainWindowPosition;

        ResolutionDataPoint currResolution = new(
            timestamp,
            screenResolution.width, screenResolution.height,
            appScreenWidth, appScreenHeight,
            windowPosition.x, windowPosition.y,
            screenResolution.refreshRate,
            Screen.dpi, Screen.fullScreen
        );

        LOG.D("checkResolutionChange", currResolution.toString());

        if (currResolution.isSimilar(lastResolutionData)) return;
        lastResolutionData = currResolution;

        /* Dispatch resolution change */
        dataManager.onResolutionChange(currResolution);
    }

    /*.......................................Handler.Methods......................................*/

    protected class QuagoManagerHandler : HandlerThread.Handler
    {
        internal Logger LOG;
        internal QuagoManager manager;

        public QuagoManagerHandler(QuagoManager QuagoManager)
        {
            LOG = new Logger(GetType().Name);
            this.manager = QuagoManager;
        }

        public override void HandleMessage(Message msg)
        {
            try
            {
                if (manager == null || !manager.initialized) return;
                if (manager.settings == null)
                {
                    LOG.E("Handler", "Settings is null");
                    return;
                }
                switch ((COMMAND)msg.What)
                {
                    case COMMAND.INIT:
                        LOG.I("Handler", "Quago SDK Initialized");
                        LOG.I("Handler", "SDK Version = {0}, AppToken = {1}, ",
                                Quago.VERSION_NAME, manager.settings.getAppToken()
                        );
                        if (manager.quagoMetaData == null)
                        {
                            LOG.E("Handler", "QuagoMetaData failed to initialize!");
                            break;
                        }
                        break;
                    case COMMAND.ON_RESUME:
                    case COMMAND.ON_PAUSE:
                    case COMMAND.ON_STOP:
                        break;
                    case COMMAND.SEGMENT_INIT_BEGIN:
                    /* ............................ Manual Mode ............................. */
                    case COMMAND.SEGMENT_BEGIN:
                        if (manager.maxSegmentsReached)
                        {
                            LOG.I("Handler", "MaxSegment of {0} reached!",
                                    manager.settings.getMaxSegments()
                            );
                            break;
                        }

                        /* Get the new segment */
                        QuagoSegment nextSegment = (QuagoSegment)msg.Obj;
                        long timeEnd = QuagoStandalone.CurrentTimestampMilliseconds();
                        long uptimeEnd = QuagoStandalone.ElapsedMilliseconds();
                        manager.segmentCounter++;

                        /* MaxSegment condition */
                        if (manager.settings.getMaxSegments() > -1)
                        {
                            /* condition reached, end segment and disable beginSegment */
                            if (manager.settings.getMaxSegments() < manager.segmentCounter)
                            {
                                /* End & send the current segment */
                                if (manager.currSegment != null)
                                {
                                    LOG.I("Handler", "Ending last segment");
                                    handleSegmentChange(
                                            manager.currSegment,
                                            QuagoStandalone.CurrentTimestampMilliseconds(), QuagoStandalone.ElapsedMilliseconds(),
                                            Commons.PROMPT_DEVELOPER_MAX_SEGMENTS_REACHED, Commons.MODE_MANUAL
                                    );
                                    manager.currSegment = null;
                                }
                                LOG.I("Handler", "MaxSegment of {0} reached!",
                                        manager.settings.getMaxSegments()
                                );
                                manager.maxSegmentsReached = true;
                                break;
                            }

                            LOG.I("Handler", "SegmentBegin Name = {0} [{1}/{2}]",
                                    nextSegment.getName(),
                                    manager.segmentCounter,
                                    manager.settings.getMaxSegments()
                            );
                        }
                        else
                        {
                            LOG.I("Handler", "SegmentBegin Name = {0}",
                                    nextSegment.getName()
                            );
                        }

                        /* End & send current active segment */
                        if (manager.currSegment != null)
                            handleSegmentChange(
                                    manager.currSegment,
                                    timeEnd, uptimeEnd,
                                    Commons.PROMPT_DEVELOPER, Commons.MODE_MANUAL
                            );

                        /* Give the next segment a starting time and set it's counter */
                        nextSegment.setStartingTimes(timeEnd, uptimeEnd);
                        nextSegment.segmentCounter = manager.segmentCounter;
                        manager.currSegment = nextSegment;
                        break;

                    case COMMAND.SEGMENT_END:
                        if (manager.currSegment == null)
                        {
                            LOG.I("Handler", "EndSegment called without an active segment.");
                            break;
                        }
                        int endPrompt;
                        if (manager.settings.getMaxSegments() > -1 &&
                                manager.settings.getMaxSegments() <= manager.currSegment.segmentCounter)
                        {
                            LOG.I("Handler", "Ending last segment");
                            endPrompt = Commons.PROMPT_DEVELOPER_MAX_SEGMENTS_REACHED;
                        }
                        else
                        {
                            LOG.I("Handler", "Segment ended.");
                            endPrompt = Commons.PROMPT_DEVELOPER;
                        }
                        /* Handle current segment and it's ending times */
                        handleSegmentChange(
                                manager.currSegment,
                                QuagoStandalone.CurrentTimestampMilliseconds(), QuagoStandalone.ElapsedMilliseconds(),
                                endPrompt, Commons.MODE_MANUAL
                        );
                        manager.currSegment = null;
                        break;
                    /* ........................... Tracking Mode ............................ */
                    case COMMAND.TRACKING_BEGIN:
                        if (manager.maxTrackingSegmentsReached)
                        {
                            LOG.I("Handler", "MaxTrackingSegment of {0} reached!",
                                    manager.settings.getMaxSegments()
                            );
                            break;
                        }
                        if (manager.trackingCurrSegment != null)
                        {
                            LOG.W("Handler", "beginTracking() was called while already tracking, ignoring call!");
                            break;
                        }

                        /* Get the new segment */
                        QuagoSegment trackingSegment = (QuagoSegment)msg.Obj;
                        long trackingTimeEnd = QuagoStandalone.CurrentTimestampMilliseconds();
                        long trackingUptimeEnd = QuagoStandalone.ElapsedMilliseconds();
                        manager.segmentCounter++;

                        /* MaxSegment condition */
                        if (manager.settings.getMaxSegments() > -1)
                        {
                            /* condition reached, end segment and disable beginSegment */
                            if (manager.settings.getMaxSegments() < manager.segmentCounter)
                            {
                                /* End & send the current segment */
                                if (manager.trackingCurrSegment != null)
                                {
                                    LOG.I("Handler", "Ending Tracking last segment");
                                    handleSegmentChange(
                                            manager.trackingCurrSegment,
                                            QuagoStandalone.CurrentTimestampMilliseconds(), QuagoStandalone.ElapsedMilliseconds(),
                                            Commons.PROMPT_DEVELOPER_MAX_SEGMENTS_REACHED, Commons.MODE_AUTO
                                    );
                                    manager.trackingCurrSegment = null;
                                }
                                LOG.I("Handler", "MaxTrackingSegment of {0} reached!",
                                        manager.settings.getMaxSegments()
                                );
                                manager.maxTrackingSegmentsReached = true;
                                break;
                            }

                            if (trackingSegment.userId == null)
                                LOG.I("Handler", "Begin Tracking [{0}/{1}]",
                                        manager.segmentCounter,
                                        manager.settings.getMaxSegments()
                                );
                            else
                                LOG.I("Handler", "Begin Tracking userId = {0} [{1}/{2}]",
                                        trackingSegment.userId,
                                        manager.segmentCounter,
                                        manager.settings.getMaxSegments()
                                );
                        }
                        else
                        {
                            if (trackingSegment.userId == null)
                                LOG.I("Handler", "Begin Tracking");
                            else
                                LOG.I("Handler", "Begin Tracking userId = {0}",
                                        trackingSegment.userId
                                );
                        }

                        /* Give the next segment a starting time and set it's counter */
                        trackingSegment.setStartingTimes(trackingTimeEnd, trackingUptimeEnd);
                        trackingSegment.segmentCounter = manager.segmentCounter;
                        manager.trackingCurrSegment = trackingSegment;

                        /* Start the periodic check for sendoff condition */
                        SendMessageDelayed(
                                Message.Obtain(this, (int)COMMAND.TRACKING_CHECK),
                                manager.settings.getAutoMotionIntervalMillis()
                        );
                        break;

                    case COMMAND.TRACKING_CHECK:
                        LOG.V("Handler", "TRACKING_CHECK");
                        if (manager.trackingCurrSegment == null) break;
                        long trackingCheckTimeEnd = QuagoStandalone.CurrentTimestampMilliseconds();
                        long trackingCheckUptimeEnd = QuagoStandalone.ElapsedMilliseconds();

                        /* Check sendoff condition */
                        int trackingPrompt = 0;
                        if (trackingCheckUptimeEnd - manager.trackingCurrSegment.uptimeStart
                                >= manager.settings.getAutoMaxDurationMillis())
                        {
                            trackingPrompt = Commons.PROMPT_TRACKING_DURATION;
                            LOG.D("Handler", "Tracking Condition met = Duration");
                        }
                        else
                        {
                            if (QueryManager.countTouches(
                                    manager.dataManager.arrMotion,
                                    manager.trackingCurrSegment.uptimeStart,
                                    trackingCheckUptimeEnd,
                                    manager.settings.getAutoMotionAmount())
                                    >= manager.settings.getAutoMotionAmount())
                            {
                                trackingPrompt = Commons.PROMPT_TRACKING_AMOUNT;
                                LOG.D("Handler", "Tracking Condition met = Amount");
                            }
                        }

                        if (trackingPrompt != 0)
                        {
                            /* Conditions met, prepare the new Segment */
                            manager.segmentCounter++;

                            QuagoSegment trackingCheckSegment = new QuagoSegment(
                                    manager.trackingCurrSegment.name,
                                    manager.trackingCurrSegment.userId,
                                    manager.uuid.generate(),
                                    manager.quagoMetaData,
                                    new ResolutionDataPoint(manager.lastResolutionData)
                            );

                            trackingCheckSegment.setStartingTimes(trackingCheckTimeEnd, trackingCheckUptimeEnd);
                            trackingCheckSegment.segmentCounter = manager.segmentCounter;

                            /* Sendoff the old Segment */
                            handleSegmentChange(
                                    manager.trackingCurrSegment,
                                    trackingCheckTimeEnd, trackingCheckUptimeEnd,
                                    trackingPrompt, Commons.MODE_AUTO
                            );

                            /* Update the current tracking segment with the new segment */
                            manager.trackingCurrSegment = trackingCheckSegment;
                        }

                        SendMessageDelayed(
                                Message.Obtain(this, (int)COMMAND.TRACKING_CHECK),
                                manager.settings.getAutoMotionIntervalMillis()
                        );
                        break;

                    case COMMAND.TRACKING_END:
                        if (manager.trackingCurrSegment == null)
                        {
                            LOG.W("Handler", "End Tracking called without an active tracking segment.");
                            break;
                        }
                        RemoveMessages((int)COMMAND.TRACKING_CHECK);

                        int trackingEndPrompt;
                        if (manager.settings.getMaxSegments() > -1 &&
                                manager.settings.getMaxSegments() <= manager.trackingCurrSegment.segmentCounter)
                        {
                            LOG.I("Handler", "Ending last tracking segment");
                            trackingEndPrompt = Commons.PROMPT_DEVELOPER_MAX_SEGMENTS_REACHED;
                        }
                        else
                        {
                            LOG.I("Handler", "Segment tracking ended.");
                            trackingEndPrompt = Commons.PROMPT_DEVELOPER;
                        }
                        /* Handle current segment and it's ending times */
                        handleSegmentChange(
                                manager.trackingCurrSegment,
                                QuagoStandalone.CurrentTimestampMilliseconds(), QuagoStandalone.ElapsedMilliseconds(),
                                trackingEndPrompt, Commons.MODE_AUTO
                        );
                        manager.trackingCurrSegment = null;
                        break;

                    case COMMAND.SET_KEY_VALUE:
                        LOG.D("Handler", "SET_KEY_VALUE");
                        bool keyValuesCurrSegment = manager.currSegment != null;
                        bool keyValuesCurrTrackingSegment = manager.trackingCurrSegment != null;
                        if (!keyValuesCurrSegment && !keyValuesCurrTrackingSegment)
                        {
                            LOG.E("Handler", "setKeyValue() ignored due to not having an active Segment.");
                            break;
                        }
                        string[] keyValues = (string[])msg.Obj;
                        if (keyValuesCurrSegment)
                        {
                            manager.currSegment.setKeyValue(keyValues[0], keyValues[1]);
                            LOG.I("Handler", "KeyValues for Segment {0} = [{1},{2}]",
                                    manager.currSegment.getName(),
                                    keyValues[0],
                                    keyValues[1]
                            );
                        }
                        if (keyValuesCurrTrackingSegment)
                        {
                            manager.trackingCurrSegment.setKeyValue(keyValues[0], keyValues[1]);
                            LOG.I("Handler", "KeyValues for Tracking Segment = [{0},{1}]",
                                    keyValues[0],
                                    keyValues[1]
                            );
                        }
                        break;

                    case COMMAND.RELEASE:
                        LOG.D("Handler", "ON_RELEASE");
                        manager.initialized = false;
                        RemoveAllMessages();
                        break;

                    case COMMAND.SET_USER_ID:
                        LOG.D("Handler", "SET_USER_ID");
                        bool userIdCurrSegment = manager.currSegment != null;
                        bool userIdCurrTrackingSegment = manager.trackingCurrSegment != null;
                        if (!userIdCurrSegment && !userIdCurrTrackingSegment)
                        {
                            LOG.E("Handler", "setUserId() ignored due to not having an active Segment.");
                            break;
                        }
                        if (!(msg.Obj is string))
                        {
                            LOG.E("Handler", "userId object must be String");
                            break;
                        }
                        string strUserId = (string)msg.Obj;
                        if (userIdCurrSegment)
                        {
                            manager.currSegment.userId = strUserId;
                            LOG.I("Handler", "UserId for Segment {0} = {1}",
                                    manager.currSegment.getName(),
                                    manager.currSegment.userId
                            );
                        }
                        if (userIdCurrTrackingSegment)
                        {
                            LOG.I("Handler", "Set UserId for Tracking Segment = {0}", strUserId);
                            manager.beginTracking(strUserId);
                        }
                        if (userIdCurrTrackingSegment)
                        {
                            if (manager.trackingCurrSegment != null)
                            {
                                manager.trackingCurrSegment.userId = strUserId;
                                LOG.I("Handler", "Set UserId for Tracking Segment = {0}", strUserId);
                            }
                            else
                            {
                                LOG.E("Handler", "UserId was already set, end then begin Tracking with the new UserId.");
                            }
                        }
                        break;

                    case COMMAND.SET_ADDITIONAL_ID:
                        LOG.D("Handler", "SET_ADDITIONAL_ID");
                        bool additionalCurrSegment = manager.currSegment != null;
                        bool additionalCurrTrackingSegment = manager.trackingCurrSegment != null;
                        if (!additionalCurrSegment && !additionalCurrTrackingSegment)
                        {
                            LOG.E("Handler", "setAdditionalId() ignored due to not having an active Segment.");
                            break;
                        }
                        if (!(msg.Obj is string))
                        {
                            LOG.E("Handler", "AdditionalId object must be String");
                            break;
                        }
                        string strAdditionalId = (string)msg.Obj;
                        if (additionalCurrSegment)
                        {
                            manager.currSegment.additionalId = strAdditionalId;
                            LOG.I("Handler", "AdditionalId for Segment {0} = {1}",
                                    manager.currSegment.getName(),
                                    manager.currSegment.additionalId
                            );
                        }
                        if (additionalCurrTrackingSegment)
                        {
                            manager.trackingCurrSegment.additionalId = strAdditionalId;
                            LOG.I("Handler", "AdditionalId for Tracking Segment = {0}",
                                    manager.trackingCurrSegment.additionalId
                            );
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                LOG.E("Handler", e);
            }
        }

        protected void handleSegmentChange(QuagoSegment segment,
                                           long endTime, long endBootTime,
                                           int prompt, int mode)
        {
            //            long start = QuagoStandalone.ElapsedMilliseconds();
            segment.setEndingTimes(endTime, endBootTime);

            /* Query the data of the current segment */
            DataSegment data = manager.dataManager.query(
                    segment,
                    segment.getUptimeStart(),
                    segment.getUptimeEnd()
            );

            /* Season the segment with flavour */
            data.app_token = manager.settings.getAppToken();
            data.session_id = manager.sessionId;
            data.app_package_name = manager.quagoMetaData.unity.application.identifier;
            data.app_version_name = manager.quagoMetaData.unity.application.version;
            data.sdk_version_name = Quago.VERSION_NAME;
            data.wrapper = manager.settings.getWrapper();
            data.wrapper_version = manager.settings.getWrapperVersion();
            data.prompt = prompt;
            data.mode = mode;
            data.flavor = (int)manager.settings.getFlavor();

            data.seg_name = segment.name;
            data.seg_id = segment.segmentId;
            data.user_id = segment.userId;
            data.additional_id = segment.additionalId;
            data.seg_count = segment.segmentCounter;

            data.key_values = new();
            foreach (string key in segment.mapKeyValues.Keys)
                data.key_values.Add(new string[] { key, segment.mapKeyValues[key] });

            data.key_bindings = QuagoStandalone.KeyMapToDictionary();

            data.times = new SegmentTimes(
                    segment.timeStart,
                    segment.timeEnd,
                    segment.uptimeStart,
                    segment.uptimeEnd,
                    manager.sdkInitTime,
                    manager.sdkInitUptime
            );

            if (segment.resolutionData != null)
            {
                data.resolution_unity_desktop = new(segment.resolutionData);
                data.screen_width = segment.resolutionData.screenWidth;
                data.screen_height = segment.resolutionData.screenHeight;
                data.app_width = segment.resolutionData.appWidth;
                data.app_height = segment.resolutionData.appHeight;
            }

            /* Send to Callback owner */
            if (manager.settings.getCallback() != null)
            {
                try
                {
                    Dictionary<string, object> headers = new();
                    headers.Add("app_token", data.app_token);
                    headers.Add("platform", DataSegment.platform);
                    headers.Add("seg_name", data.seg_name);
                    headers.Add("seg_count", data.seg_count);
                    headers.Add("user_id", data.user_id);
                    headers.Add("additional_id", data.additional_id);
                    headers.Add("sdk_version_name", data.sdk_version_name);
                    headers.Add("mode", data.mode);

                    /* Sendoff Payload to Callback */
                    manager.settings.getCallback().onJsonSegment(
                            manager.serializer.ToJSON(headers),
                            manager.serializer.ToJSON(data.ToDictionary())
                    );
                }
                catch (Exception exception)
                {
                    LOG.E("handleSegmentChange_json_sendoff", exception);
                }
                return;
            }

            /* Send to Quago Servers */
            try
            {
                if (manager.quagoNetworkModule == null)
                {
                    LOG.E("handleSegmentChange_server_sendoff", "quagoNetworkModule is null");
                    return;
                }

                /* Prepare Payload */
                Dictionary<String, String> payload = new();
                payload.Add("app_token", data.app_token);
                payload.Add("env",
                        data.flavor == (int)QuagoSettings.QuagoFlavor.PRODUCTION ? "prod" : "stage"
                );
                payload.Add("sdk_version", data.sdk_version_name);
                payload.Add("payload",
                    Convert.ToBase64String(
                        Commons.Gzip(
                            Encoding.UTF8.GetBytes(
                                manager.serializer.ToJSON(data.ToDictionary())
                                )
                            )
                        )
                );

                /* Sendoff Payload to Server */
                manager.quagoNetworkModule.SendPayload(
                        manager.serializer.ToJSON(payload)
                );
            }
            catch (Exception e)
            {
                LOG.E("handleSegmentChange_server_sendoff", e);
            }
        }
    }
    ////////
}
#endif