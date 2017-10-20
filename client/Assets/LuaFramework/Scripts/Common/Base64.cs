using System;
using System.Text;


// 2016 kingbird 
class Base64
{
    // 这个字符表跟标准的base64不同，只要和服务器约定一样的序列就可以使用，非标准序列可以做到更安全的加密
    byte[] code = Encoding.ASCII.GetBytes("sY+N7PTnwM6bfAg4eaxkzCdhvJlUXZQSF82mDpK5WL/uEo31c0jiRtIHyqBOVGr9");
    byte[] reverse = new byte[128];

    static Base64 inst = new Base64();

    private Base64()
    {
        for(int i = 0; i < code.Length; i++)
        {
            reverse[code[i]] = (byte)i;
        }
    }

    public static Base64 Inst
    {
        get { return inst; }
    }

    // 假定byte长度是8位了
    public byte[] Encode(byte[] input)
    {
        byte[] ret = new byte[((input.Length + 2)/3) * 4];
        int index = 0;
        int i;
        for (i = 2 ; i < input.Length; i += 3)
        {
            int val = (input[i - 2] << 16) | (input[i - 1] << 8) | input[i];
            ret[index++] = code[val >> 18];
            ret[index++] = code[(val >> 12) & 0x3f];
            ret[index++] = code[(val >> 6) & 0x3F];
            ret[index++] = code[val & 0x3F];
        }
        i -= 2;
        switch(input.Length % 3)
        {
            case 1:
                ret[index++] = code[input[i] >> 2];
                ret[index++] = code[(input[i] << 4) & 0x3F];
                break;
            case 2:
                ret[index++] = code[input[i] >> 2];
                ret[index++] = code[(input[i] << 4) & 0x3F | (input[i+1] >> 4)];
                ret[index++] = code[(input[i+1] << 2) & 0x3F];
                break;
            default:break;
        }

        while (index < ret.Length)
        {
            ret[index++] = (byte)'=';
        }
        return ret;
    } 

    public byte[] Decode(byte[] input)
    {
        int len = (input.Length / 4) * 3;
        int tail = 0;
        if (len > 0)
        {
            tail += input[input.Length - 1] == (byte)'=' ? 1 : 0;
            tail += input[input.Length - 2] == (byte)'=' ? 1 : 0;
            len -= tail;
        }
        byte[] ret = new byte[len];

        int i, index = 0, serach_len = input.Length - tail;
        for (i = 3; i < serach_len; i += 4)
        {
            int val = (reverse[input[i - 3]] << 18) | (reverse[input[i - 2]] << 12) | (reverse[input[i - 1]] << 6) | reverse[input[i]];
            ret[index++] = (byte)(val >> 16);
            ret[index++] = (byte)((val >> 8) & 0xFF);
            ret[index++] = (byte)(val &0xFF);
        }
        i -= 3;
        switch (tail)
        {
            case 1:
                int val = (reverse[input[i]] << 12) | (reverse[input[i+1]] << 6) | reverse[input[i+2]];
                ret[index++] = (byte)(val >> 10);
                ret[index++] = (byte)((val >> 2) & 0xFF);
                break;
            case 2:
                val = (reverse[input[i]] << 6) | reverse[input[i + 1]];
                ret[index++] = (byte)(val >> 4);
                break;
            default: break;
        }

        return ret;
    }
}

