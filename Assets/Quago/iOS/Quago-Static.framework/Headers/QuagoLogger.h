/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */

#import <Foundation/Foundation.h>
#import "LogLevel.h"
#import "QuagoFunctionalLogger.h"

@class QuagoFunctionalLogger;

NS_ASSUME_NONNULL_BEGIN

typedef void(^QuagoLoggerCallback)(LogLevel logLevel, NSString* msg, NSException* throwable);

@interface QuagoLogger : NSObject

extern NSString* const PLACEHOLDER_CLASS;

+ (QuagoLoggerCallback) loggerCallback;

+ (void) setGlobalLogLevel : (LogLevel) logLevel;

+ (LogLevel) getLogLevel;

+ (QuagoFunctionalLogger*) create : (Class) theClass;

+ (QuagoFunctionalLogger*) createWithName : (NSString*) className;

+ (BOOL) askForPermission : (LogLevel) logLevel;

+ (BOOL) askForPermissionWithPriority : (int) priority;

+ (void) overrideLogger : (QuagoLoggerCallback) callback;

+ (void) release;

@end

NS_ASSUME_NONNULL_END
