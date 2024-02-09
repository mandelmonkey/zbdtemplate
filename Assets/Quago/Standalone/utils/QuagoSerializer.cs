/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class QuagoSerializer
{
    private const char SQUARE_BRACKET_BEGIN = '[', SQUARE_BRACKET_END = ']',
                     CURLY_BRACKET_START = '{', CURLY_BRACKET_END = '}',
                     COLON = ':', COMMA = ',', QUOTATION_MARK = '"';
    private const string NULL = "null";

    /**
     * From RFC 7159, "All Unicode characters may be placed within the quotation marks except for
     * the characters that must be escaped: quotation mark, reverse solidus, and the control characters
     * (U+0000 through U+001F)."
     */
    private static readonly string[] REPLACEMENT_CHARS;

    static QuagoSerializer()
    {
        REPLACEMENT_CHARS = new string[128];
        for (int i = 0; i <= 0x1f; i++) REPLACEMENT_CHARS[i] = string.Format("\\u{0:x4}", i);
        REPLACEMENT_CHARS['"'] = "\\\"";
        REPLACEMENT_CHARS['\\'] = "\\\\";
        REPLACEMENT_CHARS['\t'] = "\\t";
        REPLACEMENT_CHARS['\b'] = "\\b";
        REPLACEMENT_CHARS['\n'] = "\\n";
        REPLACEMENT_CHARS['\r'] = "\\r";
        REPLACEMENT_CHARS['\f'] = "\\f";
    }

    private StringBuilder builder;

    public string ToJSON(object obj)
    {
        return ToJSON(obj, 200000);
    }

    public string ToJSON(object obj, int bufferSize)
    {
        if (obj == null) return NULL;
        try
        {
            if (builder == null) builder = new StringBuilder(bufferSize);
            serializeObject(obj);
            string json = builder.ToString();
            builder.Length = 0;

            return json;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        return null;
    }

    private void serializeObject(object obj)
    {
        try
        {
            /* Null */
            if (obj == null)
            {
                builder.Append(NULL);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            /* Primitives */
            if (obj is float)
            {
                float value = (float)obj;
                if (float.IsNaN(value))
                {
                    builder.Append(NULL);
                    return;
                }
                builder.Append(value);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            if (obj is int)
            {
                builder.Append(((int)obj));
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            if (obj is long)
            {
                builder.Append(((long)obj));
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            if (obj is string || obj is String)
            {
                string charSequence = (string)obj;
                int length = charSequence.Length;
                builder.Append(QUOTATION_MARK);
                for (int i = 0; i < length; i++)
                {
                    char c = charSequence[i];
                    if (c < REPLACEMENT_CHARS.Length)
                    {
                        string replacement = REPLACEMENT_CHARS[c];
                        if (replacement != null)
                        {
                            builder.Append(replacement);
                            continue;
                        }
                    }
                    builder.Append(c);
                }
                builder.Append(QUOTATION_MARK);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            if (obj is bool || obj is Boolean)
            {
                builder.Append((bool)obj ? "true" : "false");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            if (obj is double || obj is Double)
            {
                double value = (double)obj;
                if (Double.IsNaN(value))
                {
                    builder.Append(NULL);
                    return;
                }
                builder.Append(value);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            if (obj is short)
            {
                builder.Append(((short)obj));
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            if (obj is char || obj is Char)
            {
                builder.Append(QUOTATION_MARK);
                builder.Append(((char)obj));
                builder.Append(QUOTATION_MARK);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            if (obj is Byte)
            {
                builder.Append(((byte)obj));
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            /* Array */
            if (obj.GetType().IsArray)
            {
                builder.Append(SQUARE_BRACKET_BEGIN);

                if (obj is double[] doubles)
                {
                    if (doubles.Length > 0)
                    {
                        serializeObject(doubles[0]);
                        for (int i = 1; i < doubles.Length; i++)
                        {
                            builder.Append(COMMA);
                            serializeObject(doubles[i]);
                        }
                    }
                }
                else if (obj is string[] strings)
                {
                    if (strings.Length > 0)
                    {
                        serializeObject(strings[0]);
                        for (int i = 1; i < strings.Length; i++)
                        {
                            builder.Append(COMMA);
                            serializeObject(strings[i]);
                        }
                    }
                }
                else if (obj is int[] integers)
                {
                    if (integers.Length > 0)
                    {
                        serializeObject(integers[0]);
                        for (int i = 1; i < integers.Length; i++)
                        {
                            builder.Append(COMMA);
                            serializeObject(integers[i]);
                        }
                    }
                }
                else if (obj is float[] floats)
                {
                    if (floats.Length > 0)
                    {
                        serializeObject(floats[0]);
                        for (int i = 1; i < floats.Length; i++)
                        {
                            builder.Append(COMMA);
                            serializeObject(floats[i]);
                        }
                    }
                }
                else if (obj is object[] objects)
                {
                    if (objects.Length > 0)
                    {
                        serializeObject(objects[0]);
                        for (int i = 1; i < objects.Length; i++)
                        {
                            builder.Append(COMMA);
                            serializeObject(objects[i]);
                        }
                    }
                }

                builder.Append(SQUARE_BRACKET_END);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            /* Enum */
            if (obj is Enum)
            {
                builder.Append(QUOTATION_MARK);
                builder.Append(((Enum)obj).GetType().Name);
                builder.Append(QUOTATION_MARK);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        bool shouldCOMMA;
        try
        {
            /* Collections */
            if (IsDictionary(obj))
            {
                //KeyValuePair
                builder.Append(CURLY_BRACKET_START);
                IDictionary map = (IDictionary)obj;
                shouldCOMMA = false;
                foreach (DictionaryEntry pair in map)
                {
                    if (shouldCOMMA) builder.Append(COMMA);
                    object keyObj = pair.Key;

                    builder.Append(QUOTATION_MARK);

                    if (keyObj is int)
                        builder.Append(((int)keyObj));
                    else if (keyObj is float)
                        builder.Append(((float)keyObj));
                    else if (keyObj is long)
                        builder.Append(((long)keyObj));
                    else if (keyObj is string)
                        builder.Append(((string)keyObj));
                    else if (keyObj is bool || keyObj is Boolean)
                        builder.Append(((bool)keyObj));
                    else if (keyObj is double || keyObj is Double)
                        builder.Append(((double)keyObj));
                    else if (keyObj is short)
                        builder.Append(((short)keyObj));
                    else if (keyObj is char || keyObj is Char)
                        builder.Append(((char)keyObj));
                    else if (keyObj is byte || keyObj is Byte)
                        builder.Append(((byte)keyObj));
                    else
                        serializeObject(keyObj);

                    builder.Append(QUOTATION_MARK);
                    builder.Append(COLON);
                    serializeObject(pair.Value);
                    shouldCOMMA = true;
                }

                builder.Append(CURLY_BRACKET_END);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            if (obj is IList)
            {
                builder.Append(SQUARE_BRACKET_BEGIN);

                shouldCOMMA = false;
                IList iterable = (IList)obj;
                foreach (object o in iterable)
                {
                    if (shouldCOMMA) builder.Append(COMMA);
                    serializeObject(o);
                    shouldCOMMA = true;
                }

                builder.Append(SQUARE_BRACKET_END);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        try
        {
            /* Object */
            builder.Append(CURLY_BRACKET_START);

            shouldCOMMA = false;
            Type current = obj.GetType();
            do
            {
                FieldInfo[] fields = current.GetFields();
                foreach (FieldInfo field in fields)
                {
                    /* Ignore synthetic/not needed fields such as $change */
                    if (field.IsNotSerialized ||
                        field.IsStatic) continue;

                    if (shouldCOMMA) builder.Append(COMMA);

                    /* Field */
                    builder.Append(QUOTATION_MARK);
                    builder.Append(field.Name);
                    builder.Append(QUOTATION_MARK);
                    builder.Append(COLON);

                    /* Value */
                    serializeObject(field.GetValue(obj));

                    shouldCOMMA = true;
                }
                if (current.GetNestedTypes().Length == 0) break;
            } while ((current = current.GetNestedTypes()[0]) != null);

            builder.Append(CURLY_BRACKET_END);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private bool IsList(object o)
    {
        if (o == null) return false;
        return o is IList &&
               o.GetType().IsGenericType &&
               o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
    }

    private bool IsDictionary(object o)
    {
        if (o == null) return false;
        return o is IDictionary &&
               o.GetType().IsGenericType &&
               o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
    }
}
#endif