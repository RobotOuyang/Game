#import "UnityAppController.h"
#import <StoreKit/StoreKit.h>
#import "AppStorePay.h"
#import "WXApi.h"

@interface MirrorAppDelegate : UnityAppController <WXApiDelegate, SKPaymentTransactionObserver>

@end

// Helper method to create C string copy
// 返回给unity，防止本地nsstring已经销毁，有少量内存泄漏，不会频繁生成
char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

NSString* CreateNSString (const char* string)
{
    if (string)
        return [NSString stringWithUTF8String: string];
    else
        return [NSString stringWithUTF8String: ""];
}

@implementation MirrorAppDelegate

- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
    [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
    return [WXApi handleOpenURL:url delegate:self];
}

- (BOOL)application:(UIApplication*)application handleOpenURL:(NSURL*)url
{
    [super application:application handleOpenURL:url];
    return [WXApi handleOpenURL:url delegate:self];
}

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    [[SKPaymentQueue defaultQueue] addTransactionObserver: self];       // appstore
    [WXApi registerApp:@"wx852782bbc02a58c3" withDescription:@"weixin"]; // weixin

 
    return [super application:application didFinishLaunchingWithOptions:launchOptions];
}

// weixin

- (void) onReq:(BaseReq *)req
{
    
}

- (void) onResp:(BaseResp *)resp
{
    if ([resp isKindOfClass:[SendAuthResp class]])
    {
        SendAuthResp *auth = (SendAuthResp *)resp;
        if (resp.errCode == WXSuccess && auth.code != nil)
        {
            UnitySendMessage("GameManager", "wx_login_message", MakeStringCopy([auth.code UTF8String]));
        }
    }
    else if ([resp isKindOfClass:[PayResp class]])
    {
        PayResp * pay = (PayResp *)resp;
        if (pay.errCode != WXSuccess && pay.errStr != nil)
        {   
            UnitySendMessage("GameManager","wx_pay_message", MakeStringCopy([pay.errStr UTF8String]));
        }
    }
    else if ([resp isKindOfClass:[SendMessageToWXResp class]])
    {
        if (resp.errCode == WXSuccess)
        {
            UnitySendMessage("GameManager","wx_share_message", "ios_ok");
        }
    }
}

// appstore
- (void)paymentQueue:(SKPaymentQueue *)queue
 updatedTransactions:(NSArray *)transactions
{
    [AppStorePay SaveTransactions:transactions];
    
    for (SKPaymentTransaction *transaction in transactions) {
        switch (transaction.transactionState) {
                // Call the appropriate custom method for the transaction state.
            case SKPaymentTransactionStatePurchasing:
                // UnitySendMessage("GameManager", "appstore_message", "begin");
                break;
            case SKPaymentTransactionStateDeferred:
                NSLog(@"transaction.deferred");
                break;
            case SKPaymentTransactionStateFailed:
                [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
                NSLog(@"transaction.error %@", [transaction.error localizedDescription]);
                break;
            case SKPaymentTransactionStatePurchased:
                [AppStorePay CheckReceipt:transaction];
                break;
            case SKPaymentTransactionStateRestored:
                [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
                break;
            default:
                // For debugging
                NSLog(@"Unexpected transaction state %@", @(transaction.transactionState));
                break;
        }
    }
}

@end
