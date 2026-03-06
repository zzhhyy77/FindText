using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FindText.Helpers
{
    class EncodingHelper
    {
        public static Encoding DetectEncoding(byte[] bytes)
        {
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Encoding.UTF8;
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return Encoding.GetEncoding("UTF-16BE");
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return Encoding.GetEncoding("UTF-16LE");
            if (IsGB2312(bytes))
                return Encoding.GetEncoding("GB2312");
            if (IsUTF8(bytes))
                return Encoding.UTF8;

            return Encoding.Default;
        }

        public static bool IsGB2312(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return false;

            try
            {
                // 尝试使用GB2312解码
                string decoded = Encoding.GetEncoding("GB2312").GetString(bytes);

                // 重新编码回字节，检查是否一致
                byte[] reencoded = Encoding.GetEncoding("GB2312").GetBytes(decoded);

                // 比较原始字节和重新编码后的字节
                if (reencoded.Length != bytes.Length)
                    return false;

                for (int i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] != reencoded[i])
                        return false;
                }

                return true;
            }
            catch
            {
                // 解码失败，不是有效的GB2312编码
                return false;
            }
        }

        public static bool IsUTF8(byte[] bytes)
        {
            try
            {
                string utf8String = Encoding.UTF8.GetString(bytes);
                byte[] reencoded = Encoding.UTF8.GetBytes(utf8String);
                if (reencoded.Length == bytes.Length)
                {
                    bool match = true;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (bytes[i] != reencoded[i])
                        {
                            match = false;
                            break;
                        }
                    }
                    return match;                        
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

    }
}
