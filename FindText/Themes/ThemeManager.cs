using System;
using System.Collections.Generic;
using System.Windows;

namespace FindText.Themes
{
    /*
        添加颜色主题：
         1、复制Themes/LightTheme.xaml，重命名为 【新主题.xaml】
         2、修改【新主题.xaml】中的颜色值
         3、在ThemeManager.cs 中登记新主题

     */

    public class ThemeManager
    {
        private static readonly Dictionary<string, Uri> Themes = new()
        {
            ["Light"] = new Uri("Themes/LightTheme.xaml", UriKind.Relative),
            ["Dark"] = new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
        };

        internal static void ApplyTheme(string themeName)
        {
            if (!Themes.ContainsKey(themeName)) return;

            var app = Application.Current;
            //var dict = app.Resources.MergedDictionaries.FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme") == true);
            int idx = app.Resources.MergedDictionaries.Count - 1;
            if (app.Resources.MergedDictionaries.Count >= 3)
            {
                //因为CiontrolsStyle.xaml里用的是StaticResource ,需都移除后重新加载
                app.Resources.MergedDictionaries.RemoveAt(idx );
                app.Resources.MergedDictionaries.RemoveAt(idx - 1);
            }

            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = Themes[themeName] });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Assets/CiontrolStyles.xaml", UriKind.Relative) });

            /*
            //WindowsServer2012上无效
            switch (themeName)
            {
                case "Dark": 
                    Application.Current.ThemeMode = ThemeMode.Dark;
                    break;

                case "Light":
                    Application.Current.ThemeMode = ThemeMode.Light;
                    break;
            }
            */
        }

        public static void SystemPreferenceChanged()
        {

        }
    }
}
