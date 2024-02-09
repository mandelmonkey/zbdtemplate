/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;

public class QuagoNetworkSettings
{
    public readonly string Url;
    public readonly long InitialIntervalMillis, MaximumBackoffMillis, DeadlineMillis;
    public readonly float Multiplier, RandomizationFactor, MinFactor, MaxFactor;
    public readonly int MaxCachedMessages;

    public static Builder BuildNetConfig()
    {
        return new Builder();
    }

    public float GetRandomFromRange()
    {
        return MinFactor + (float)new Random().NextDouble() * (MaxFactor - MinFactor);
    }

    private QuagoNetworkSettings(Builder builder)
    {
        Url = builder.Url;
        Multiplier = builder.Multiplier;
        DeadlineMillis = builder.DeadlineMillis;
        MaxCachedMessages = builder.MaxCachedMessages;
        MaximumBackoffMillis = builder.MaximumBackoffMillis;
        InitialIntervalMillis = builder.InitialIntervalMillis;
        RandomizationFactor = builder.RandomizationFactor;
        MinFactor = 1f - RandomizationFactor;
        MaxFactor = 1f + RandomizationFactor;
    }

    public class Builder
    {
        protected internal string Url;
        protected internal long InitialIntervalMillis, MaximumBackoffMillis, DeadlineMillis;
        protected internal float Multiplier, RandomizationFactor;
        protected internal int MaxCachedMessages;

        public Builder()
        {
            InitialIntervalMillis = (long)TimeSpan.FromSeconds(1).TotalMilliseconds;
            MaximumBackoffMillis = (long)TimeSpan.FromMinutes(10).TotalMilliseconds;
            DeadlineMillis = (long)TimeSpan.FromMinutes(90).TotalMilliseconds;
            Multiplier = 1.5f;
            RandomizationFactor = 0.4f;
            MaxCachedMessages = 10;
        }

        public Builder SetUrl(string url)
        {
            Url = url;
            return this;
        }

        public Builder SetInitialIntervalMillis(long initialIntervalMillis)
        {
            InitialIntervalMillis = initialIntervalMillis;
            return this;
        }

        public Builder SetMaximumBackoffMillis(long maximumBackoffMillis)
        {
            MaximumBackoffMillis = maximumBackoffMillis;
            return this;
        }

        public Builder SetDeadlineMillis(long deadlineMillis)
        {
            DeadlineMillis = deadlineMillis;
            return this;
        }

        public Builder SetMultiplier(float multiplier)
        {
            Multiplier = multiplier;
            return this;
        }

        public Builder SetRandomizationFactor(float randomizationFactor)
        {
            RandomizationFactor = randomizationFactor;
            return this;
        }

        public Builder SetMaxCachedMessages(int maxCachedMessages)
        {
            MaxCachedMessages = maxCachedMessages;
            return this;
        }

        public QuagoNetworkSettings Build()
        {
            return new QuagoNetworkSettings(this);
        }
    }
}
#endif