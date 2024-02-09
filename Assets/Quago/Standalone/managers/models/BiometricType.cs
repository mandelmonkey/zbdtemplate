/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
public enum BiometricType
{
    NULL = 0,

    MOTION_DOWN = 1,
    MOTION_POINTER_DOWN = 2,
    MOTION_MOVE = 3,
    MOTION_POINTER_UP = 4,
    MOTION_UP = 5,

    KEY_DOWN = 10,
    KEY_UP = 11,

    MOUSE_WHEEL = 20,

    ON_RESUME = 40,
    ON_PAUSE = 41,
    ON_STOP = 42,

    TEXT = 50,

    CUSTOM = 200
}
#endif