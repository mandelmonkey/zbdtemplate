/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Security.Cryptography;

public class QuagoUUID
{
    public static string DIGITS = "0123456789";
    public static string UPPER_CASE = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public static string LOWER_CASE = UPPER_CASE.ToLower();
    public static string ALPHANUMERIC = UPPER_CASE + LOWER_CASE + DIGITS;
    public static int DEFAULT_SIZE = 36;

    private char[] allowedCharacters, charBuffer;
    private SecureRandomGenerator seed;

    /**
     * Alphanumeric String generator.
     * Uses a new {@link SecureRandom} as seed.
     * Uses the default size of 36 characters.
     */
    public QuagoUUID() : this(DEFAULT_SIZE, new SecureRandomGenerator(), ALPHANUMERIC){}

    /**
     * Alphanumeric String generator.
     * Uses a new {@link SecureRandom} as seed.
     *
     * @param size size of the generated String
     */
    public QuagoUUID(int size) : this(size, new SecureRandomGenerator(), ALPHANUMERIC)
    { }

    /**
     * Alphanumeric String generator.
     *
     * @param size              the size of any generated String by this instance and it must be greater then zero.
     * @param seed              a common seed of SecureRandom.
     * @param allowedCharacters which characters are allowed to be used to generate the strings,
     *                          create your own sequence or use any from the above:
     *                          - {@link #DIGITS} - only digits: 0-9
     *                          - {@link #UPPER_CASE} - only upper case letters: A-Z
     *                          - {@link #LOWER_CASE} - only lower case letters: a-z
     *                          - {@link #ALPHANUMERIC} - all of the above: 0-9A-Za-z
     * @throws IllegalArgumentException when {@code size} length < 1.
     * @throws IllegalArgumentException when {@code allowedCharacters} length < 2.
     * @throws NullPointerException     when {@code seed} is null.
     */
    public QuagoUUID(int size, SecureRandomGenerator seed, string allowedCharacters)
    {
        if (size < 1) throw new ArgumentOutOfRangeException("size must be above 2 characters");
        if (seed == null) throw new ArgumentNullException("seed is null");
        if (allowedCharacters.Length < 2)
            throw new ArgumentOutOfRangeException("allowed characters must include at least 2 characters");
        this.allowedCharacters = allowedCharacters.ToCharArray();
        this.charBuffer = new char[size];
        this.seed = seed;
    }

    /**
     * @return randomly generated String.
     */
    public String generate()
    {
        int idx = 0;
        while (idx < charBuffer.Length)
            charBuffer[idx++] = allowedCharacters[seed.Next(0,allowedCharacters.Length)];
        return new String(charBuffer);
    }
}
#endif