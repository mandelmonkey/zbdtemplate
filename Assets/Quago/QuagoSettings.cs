using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuagoSettings 
{
    // protected static QuagoLogger.Logger LOG = QuagoLogger.create();

    protected QuagoFlavor flavor;
    protected LogLevel logLevel = LogLevel.INFO;
    protected string appToken;
    protected QuagoLogger.OnLog loggerCallback;
    protected QuagoCallback callback;
    protected bool enableManualMotionDispatcher, enableManualKeysDispatcher;
    /* UNITY = 1 */
    protected bool disableInitSegment;
    protected int wrapper = 1;
    protected string wrapperVersion = Quago.VERSION_NAME;
    protected int maxSegments = -1;
    protected int autoMotionAmount = 100;
    protected long autoMaxDurationMillis = 5*60*1000;//5min
    protected long autoMotionIntervalMillis = 30*1000;//30sec

    protected QuagoSettings(string appToken, QuagoFlavor flavor) {
        this.appToken = appToken;
        this.flavor = flavor;
    }

    /**
     * Used to build a {@link QuagoSettings.Builder} instance.
     * Mandatory to call this method using a valid appToken for your application.
     *
     * @param appToken
     * @return
     */
    public static QuagoSettings.Builder create(string appToken,
                                                QuagoFlavor flavor) {
        return new QuagoSettings.Builder(appToken, flavor);
    }

    public bool isManualMotionDispatcherEnabled() {
        return enableManualMotionDispatcher;
    }

    public bool isManualKeysDispatcherEnabled() {
        return enableManualKeysDispatcher;
    }

    public bool isInitSegmentDisabled() {
        return disableInitSegment;
    }

    public string getAppToken() {
        return appToken;
    }

    public QuagoLogger.OnLog GetLogger() {
        return loggerCallback;
    }

    public QuagoCallback getCallback() {
        return callback;
    }

    public int getWrapper() {
        return wrapper;
    }

    public string getWrapperVersion() {
        return wrapperVersion;
    }

    public QuagoFlavor getFlavor() {
        return flavor;
    }

    public int getMaxSegments() {
        return maxSegments;
    }

    public LogLevel getLogLevel() {
        return logLevel;
    }

    public int getAutoMotionAmount() {
        return autoMotionAmount;
    }

    public long getAutoMaxDurationMillis() {
        return autoMaxDurationMillis;
    }

    public long getAutoMotionIntervalMillis() {
        return autoMotionIntervalMillis;
    }

    /*.......................................Public.Classes.......................................*/

    public class Builder {
        protected QuagoSettings settings;

        public Builder( string appToken, QuagoFlavor flavor) {
            if (string.IsNullOrEmpty(appToken))
                throw new System.Exception("appToken can't be null or empty!");
            settings = new QuagoSettings(appToken, flavor);
        }

        public QuagoSettings.Builder setJsonCallback(QuagoCallback.OnJsonSegment callback) {
            if (callback == null) {
                return this;
            }
            settings.callback = new QuagoCallback(callback);
            return this;
        }

        public QuagoSettings.Builder setLogLevel(LogLevel logLevel) {
            settings.logLevel = logLevel;
            return this;
        }

        /**
         * When set, {@link QuagoLogger} won't print logs into the logcat, instead it will pass
         * them to the given callback.
         * <p>
         * By default the callback is null.
         * Setting it to null will reset it to print logs into logcat instead.
         *
         * @param callback the callback to receive logs.
         * @return
         */
        public QuagoSettings.Builder overrideLogger(QuagoLogger.OnLog callback) {
            settings.loggerCallback = callback;
            return this;
        }

        public QuagoSettings.Builder enableManualTouchDispatcher() {
            settings.enableManualMotionDispatcher = true;
            return this;
        }

        public QuagoSettings.Builder enableManualKeysDispatcher() {
            settings.enableManualKeysDispatcher = true;
            return this;
        }

        public QuagoSettings.Builder setMaxSegments(int maxSegments) {
            settings.maxSegments = maxSegments;
            return this;
        }

        public QuagoSettings.Builder setTrackingMotionAmount(int autoMotionAmount) {
            settings.autoMotionAmount = autoMotionAmount;
            return this;
        }

        public QuagoSettings.Builder setTrackingInterval(long intervalMillis) {
            settings.autoMotionIntervalMillis = intervalMillis;
            return this;
        }

        public QuagoSettings.Builder setTrackingMaxDuration(long durationMillis) {
            settings.autoMaxDurationMillis = durationMillis;
            return this;
        }

        public QuagoSettings.Builder disableInitSegment() {
            settings.disableInitSegment = true;
            return this;
        }

        /**
         * Important: For internal use only, don't call this method.
         * Set's the information of the possible wrapper running this SDK.
         *
         * @param wrapper
         * @param version
         * @return
         */
        public QuagoSettings.Builder setWrapperInfo( int wrapper,  string version) {
            settings.wrapper = wrapper;
            settings.wrapperVersion = version;
            return this;
        }

        public QuagoSettings build() {
            return settings;
        }
    }

    /**
     * Enum for controlling {@link com.quago.mobile.sdk.utils.QuagoLogger} log levels.
     * Using only the most used priorities.
     */
    public enum LogLevel {
        /* Enable all logs */
        VERBOSE = 2,
        /* Enable debug logs */
        DEBUG = 3,
        /* Default - used for integration */
        INFO = 4,
        /* Show only Error logs */
        WARNING = 5,
        /* Show only Error logs */
        ERROR = 6,
        /* Disable all logs */
        DISABLED = 10
    }

    public enum QuagoFlavor {
        AUTHENTIC = 0, UNAUTHENTIC = 1, PRODUCTION = 2, DEVELOPMENT = 3
    }
}
