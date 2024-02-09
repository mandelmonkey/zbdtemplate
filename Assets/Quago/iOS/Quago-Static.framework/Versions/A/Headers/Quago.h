/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "QuagoSettingsBuilder.h"
#import "QuagoFunctionalLogger.h"

//! Project version number for Quago.
FOUNDATION_EXPORT double QuagoVersionNumber;

//! Project version string for Quago.
FOUNDATION_EXPORT const unsigned char QuagoVersionString[];

NS_ASSUME_NONNULL_BEGIN
@interface Quago : NSObject

+(void)initialize:(QuagoSettingsBuilder *) builder;

+(void)initializeWithSettings:(QuagoSettings *) settings;

+ (void)beginSegment:(NSString *)name;

+ (void)endSegment;

+ (void)setKeyValues:(NSString *)value forKey:(NSString *)key;

+ (void)sendPurchaseEvent:(NSString *)productName price:(double) price currency:(NSString *) currency;

+ (void)setUserId:(NSString *)userId;

+ (void)setAdditionalId:(NSString *)additionalId;

+ (NSString * _Nullable)getSessionId;

+ (void)onResume;

+ (void)onPause;

@end
NS_ASSUME_NONNULL_END
