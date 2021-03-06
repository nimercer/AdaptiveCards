//
//  ACRRegistartion
//  ACRRegistartion.h
//
//  Copyright © 2017 Microsoft. All rights reserved.
//
//
@class ACRBaseCardElementRenderer;

#import "ACRBaseActionElementRenderer.h"
#import "ACOBaseCardElement.h"

@interface ACRRegistration:NSObject

+ (ACRRegistration *)getInstance;

- (ACRBaseCardElementRenderer *)getRenderer:(NSNumber *) cardElementType;

- (ACRBaseActionElementRenderer *)getActionRenderer:(NSNumber *) cardElementType;

- (id<ACRIBaseActionSetRenderer>)getActionSetRenderer;

- (void)setActionRenderer:(ACRBaseActionElementRenderer *)renderer cardElementType:(NSNumber *)cardElementType;

- (void)setBaseCardElementRenderer:(ACRBaseCardElementRenderer *)renderer cardElementType:(ACRCardElementType)cardElementType;

- (void)setActionSetRenderer:(id<ACRIBaseActionSetRenderer>)actionsetRenderer;

- (void)setCustomElementParser:(NSObject<ACOIBaseCardElementParser> *)customElementParser key:(NSString *)key;;

- (NSObject<ACOIBaseCardElementParser> *)getCustomElementParser:(NSString *)key;

- (void)setCustomElementRenderer:(ACRBaseCardElementRenderer *)renderer key:(NSString *)key;

- (BOOL)isElementRendererOverridden:(ACRCardElementType)cardElementType;

- (BOOL)isActionRendererOverridden:(NSNumber *)cardElementType;

- (void)setCustomActionElementParser:(NSObject<ACOIBaseActionElementParser> *)parser key:(NSString *)key;

- (NSObject<ACOIBaseActionElementParser> *)getCustomActionElementParser:(NSString *)key;

- (void)setCustomActionRenderer:(ACRBaseActionElementRenderer *)renderer key:(NSString *)key;

- (ACOParseContext *)getParseContext;

@end
