using System.Linq;
using System.Text.RegularExpressions;
using System;
using FindText.Helpers;


namespace FindText.Helpers
{
    public static class RegexHelper
    {
        /// <summary>
        /// 检查文本是否同时包含所有关键字
        /// </summary>
        public static bool ContainsAllKeywords(string text, params string[] keywords)
        {
            if (string.IsNullOrEmpty(text) || keywords.Length == 0)
                return false;

            string pattern = string.Concat(
                keywords.Select(k => $"(?=.*{Regex.Escape(k)})")
            );

            return Regex.IsMatch(text, pattern);
        }

        /// <summary>
        /// 检查文本是否包含任意一个关键字
        /// </summary>
        public static bool ContainsAnyKeyword(string text, params string[] keywords)
        {
            if (string.IsNullOrEmpty(text) || keywords.Length == 0)
                return false;

            string pattern = string.Join("|", keywords.Select(Regex.Escape));
            return Regex.IsMatch(text, pattern);
        }
    }

}

/*
 
示例

        string text = "支持Java、Python和C#开发";

        bool all = RegexHelper.ContainsAllKeywords(text, "Java", "Python");
        bool any = RegexHelper.ContainsAnyKeyword(text, "Java", "Go", "Ruby");

        Console.WriteLine($"包含所有: {all}"); // True
        Console.WriteLine($"包含任意: {any}"); // True

*/
