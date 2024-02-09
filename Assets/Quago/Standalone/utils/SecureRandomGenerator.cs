/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Security.Cryptography;

/// <summary>
/// Secure random generator
/// </summary>
public class SecureRandomGenerator : IDisposable
{
    private readonly RNGCryptoServiceProvider csp;
    /// <summary>
    /// Constructor
    /// </summary>
    public SecureRandomGenerator()
    {
        csp = new RNGCryptoServiceProvider();
    }

    /// <summary>
    /// Get random value
    /// </summary>
    /// <param name="minValue"></param>
    /// <param name="maxExclusiveValue"></param>
    /// <returns></returns>
    public int Next(int minValue, int maxExclusiveValue)
    {
        if (minValue == maxExclusiveValue) return minValue;

        if (minValue > maxExclusiveValue)
            throw new ArgumentOutOfRangeException($"{nameof(minValue)} must be lower than {nameof(maxExclusiveValue)}");

        var diff = (long)maxExclusiveValue - minValue;
        var upperBound = uint.MaxValue / diff * diff;

        uint ui;
        do
        {
            ui = GetRandomUInt();
        } while (ui >= upperBound);
        return (int)(minValue + (ui % diff));
    }

    private uint GetRandomUInt()
    {
        var randomBytes = GenerateRandomBytes(sizeof(uint));
        return BitConverter.ToUInt32(randomBytes, 0);
    }

    private byte[] GenerateRandomBytes(int bytesNumber)
    {
        var buffer = new byte[bytesNumber];
        csp.GetBytes(buffer);
        return buffer;
    }

    private bool _disposed;

    /// <summary>
    /// Public implementation of Dispose pattern callable by consumers.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        // Dispose managed state (managed objects).
        if (disposing) csp?.Dispose();

        _disposed = true;
    }
}
#endif