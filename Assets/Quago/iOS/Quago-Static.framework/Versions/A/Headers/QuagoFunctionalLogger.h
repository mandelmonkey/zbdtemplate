/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */

#import <Foundation/Foundation.h>
#import "QuagoLogger.h"

@class QuagoLogger;

NS_ASSUME_NONNULL_BEGIN

@interface QuagoFunctionalLogger : NSObject

extern NSString* const TAG;
extern NSString* const EMPTY_STRING;

@property (nonatomic,copy) NSString* className;

- (instancetype) initWithName : (NSString*) className;

- (instancetype) init : (Class) clazz;

- (void) v : (NSString*) method exception : (NSException*) t;

- (void) v : (NSString*) method message : (NSString*) msg exception : (NSException*)t;

- (void) v : (NSString*) method message : (NSString*) msg, ...;


- (void) d : (NSString*) method exception : (NSException*) t;

- (void) d : (NSString*) method message : (NSString*) msg exception : (NSException*)t;

- (void) d : (NSString*) method message : (NSString*) msg, ...;


- (void) i : (NSString*) method exception : (NSException*) t;

- (void) i : (NSString*) method message : (NSString*) msg exception : (NSException*)t;

- (void) i : (NSString*) method message : (NSString*) msg, ...;


- (void) w : (NSString*) method exception : (NSException*) t;

- (void) w : (NSString*) method message : (NSString*) msg exception : (NSException*)t;

- (void) w : (NSString*) method message : (NSString*) msg, ...;


- (void) e : (NSString*) method exception : (NSException*) t;

- (void) e : (NSString*) method message : (NSString*) msg exception : (NSException*)t;

- (void) e : (NSString*) method message : (NSString*) msg, ...;


- (void) log : (LogLevel) logLevel method : (NSString*) method msg : (NSString*) msg exception : (nullable NSException*)t arguments : (va_list) args;

@end

NS_ASSUME_NONNULL_END
