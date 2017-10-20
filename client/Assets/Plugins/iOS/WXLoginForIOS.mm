#import "WXApi.h"
#import <Foundation/Foundation.h>
#import <Security/Security.h>

NSString* CreateNSString (const char* string);
// Helper method to create C string copy
char* MakeStringCopy (const char* string);

@interface MirrorKeyChain : NSObject
+ (void)key_chain_save:(NSString *)service data:(id)data;
+ (id)key_chain_load:(NSString *)service;
+ (void)key_chain_delete:(NSString *)service;
@end

@implementation MirrorKeyChain
+ (NSMutableDictionary *)getKeychainQuery:(NSString *)service {
    return [NSMutableDictionary dictionaryWithObjectsAndKeys:
            (id)kSecClassGenericPassword,(id)kSecClass,
            service, (id)kSecAttrService,
            service, (id)kSecAttrAccount,
            (id)kSecAttrAccessibleAfterFirstUnlock,(id)kSecAttrAccessible,
            nil];
}

+ (void)key_chain_save:(NSString *)service data:(id)data {
    //Get search dictionary
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    //Delete old item before add new item
    SecItemDelete((CFDictionaryRef)keychainQuery);
    //Add new object to search dictionary(Attention:the data format)
    [keychainQuery setObject:[NSKeyedArchiver archivedDataWithRootObject:data] forKey:(id)kSecValueData];
    //Add item to keychain with the search dictionary
    SecItemAdd((CFDictionaryRef)keychainQuery, NULL);
}

+ (id)key_chain_load:(NSString *)service {
    id ret = nil;
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    //Configure the search setting
    //Since in our simple case we are expecting only a single attribute to be returned (the password) we can set the attribute kSecReturnData to kCFBooleanTrue
    [keychainQuery setObject:(id)kCFBooleanTrue forKey:(id)kSecReturnData];
    [keychainQuery setObject:(id)kSecMatchLimitOne forKey:(id)kSecMatchLimit];
    CFDataRef keyData = NULL;
    if (SecItemCopyMatching((CFDictionaryRef)keychainQuery, (CFTypeRef *)&keyData) == noErr) {
        try {
            ret = [NSKeyedUnarchiver unarchiveObjectWithData:(__bridge NSData *)keyData];
        } catch (NSException *e) {
            NSLog(@"Unarchive of %@ failed: %@", service, e);
        }
    }
    if (keyData)
        CFRelease(keyData);
    return ret;
}

+ (void)key_chain_delete:(NSString *)service {
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    SecItemDelete((CFDictionaryRef)keychainQuery);
}

@end

extern "C" {
	void _OpenWXLogin()
	{
        SendAuthReq *req = [[SendAuthReq alloc] init];
        req.scope = @"snsapi_userinfo";
        req.state = @"mirror_tech";
        
        [WXApi sendReq:req];
	}

	int _IsWXInstalled()
	{
		if( [WXApi isWXAppInstalled] )
		{
			return 1;
		}
		return 0;
	}

	void _OpenWXPay(const char* json)
	{
		NSError *error;
		NSString *jsonstring = CreateNSString(json);
		NSData *data = [jsonstring   dataUsingEncoding:NSUTF8StringEncoding];
		NSMutableDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableLeaves error:&error];
		if(dict != nil){
            NSMutableString *stamp  = [dict objectForKey:@"timestamp"];
            //调起微信支付
            PayReq* req             = [[PayReq alloc] init];
            req.partnerId           = [dict objectForKey:@"partnerid"];
            req.prepayId            = [dict objectForKey:@"prepayid"];
            req.nonceStr            = [dict objectForKey:@"noncestr"];
            req.timeStamp           = stamp.intValue;
            req.package             = [dict objectForKey:@"package"];
            req.sign                = [dict objectForKey:@"sign"];
            [WXApi sendReq:req];
        }
	}

	void _WXShareUrl(const char* url, const char* title, const char* desc, const char* thumb, bool is_timeline)
	{
        WXMediaMessage *message = [WXMediaMessage message];
        message.title = CreateNSString(title);
        message.description = CreateNSString(desc);
        [message setThumbImage: [UIImage imageWithContentsOfFile:CreateNSString(thumb)]];
        
        WXWebpageObject *webObject = [WXWebpageObject object];
        webObject.webpageUrl = CreateNSString(url);
        message.mediaObject = webObject;
        
        SendMessageToWXReq *req = [[SendMessageToWXReq alloc] init];
        req.bText = NO;
        req.message = message;
        req.scene = is_timeline ? WXSceneTimeline : WXSceneSession;
        [WXApi sendReq:req];
    }

    //
	void _WXSharePicture(const char* file, int width, int height, bool is_timeline)
	{
        WXMediaMessage *message = [WXMediaMessage message];
        NSData * imgData = [NSData dataWithContentsOfFile:CreateNSString(file)];
        UIImage *image = [UIImage imageWithData:imgData];

        // 坑爹的微信sdk，这个缩略图超过了32k会莫名失败的。
        UIGraphicsBeginImageContext(CGSizeMake(width, height));
        [image drawInRect:CGRectMake(0, 0, width, height)];
        UIImage *newImage = UIGraphicsGetImageFromCurrentImageContext();
        UIGraphicsEndImageContext();
        NSData *thumb = UIImageJPEGRepresentation(newImage, 0.5);
        
        WXImageObject *imageObject = [WXImageObject object];
        imageObject.imageData = imgData;
        message.thumbData = thumb;
        message.mediaObject = imageObject;
        
        SendMessageToWXReq* req = [[SendMessageToWXReq alloc] init];
        req.bText = NO;
        req.message = message;
        req.scene = is_timeline ? WXSceneTimeline : WXSceneSession;
        [WXApi sendReq:req];
	}
    
    const char * _GetKeyChain(const char* app_key, const char *data_key)
    {
        NSMutableDictionary *usernamepasswordKVPairs = (NSMutableDictionary *)[MirrorKeyChain key_chain_load:CreateNSString(app_key)];
        if (usernamepasswordKVPairs == nil)
        {
            return NULL;
        }
        return MakeStringCopy([[usernamepasswordKVPairs objectForKey:CreateNSString(data_key)] UTF8String]);
    }
    
    void _SaveKeyChain(const char* app_key, const char *data_key, const char* value)
    {
        NSMutableDictionary *usernamepasswordKVPairs = [NSMutableDictionary dictionary];
        [usernamepasswordKVPairs setObject:CreateNSString(value) forKey:CreateNSString(data_key)];
        [MirrorKeyChain key_chain_save:CreateNSString(app_key) data:usernamepasswordKVPairs];
    }
}
