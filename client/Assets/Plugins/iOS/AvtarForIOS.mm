#import <QuartzCore/CADisplayLink.h>
#import <AdSupport/ASIdentifierManager.h>

@interface UnityAvtar : UIViewController<UIImagePickerControllerDelegate, UINavigationControllerDelegate>
@property (nonatomic) NSString* _persist;
@property (nonatomic) NSString* _file_name;
@end


NSString* CreateNSString (const char* string);
// Helper method to create C string copy
char* MakeStringCopy (const char* string);

@implementation UnityAvtar

- (void)OpenTarget:(UIImagePickerControllerSourceType)type
{
	
    UIImagePickerController * picker = [[UIImagePickerController alloc] init];
    
    picker.delegate = self;
    picker.allowsEditing = YES;
    picker.sourceType = type;
    
    [self presentViewController: picker animated:YES completion:nil];
}

-(void)imagePickerController: (UIImagePickerController *)pic
	didFinishPickingMediaWithInfo:(NSDictionary *)info
{
    [pic dismissViewControllerAnimated: YES completion:nil];
    
    UIImage* image = info[UIImagePickerControllerEditedImage ];
    NSString *imagePath = [self GetSavePath];
    [self SaveFileToDoc:image path:imagePath];
}

-(NSString *) GetSavePath
{
	NSString *docPath = self._persist;

	return [docPath stringByAppendingPathComponent:self._file_name];
}

- (UIImage *)compressImage:(UIImage *)sourceImage toTargetWidth:(CGFloat)targetWidth {
    CGSize imageSize = sourceImage.size;
    
    CGFloat width = imageSize.width;
    CGFloat height = imageSize.height;
    
    CGFloat targetHeight = (targetWidth / width) * height;
    
    UIGraphicsBeginImageContext(CGSizeMake(targetWidth, targetHeight));
    [sourceImage drawInRect:CGRectMake(0, 0, targetWidth, targetHeight)];
    
    UIImage *newImage = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    
    return newImage;
}

-(void) SaveFileToDoc:(UIImage *)image 
	path:(NSString *)path
{
    UIImage* scale = [self compressImage:image toTargetWidth: 256];
	NSData *data = UIImageJPEGRepresentation(scale, 0.5);

	if (data == nil){
		data = UIImagePNGRepresentation(scale);
	}

    if (data != nil)
    {
        [data writeToFile:path atomically:YES];
        UnitySendMessage("GameManager", "avatar_message", MakeStringCopy([[self GetSavePath] UTF8String]));
    }
}

-(void)imagePickerControllerDidCancel:(UIImagePickerController *)pic
{
	[pic dismissViewControllerAnimated: YES completion:nil];
}

-(void)objc_copyTextToClipboard:(NSString*)text
{
	UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
	pasteboard.string = text;
}

@end

UnityAvtar *MakeNewAvtar(const char * persist, const char* file_name)
{
	UnityAvtar *app = [[UnityAvtar alloc] init];
	app._file_name = CreateNSString(file_name);
	app._persist = CreateNSString(persist);

	UIViewController *vc = UnityGetGLViewController();
	[vc.view addSubview:app.view];
	return app;
}

extern "C" {

	void _OpenCamera(const char * persist, const char* file_name)
	{
		UnityAvtar *app = MakeNewAvtar(persist, file_name);

		if ([UIImagePickerController  isSourceTypeAvailable:UIImagePickerControllerSourceTypeCamera])
		{
			[app OpenTarget:UIImagePickerControllerSourceTypeCamera];
		}
	}

	void _OpenPhoto(const char * persist, const char* file_name)
	{
		UnityAvtar *app = MakeNewAvtar(persist, file_name);

		if ([UIImagePickerController  isSourceTypeAvailable:UIImagePickerControllerSourceTypePhotoLibrary])
		{
			[app OpenTarget:UIImagePickerControllerSourceTypePhotoLibrary];
		}
	}

	const char* _GetIdfa()
	{
		NSString *str = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
        return MakeStringCopy([str UTF8String]);
	}

	const char* _GetVersion(){
        UIDevice *device = [UIDevice currentDevice];
        NSString *version =  device.systemName; 
        return MakeStringCopy([version UTF8String]);
    } 
}
