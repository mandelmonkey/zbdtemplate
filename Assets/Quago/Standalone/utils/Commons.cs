/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEngine;

public class Commons
{
    public static readonly int PROMPT_DEVELOPER = 1;
    public static readonly int PROMPT_DEVELOPER_MAX_SEGMENTS_REACHED = 2;
    public static readonly int PROMPT_TRACKING_AMOUNT = 3;
    public static readonly int PROMPT_TRACKING_DURATION = 4;

    public static readonly int MODE_AUTO = 1;
    public static readonly int MODE_MANUAL = 2;

    public static byte[] Gzip(byte[] data)
    {
        try
        {
            using (MemoryStream bos = new MemoryStream())
            {
                using (GZipStream outStream = new GZipStream(bos, CompressionMode.Compress))
                {
                    outStream.Write(data, 0, data.Length);
                }
                return bos.ToArray();
            }
        }
        catch (IOException e)
        {
            throw new Exception(e.Message);
        }
    }

    public static byte[] Gunzip(byte[] data)
    {
        try
        {
            using (MemoryStream bos = new MemoryStream())
            {
                using (MemoryStream bis = new MemoryStream(data))
                {
                    using (GZipStream inStream = new GZipStream(bis, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[1024];
                        int len = 0;
                        while ((len = inStream.Read(buffer, 0, buffer.Length)) >= 0)
                        {
                            bos.Write(buffer, 0, len);
                        }
                    }
                }
                return bos.ToArray();
            }
        }
        catch (IOException e)
        {
            throw new Exception(e.Message);
        }
    }

    /**
     * Turns an array of booleans to a long using binary form, Used for combining flags together.
     * <p>
     * examples:
     * <ul>
     * <li>0 = getFlags(false, false);</li>
     * <li>1 = getFlags(false, true);</li>
     * <li>2 = getFlags(true, false);</li>
     * <li>3 = getFlags(true, true);</li>
     * </ul>
     * </p>
     *
     * @param values
     * @return
     */
    public static long CombineValues(params bool[] values)
    {
        long n = 0;
        foreach (bool value in values)
        {
            n = (n << 1) + (value ? 1 : 0);
        }
        return n;
    }

    /**
     * Generate a random number between min and max inclusive.
     *
     * @param min must be smaller then max.
     * @param max must be bigger then min.
     * @return an int between [min,max]
     */
    public static int RandomBetween(int min, int max)
    {
        if (min > max)
        {
            int temp = max;
            max = min;
            min = temp;
        }
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            byte[] buffer = new byte[4];
            rng.GetBytes(buffer);
            int randomNumber = BitConverter.ToInt32(buffer, 0);
            return Mathf.Abs(randomNumber % (max - min + 1)) + min;
        }
    }
}
#endif