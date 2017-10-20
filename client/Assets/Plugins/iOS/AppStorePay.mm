//
//  AppStorePay.mm
//  Unity-iPhone
//
//  Created by 镜像 on 16/9/16.
//  注意，因为支付到账的回调可能需要在应用刚打开的时候就响应，因此部分逻辑写在了appdelegate里
//

#import <StoreKit/StoreKit.h>
#import <CommonCrypto/CommonCrypto.h>
#import "AppStorePay.h"

NSString* CreateNSString (const char* string);
// Helper method to create C string copy
char* MakeStringCopy (const char* string);

static NSArray * transactions;

@implementation AppStorePay

// AppStore Payment
- (void) validateProductIdentifiers
{
    NSSet * set = [NSSet setWithArray:@[self._item_name]];
    
    SKProductsRequest *productsRequest = [[SKProductsRequest alloc]
                                          initWithProductIdentifiers:set];
    
    // Keep a strong reference to the request.
    self.request = productsRequest;
    productsRequest.delegate = self;
    [productsRequest start];
}

- (void) productsRequest:(SKProductsRequest *)request
      didReceiveResponse:(SKProductsResponse *)response
{
    self.products = response.products;
    
    if (response.products.count == 0)
    {
        NSLog(@"获取不到商品列表！");
        return;
    }
    
    SKMutablePayment *payment = [SKMutablePayment paymentWithProduct:[self.products objectAtIndex:0]];
    payment.quantity = 1;
    payment.applicationUsername = self._user_name;
    [[SKPaymentQueue defaultQueue] addPayment:payment];
}

+ (void) CheckReceipt: (SKPaymentTransaction *) transaction
{
    try {
        NSDictionary * dict = @{
                                @"state" : @"suc",
                                @"player": transaction.payment.applicationUsername,
                                @"receipt": [transaction.transactionReceipt base64EncodedStringWithOptions:0],
                                @"hash": [NSString stringWithFormat:@"%lu", (unsigned long)transaction.hash],
                            };
        
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dict options:0 error:nil];
        NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        NSLog(@"pay suc : %@", jsonString);
        UnitySendMessage("GameManager", "appstore_message", MakeStringCopy([jsonString UTF8String]));
    } catch ( NSException *e) {
        NSLog(@"%@", e);
    }
}

+ (void) SaveTransactions: (NSArray *) trans
{
    transactions = [trans copy];
}

+ (void) FinishTransaction : (NSString *) hash_code
{
    if (transactions != nil)
    {
        for(SKPaymentTransaction *transaction in transactions)
        {
            if (transaction.hash == [hash_code integerValue])
            {
                [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
            }
        }
    }
}
+ (NSDictionary *)dictionaryWithJsonString:(NSString *)jsonString {
    if (jsonString == nil) {
        return nil;
    }
    
    NSData *jsonData = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSError *err;
    NSDictionary *dic = [NSJSONSerialization JSONObjectWithData:jsonData
                                                        options:NSJSONReadingMutableContainers
                                                          error:&err];
    if(err) {
        NSLog(@"json解析失败：%@",err);
        return nil;
    }
    return dic;
}
@end

// 强行拿住每一个支付实例
NSMutableSet* pay_list = [[NSMutableSet alloc] init];

extern "C"
{
    void _AppStoreBuy(const char * name, const char * user_name)
    {
        if ([SKPaymentQueue canMakePayments])
        {
            AppStorePay *pay = [[AppStorePay alloc] init];
            [pay_list addObject:pay];
            pay._item_name = CreateNSString(name);
            pay._user_name = CreateNSString(user_name);
            [pay validateProductIdentifiers];
        }
        else
        {
            NSLog(@"用户禁止应用内付费");
        }
    }
    
    void _FinishAppPay(const char * hash_code)
    {
        NSString * str = CreateNSString(hash_code);
        [AppStorePay FinishTransaction:str];
    }
	void _Bpay(const char * data)
    {
        NSString * str = CreateNSString(data);
        //
        //        [[JsPay sharedInstance] payorderWithJson:[AppStorePay dictionaryWithJsonString:str] backBlock:^(id resultDic) {
        //            //微信回调配置完成后，success支付成功，fail支付失败,
        //            //支付宝模式需要客户端，没有回调，需要调用查询接口
        //            NSLog(@"%@",resultDic);
        //        }];
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:str]];
        
    }
}
