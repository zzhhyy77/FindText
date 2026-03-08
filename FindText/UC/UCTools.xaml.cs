using FindText.Helpers;
using FindText.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FindText.UC
{

    public partial class UCTools : UserControl
    {

        string _curPath;

        internal bool CacheChanged { get; set; }

        public UCTools()
        {
            InitializeComponent();
            CacheChanged = false;
            buttonSaveCache.Click += ButtonSaveCache_Click;
            buttonOpenCacheFolder.Click += ButtonOpenCacheFolder_Click;
            buttonOpenCache.Click += ButtonOpenCache_Click;
            buttonConvertCache.Click += ButtonConvertCache_Click;
        }


        private void ButtonSaveCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(textboxTitle.Text))
                {
                    PopupHelper.ShowPopupMessage((Button)sender, TextCache.Text["WT.TitleNull"]);
                    return;
                }
                string txt = textboxInput.Text;
                if (string.IsNullOrEmpty(txt))
                {
                    PopupHelper.ShowPopupMessage((Button)sender, TextCache.Text["WT.TextNull"]);
                    return;
                }
                string title = textboxTitle.Text;

                string[] strs = txt.Trim().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                List<TextSearchResult> list = new List<TextSearchResult>();
                foreach (var s in strs)
                {
                    if (!string.IsNullOrEmpty(s.Trim()))
                    {
                        TextSearchResult fileResult = new TextSearchResult() { FilePath = s, Position = 1 };
                        list.Add(fileResult);
                    }
                }

                if (list.Count <= 0)
                    return;

                string dir = AppConfigHelper.GetLocalFolder("Cache");
                string filename = $"{dir}\\Data_{title}_(cache).cache";
                string json = JsonHelper.ToJson(list);

                byte[] data = Utils.StringCompress.Compress(json);
                File.WriteAllBytes(filename, data);
                PopupHelper.ShowPopupMessage((Button)sender, TextCache.Text["Tool.CacheDir"]);
                CacheChanged = true;
            }
            catch (Exception ex)
            {
                PopupHelper.ShowPopupMessage((Button)sender, ex.Message);
            }

        }

        private void ButtonOpenCacheFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = AppConfigHelper.GetLocalFolder("Cache");
            Win32Helper.OpenFileWithShell(path);
        }

        private void ButtonOpenCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.InitialDirectory = $"{Environment.CurrentDirectory}\\Cache";
                dlg.Filter = $"{TextCache.Text["WT.OpenCache"]}|*.cache";

                if (dlg.ShowDialog(App.Current.MainWindow) == true)
                {

                    byte[] data = File.ReadAllBytes(dlg.FileName);

                    if (data == null)
                        return;

                    string json = string.Empty;
                    try
                    {
                        json = Utils.StringCompress.Decompress(data);
                        textboxJson.Text = json;
                        _curPath = dlg.FileName;

                        string filename = Path.GetFileName(_curPath).Trim();
                        string[] buf = filename.Split('_');
                        if (buf.Length >= 3)
                        {
                            textboxConvertTitle.Text = buf[1];
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message);
            }

        }

        private void ButtonConvertCache_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textboxJson.Text))
            {
                PopupHelper.ShowPopupMessage((Button)sender, TextCache.Text["WT.TextNull"]);
                return;
            }

            string title = textboxConvertTitle.Text;
            if (title.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                PopupHelper.ShowPopupMessage((Button)sender, TextCache.Text["WT.TitleError"]);
                return;
            }

            if (string.IsNullOrEmpty(textboxConvertTitle.Text))
            {
                PopupHelper.ShowPopupMessage((Button)sender, TextCache.Text["WT.TitleNull"]);
                return;
            }
            try
            {
               var tmp = JsonHelper.Parse<ObservableCollection<TextSearchResult>>(textboxJson.Text); //检查json是否正确
            }
            catch
            {
                PopupHelper.ShowPopupMessage((Button)sender, TextCache.Text["WT.JsonError"]);
                return;
            }

            try
            {
                string dir = AppConfigHelper.GetLocalFolder("Cache");

                byte[] data = Utils.StringCompress.Compress(textboxJson.Text);
                string filename = _curPath;

                if (string.IsNullOrEmpty(filename))
                    filename = $"{dir}\\Result_{title}_(Data).cache";

                //MessageBox.Show(filename);                 
                File.WriteAllBytes(filename, data);
                PopupHelper.ShowPopupMessage((Button)sender, $"{TextCache.Text["CM.Completed"]}\r\n{filename}");
                CacheChanged = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

    }
}
