using FindText.Helpers;
using FindText.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FindText.UC
{
    /// <summary>
    /// UCSettings.xaml 的交互逻辑
    /// </summary>
    public partial class UCSettings : UserControl
    {

        int inputFlag = 0;
        List<string>? _languages;

        public UCSettings()
        {
            InitializeComponent();
            LoadLanguages();
            comboboxTheme.ItemsSource = AppCache.Instance.Themes;

            if (!string.IsNullOrEmpty(AppCache.Instance.Configs.Theme))
            {
                foreach (KeyValuePair<ThemeCode, string> item in comboboxTheme.ItemsSource)
                {
                    if (item.Key.ToString() == AppCache.Instance.Configs.Theme)
                    {
                        comboboxTheme.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                comboboxTheme.SelectedIndex = 0;
            }

            //comboboxLanguage.SelectionChanged += ComboboxLanguage_SelectionChanged;
            comboboxTheme.SelectionChanged += ComboboxTheme_SelectionChanged;

            inputUnixTime.GotFocus += (s1, e1) => { inputFlag = 1; };
            inputDatetime.GotFocus += (s1, e1) => { inputFlag = 2; };

            inputUnixTime.TextChanged += InputUnixTime_TextChanged;
            inputDatetime.TextChanged += InputDatetime_TextChanged;

            buttonToolsWnd.Click += ButtonToolsWnd_Click;

            buttonOpenLocal.Click += ButtonOpenLocal_Click;
        }


        #region 事件处理

        private void ComboboxLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock? sel = comboboxLanguage.SelectedItem as TextBlock;
            if (sel != null)
            {
                TextCache.ExportLanguage();

                string path = $"{Environment.CurrentDirectory}\\Language";
                if (Directory.Exists(path))
                {
                    comboboxLanguage.Items.Clear();
                    LoadLanguages();
                    Win32Helper.OpenFileWithShell(path);
                }
            }
            else
            {
                string? lang = comboboxLanguage.SelectedItem as string;
                if (!string.IsNullOrEmpty(lang))
                {
                    AppCache.Instance.Configs.Language = lang;
                    TextCache.SetLanguage(lang);

                    //DivRoot.DataContext = null;
                    //DivRoot.DataContext = TextCache.Text;
                }
            }
        }

        private void ComboboxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var sel = comboboxTheme.SelectedItem as KeyValuePair<ThemeCode, string>?;
                if (sel == null)
                    return;

                AppCache.Instance.Configs.Theme = sel.Value.Key.ToString();
                ThemeManager.ApplyTheme(AppCache.Instance.Configs.Theme);
            }
            catch (Exception ex)
            {
                PopupHelper.ShowPopupMessage((ComboBox)sender, ex.Message);
            }
        }

        private void InputUnixTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inputFlag != 1)
                return;

            try
            {     
                if (long.TryParse(inputUnixTime.Text, out long unixTime))
                {
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
                    inputDatetime.Text = dtDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    inputDatetime.Text = TextCache.Text["Tool.UnixError"];
                }
            }
            catch
            {
                inputDatetime.Text = TextCache.Text["Tool.UnixError"];
            }
        }

        private void InputDatetime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inputFlag != 2)
                return;

            try
            {
                string str = inputDatetime.Text;
                switch (str.Length)
                {
                    case 8:
                        str = $"{str.Substring(0, 4)}-{str.Substring(4, 2)}-{str.Substring(6, 2)}";
                        break;
                    case 14:
                        str = $"{str.Substring(0, 4)}-{str.Substring(4, 2)}-{str.Substring(6, 2)} {str.Substring(8, 2)}:{str.Substring(10, 2)}:{str.Substring(12, 2)}";
                        break;
                }
                
                if (DateTime.TryParse(str, out DateTime dateTime))
                {
                    long unixTimestamp = Convert.ToInt64((dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
                    inputUnixTime.Text = $"{unixTimestamp}";
                }
                else
                {
                    inputUnixTime.Text = TextCache.Text["Tool.UnixError"];
                }
            }
            catch
            {
                inputUnixTime.Text = TextCache.Text["Tool.UnixError"];
            }
        }

        private void ButtonOpenLocal_Click(object sender, RoutedEventArgs e)
        {
            string path = AppConfigHelper.GetLocalFolder("Cache");
            Win32Helper.OpenFileWithShell(path);
        }

        private void ButtonToolsWnd_Click(object sender, RoutedEventArgs e)
        {
            
        }


        #endregion 事件处理

        private void LoadLanguages()
        {
            string path = $"{Environment.CurrentDirectory}\\Language";
            if (!Directory.Exists(path))
                return;

            DirectoryInfo curDir = new DirectoryInfo(path);
            var files = curDir.EnumerateFiles("*.json", SearchOption.TopDirectoryOnly);

            if (!files.Any())
                return;


            if (_languages == null)
                _languages = new List<string>();
            else
                _languages.Clear();

            foreach (var file in files)
            {
                {
                    _languages.Add(file.Name.Replace(".json", string.Empty));
                }
            }

            if (_languages.Count > 0)
            {
                comboboxLanguage.Items.Clear();
                comboboxLanguage.ItemsSource = null;
                comboboxLanguage.ItemsSource = _languages;

                if (!string.IsNullOrEmpty(AppCache.Instance.Configs.Language))
                    comboboxLanguage.SelectedItem = AppCache.Instance.Configs.Language;

            }
        }



    }
}
