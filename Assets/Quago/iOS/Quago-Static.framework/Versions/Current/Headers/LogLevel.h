/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */

#ifndef LogLevel_h
#define LogLevel_h

typedef NS_ENUM(NSInteger,LogLevel) {
    /* Enable all logs */
    LOG_VERBOSE = 2,
    /* Enable debug logs */
    LOG_DEBUG = 3,
    /* Default - used for integration */
    LOG_INFO = 4,
    /* Show only Error logs */
    LOG_WARNING = 5,
    /* Show only Error logs */
    LOG_ERROR = 6,
    /* Disable all logs */
    LOG_DISABLED = 10
};

#endif /* LogLevel_h */
