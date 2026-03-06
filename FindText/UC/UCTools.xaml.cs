using FindText.Helpers;
using FindText.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FindText.UC
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UCTools : UserControl
    {
        public UCTools()
        {
            InitializeComponent();

            buttonSaveCache.Click += ButtonSaveCache_Click;
        }

        private void ButtonSaveCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(textboxTitle.Text))
                {
                    PopupHelper.ShowPopupMessage((Button)sender, "请输入文件标题");
                    return;
                }
                string txt = textboxInput.Text;
                if (string.IsNullOrEmpty(txt))
                {
                    PopupHelper.ShowPopupMessage((Button)sender, "请输入内容");
                    return;
                }
                string title = textboxTitle.Text;

                string[] strs = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                List<TextSearchResult> list = new List<TextSearchResult>();
                foreach (var s in strs)
                {
                    TextSearchResult fileResult = new TextSearchResult(){ FilePath = s ,Position = 1};
                    list.Add(fileResult);
                }

                string dir = AppConfigHelper.GetLocalFolder("Cache");
                string filename = $"{dir}\\Data_{title}_(cache).cache";
                string json = JsonHelper.ToJson(list);

                byte[] data = Utils.StringCompress.Compress(json);
                File.WriteAllBytes(filename, data);
                PopupHelper.ShowPopupMessage((Button)sender, "保存完毕");
            }
            catch (Exception ex)
            {
                PopupHelper.ShowPopupMessage((Button)sender, $"保存失败: {ex.Message}");
            }

        }

    }
}
