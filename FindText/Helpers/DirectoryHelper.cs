using System;
using System.Collections.Generic;
using System.IO;

/*
    直接Directory.EnumerateFiles() 会报错，需先查找可访问目录，再逐目录搜索文件
*/

namespace FindText.Helpers
{
    public  class DirectoryHelper
    {
        /// <summary>  
        /// 遍历目录及所有子目录，跳过无权限和系统文件夹  
        /// </summary>  
        /// <param name="rootPath">要遍历的根目录路径</param>  
        public static IEnumerable<string> GetAllAccessibleDirectories(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
                throw new ArgumentException("路径不能为空", nameof(rootPath));

            var stack = new Stack<string>();
            stack.Push(rootPath);

            while (stack.Count > 0)
            {
                var currentPath = stack.Pop();

                bool isAccessible = false;
                try
                {
                    var currentDirInfo = new DirectoryInfo(currentPath);
                    
                    //跳过系统或隐藏文件夹
                    //if (IsSystemOrHiddenDirectory(currentDirInfo)) 
                    //{                   
                    //    continue;
                    //}

                    if (!HasReadAccess(currentPath))
                    {
                        continue;
                    }
                    isAccessible = true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    //throw ex;
                    continue;
                }
                catch (DirectoryNotFoundException ex)
                {
                    //throw ex;
                    continue;
                }
                catch (IOException ex)
                {
                    //throw ex;
                    continue;
                }
                catch (Exception ex)
                {
                    //throw ex;
                    continue;
                }

                if (isAccessible)
                {
                    yield return currentPath;

                    // 获取子目录并加入待处理队列  
                    try
                    {
                        var subDirs = Directory.GetDirectories(currentPath);
                        foreach (var subDir in subDirs)
                        {
                            try
                            {
                                var subDirInfo = new DirectoryInfo(subDir);
                                if (IsSystemOrHiddenDirectory(subDirInfo))
                                {
                                    continue;
                                }

                                if (!HasReadAccess(subDir))
                                {
                                    continue;
                                }

                                stack.Push(subDir);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                continue;
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //throw ex;
                    }
                }
            }
        }

        private static bool IsSystemOrHiddenDirectory(DirectoryInfo dirInfo)
        {
            // 检查是否为隐藏文件夹  
            if ((dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                return true;

            // 检查是否为系统文件夹  
            if ((dirInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                return true;

            // 检查是否为临时文件夹  
            if ((dirInfo.Attributes & FileAttributes.Temporary) == FileAttributes.Temporary)
                return true;

            return false;
        }

        private static bool HasReadAccess(string path)
        {
            try
            {
                // 尝试读取目录内容  
                var test = Directory.GetDirectories(path);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }


    }

}
