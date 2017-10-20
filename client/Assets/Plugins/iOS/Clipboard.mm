NSString* CreateNSString (const char* string);

extern "C"
{
    void _copyTextToClipboard(const char *textList)
    {
        NSString *text = CreateNSString(textList);
        [UIPasteboard generalPasteboard].string = text;
    }
}

