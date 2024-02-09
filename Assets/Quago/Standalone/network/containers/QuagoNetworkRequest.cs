/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Collections.Generic;

public abstract class QuagoNetworkRequest
{
    protected Dictionary<string, string> headers;
    public readonly string Url;
    public readonly QuagoNetworkSettings Config;
    private long beginTimestamp;
    private long randomizedInterval, retryInterval;
    private long retries;
    public readonly string RequestMethod;
    public readonly bool UseCaches, DoInput, DoOutput;

    public QuagoNetworkRequest(QuagoNetworkSettings config, string requestMethod,
                               bool useCaches, bool doInput, bool doOutput)
    {
        this.retryInterval = config.InitialIntervalMillis;
        this.RequestMethod = requestMethod;
        this.headers = new Dictionary<string, string>();
        this.UseCaches = useCaches;
        this.DoOutput = doOutput;
        this.DoInput = doInput;
        this.Url = config.Url;
        this.Config = config;
    }

    public void SetHeader(string key, string value)
    {
        headers.Add(key, value);
    }

    /**
     * Calculates the RandomizedInterval to be used for the current retry.
     * Starts the timer, increments the retry counter and prepares the fields for the next iteration.
     * <p>
     * Call this method first then call {@link #getRandomizedInterval} to get the updated interval.
     */
    public void PrepareRetry()
    {
        float randomRange = Config.GetRandomFromRange();
        randomizedInterval = (long)(retryInterval * randomRange);
        retries++;
        retryInterval = Math.Min(Config.MaximumBackoffMillis, (long)(retryInterval * Config.Multiplier));
    }

    /**
     * Start the timer for the first retry or after a reset.
     */
    public void StartTimer()
    {
        beginTimestamp = QuagoStandalone.ElapsedMilliseconds();
    }

    /**
     * @return the duration in Millis to wait until a retry.
     */
    public long GetRandomizedInterval()
    {
        return randomizedInterval;
    }

    /**
     * @return the current retry number.
     */
    public long GetRetryNumber()
    {
        return retries;
    }

    /**
     * Check if the timer has reached the deadline, if it did then this message should be dropped.
     *
     * @return true if past deadline.
     */
    public bool IsPastDeadline()
    {
        long now = QuagoStandalone.ElapsedMilliseconds();
        return now - beginTimestamp >= Config.DeadlineMillis;
    }

    public void Reset()
    {
        beginTimestamp = QuagoStandalone.ElapsedMilliseconds();
        randomizedInterval = 0;
        retryInterval = Config.InitialIntervalMillis;
    }

    public Dictionary<string, string> GetHeaders()
    {
        return headers;
    }

    public virtual void OnSuccess(int code)
    {
    }

    public virtual void OnFailure(int code)
    {
    }

    public virtual void OnException(Exception e)
    {
    }

    public abstract string GenerateBody();
}
#endif