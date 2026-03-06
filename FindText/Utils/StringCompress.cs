using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FindText.Utils
{
    public class StringCompress
    {
        public static byte[] Compress(string input)
        {
            byte[] rev;

            using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var compressedStream = new MemoryStream())
            {
                using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Optimal, true))
                {
                    uncompressedStream.CopyTo(compressorStream);
                }
                rev = compressedStream.ToArray();
            }
            return rev;

            //return Convert.ToBase64String(compressedBytes);
        }

        public static string Decompress(byte[] data)
        {
            byte[] decompressedBytes;
            var compressedStream = new MemoryStream(data);

            using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            using (var decompressedStream = new MemoryStream())
            {
                decompressorStream.CopyTo(decompressedStream);
                decompressedBytes = decompressedStream.ToArray();
            }
            return Encoding.UTF8.GetString(decompressedBytes);
        }

    }
}


