using System;
using System.IO;
using System.Text;

namespace FindText.Helpers
{
    internal class AppConfigHelper
    {
        /// <param name="folderName">程序目录内的文件夹名</param>
        /// <returns></returns>
        internal static string GetLocalFolder(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return Environment.CurrentDirectory;
            }
            else
            {
                string path = $"{Environment.CurrentDirectory}\\{folderName}";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }


        /// <summary>
        /// 将文本内容保存在当前文件夹内
        /// </summary>
        /// <param name="localFolder">程序文件夹内的子文件夹名</param>
        internal static void SaveConfig(string localFolder, string filename, string jsonStr)
        {
            string path = $"{GetLocalFolder(localFolder)}\\{filename}";
            File.WriteAllText(path, jsonStr);
        }

        internal static string LoadJson(string localFolder, string filename)
        {
            string path = $"{GetLocalFolder(localFolder)}\\{filename}";

            if (File.Exists(path))
            {
                byte[] fileBytes = File.ReadAllBytes(path);
                Encoding encoding = EncodingHelper.DetectEncoding(fileBytes); 
                string fileContent = encoding.GetString(fileBytes);
                return fileContent;
            }

            return string.Empty;
        }

        internal static string LoadJson(string filePath)
        {
            if (File.Exists(filePath))
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                Encoding encoding = EncodingHelper.DetectEncoding(fileBytes);
                string fileContent = encoding.GetString(fileBytes);
                return fileContent;
            }

            return string.Empty;
        }

    


    }
}
