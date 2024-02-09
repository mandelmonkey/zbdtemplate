/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyCodeDictionary
{
    protected Dictionary<KeyCode, int> dictionaryOfIndexs = new();
    protected bool[] eventTimeArray;

    public KeyCodeDictionary()
    {
        eventTimeArray = new bool[Enum.GetNames(typeof(KeyCode)).Length];

        int index = 0;
        foreach (KeyCode enumValue in KeyCode.GetValues(typeof(KeyCode)))
        {
            eventTimeArray[index] = false;
            dictionaryOfIndexs.TryAdd(enumValue, index);
            index++;
        }
    }

    public void OnKeyDown(KeyCode keyCode)
    {
        eventTimeArray[dictionaryOfIndexs[keyCode]] = true;
    }

    public void OnKeyUp(KeyCode keyCode)
    {
        eventTimeArray[dictionaryOfIndexs[keyCode]] = false;
    }

    public bool Check(KeyCode keyCode)
    {
        return eventTimeArray[dictionaryOfIndexs[keyCode]];
    }
}
#endif