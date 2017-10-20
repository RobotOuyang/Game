//
//  AppStorePay.h
//  Unity-iPhone
//
//  Created by 镜像 on 16/9/17.
//
//

#ifndef AppStorePay_h
#define AppStorePay_h


@interface  AppStorePay : NSObject<SKProductsRequestDelegate>
@property (nonatomic) NSString* _item_name;
@property (nonatomic) NSString* _user_name;
@property (nonatomic) SKProductsRequest * request;
@property (nonatomic) NSArray<SKProduct *> * products;

+ (void) CheckReceipt: (SKPaymentTransaction *) transaction;

+ (void) SaveTransactions: (NSArray *) trans;

@end

#endif /* AppStorePay_h */
