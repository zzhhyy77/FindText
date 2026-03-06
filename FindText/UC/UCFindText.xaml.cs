using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FindText.Helpers;
using FindText.Models;
using FindText.Workers;
using Microsoft.Win32;

namespace FindText.UC
{
    /// <summary>
    /// UCFindText.xaml 的交互逻辑
    /// </summary>
    public partial class UCFindText : UserControl
    {

        #region 事件

        public event EventHandler<EventArgs> CacheChanged;

        #endregion

        #region 字段

        CancellationTokenSource _cancelToken;
        bool _isbusying;
        string _lastTag;

        ObservableCollection<TextSearchResult>? _listHistory;
        ObservableCollection<TextSearchResult>? _results;

        string _lastFolder;
        bool _isEventEnable;

        #endregion 字段


        #region 属性


        #endregion 属性


        #region 构造函数及初始化

        public UCFindText()
        {
            InitializeComponent();
            InitializeMembers();
            InitializeConfigs();
            InitializeEvents();
        }

        private void InitializeMembers()
        {
            _isbusying = false;
            _lastFolder = string.Empty;
            _isEventEnable = true;

            _results = new ObservableCollection<TextSearchResult>();
            _listHistory = new ObservableCollection<TextSearchResult>();
            listboxHistory.ItemsSource = _listHistory;

            #region 手工排序,常用的放前面

            List<KeyValue> bm = new List<KeyValue>();
            bm.Add(new KeyValue() { Key = "Auto", Value = TextCache.Text["BM.Auto"] });
            bm.Add(new KeyValue() { Key = "gb2312", Value = TextCache.Text["BM.GB2312"] });
            bm.Add(new KeyValue() { Key = "utf-8", Value = "UTF-8" });
            bm.Add(new KeyValue() { Key = "unicode", Value = "Unicode" });
            bm.Add(new KeyValue() { Key = "ascii", Value = "ASCII" });
            bm.Add(new KeyValue() { Key = "gbk", Value = "GBK" });
            bm.Add(new KeyValue() { Key = "big5", Value = "Big5" });

            string[] names = { "gb2312", "utf-8", "unicode", "ascii", "gbk", "big5" };
            var list = Encoding.GetEncodings().Where(x => !names.Contains(x.Name.ToLower())).OrderBy(x => x.Name);

            foreach (EncodingInfo eni in list)
            {
                bm.Add(new KeyValue() { Key = $"{eni.CodePage}", Value = eni.Name });
            }

            #endregion

            comboxEncoding.ItemsSource = bm;
            comboxEncoding.SelectedIndex = 0;

            LoadCaches();
        }

        private void InitializeConfigs()
        {
            if (AppCache.Instance.Configs.SearchOption != null)
            {
                SetSearchOptions(AppCache.Instance.Configs.SearchOption);
            }
        }

        private void InitializeEvents()
        {
            //DivHelp.MouseUp += DivHelp_MouseUp;

            buttonFind.Click += ButtonFind_Click;
            inputPattern.TextChanged += InputPattern_TextChanged;
            buttonSelectDir.Click += ButtonSelectDir_Click;
            buttonCancel.Click += (s, e) => { if (_isbusying) { _cancelToken?.Cancel(); } };

            buttonGetExtName.Checked += ButtonGetExtName_Checked;
            buttonGetExtName.Unchecked += ButtonGetExtName_Unchecked;

            datagridResult.MouseDoubleClick += DatagridResult_MouseDoubleClick;
            datagridResult.SelectionChanged += DatagridResult_SelectionChanged;            

            inputResultFilter.TextChanged += InputResultFilter_TextChanged;
            buttonOpenSelection.Click += ButtonOpenSelection_Click;


            listboxHistory.MouseDoubleClick += ListboxHistory_MouseDoubleClick;
            listboxHistory.SelectionChanged += ListboxHistory_SelectionChanged;

            buttonCloseExtPanel.Click += (s, e) => { buttonGetExtName.IsChecked = false; };
            buttonHelpClose.Click += (s, e) => { DivHelp.Visibility = Visibility.Collapsed; };
        
        }

        #endregion 构造函数及初始化


        #region 事件处理

        private async void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(inputFolder.Text))
            {
                PopupHelper.ShowPopupMessage(inputFolder, "请选择目标文件夹");
                return;
            }

            if (!Directory.Exists(inputFolder.Text))
            {
                PopupHelper.ShowPopupMessage(inputFolder, "文件夹不存在");
            }

            if (string.IsNullOrEmpty(inputPattern.Text))
            {
                PopupHelper.ShowPopupMessage(inputPattern, "请输入文件类型");
                ShowHelpInfo("Pattern");
                return;
            }

            if (string.IsNullOrEmpty(inputSearchText.Text))
            {
                PopupHelper.ShowPopupMessage(inputSearchText, "请输入搜索内容");
                return;
            }

            DivProgress.Visibility = Visibility.Visible;
            DivHelp.Visibility = Visibility.Collapsed;
            DivProgress.Visibility = Visibility.Visible;
            progressBar1.IsIndeterminate = true;
            textblockProgressInfo.Text = string.Empty;
            buttonCancel.IsEnabled = true;

            AppCache.Instance.Configs.SearchOption = GetSearchOptions();
            _results = null;
            datagridResult.ItemsSource = null;

            var watch = new Stopwatch();
            watch.Start();

            await SearchTextAsync();

            watch.Stop();
            msgSelectedInfo.Text = $"{watch.ElapsedMilliseconds}"; //正则 比 indexof慢不少

            buttonCancel.IsEnabled = false;
            DivProgress.Visibility = Visibility.Collapsed;
        }

        private void InputPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AppCache.Wildcard.Contains(inputPattern.Text) || inputPattern.Text.IndexOf("**") >= 0 || inputPattern.Text.IndexOf("..") >= 0)
            {
                if (DivIgnores.Visibility != Visibility.Visible)
                    DivIgnores.Visibility = Visibility.Visible;
                //SetStoryboard(DivParams, 0.5, 1);
            }
            else
            {
                if (DivIgnores.Visibility == Visibility.Visible)
                    DivIgnores.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonSelectDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                inputFolder.Text = dialog.FolderName;
            }
        }

        private void ButtonGetExtName_Unchecked(object sender, RoutedEventArgs e)
        {
            if (listboxExtNames.Items.Count <= 0)
                return;

            string str = string.Empty;
            int i = 0;
            bool frist = true;
            foreach (SelectedValue item in listboxExtNames.ItemsSource)
            {
                if (item == null)
                    break;

                if (i > 30)
                    break;

                if (item.IsSelected)
                {
                    if (!frist)
                        str += ",";
                    str += $"*{item.Value}";
                    frist = false;
                }
                i++;
            }

            if (string.IsNullOrEmpty(str))
            {
                inputIgnores.Text = string.Empty;
            }
            else
            {
                inputIgnores.Text = str;
            }
        }

        private void ButtonGetExtName_Checked(object sender, RoutedEventArgs e)
        {
            if (_lastFolder == inputFolder.Text)
                return;

            if (!string.IsNullOrEmpty(inputFolder.Text))
            {
                _lastFolder = inputFolder.Text;
                GetAllExtNames();
            }
        }

        private void HelpCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            ShowHelpInfo($"{e.Parameter}");
        }

        private void CanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DatagridResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextSearchResult? sel = datagridResult.SelectedItem as TextSearchResult;
            if (sel == null)
                return;

            try
            {
                string colName = $"{datagridResult.CurrentCell.Column.SortMemberPath}";

                switch (colName)
                {
                    case "FilePath":
                        Win32Helper.OpenFileWithShell(sel.FilePath);
                        break;

                    case "Note":

                        break;
                }
            }
            catch (Exception ex)
            {
                DivHelp.Visibility = Visibility.Visible;
                textHelp.Text = ex.Message;
            }
        }

        private void DatagridResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datagridResult.SelectedItems == null || datagridResult.SelectedItems.Count <= 0)
            {
                DivOpenSel.Visibility = Visibility.Collapsed;
                return;
            }

            DivOpenSel.Visibility = Visibility.Visible;

            if (datagridResult.SelectedItems.Count == 1)
            {
                TextSearchResult? sel = datagridResult.SelectedItem as TextSearchResult;
                if (sel == null)
                {
                    DivOpenSel.Visibility = Visibility.Collapsed;
                    return;
                }

                msgSelectedInfo.Text = sel.FilePath;
                buttonOpenSelection.Content = TextCache.Text["FT.OpenPath"];
            }
            else
            {
                msgSelectedInfo.Text = $"{TextCache.Text["FT.SelCount"]}{datagridResult.SelectedItems.Count}";
                buttonOpenSelection.Content = TextCache.Text["FT.OpenSel"];
            }
        }

        private void ButtonOpenSelection_Click(object sender, RoutedEventArgs e)
        {
            if (datagridResult.SelectedItems.Count <= 0)
                return;

            if (string.IsNullOrEmpty(msgSelectedInfo.Text))
                return;

            try
            {
                if (datagridResult.SelectedItems.Count == 1)
                {
                    string filePath = msgSelectedInfo.Text;
                    if (File.Exists(filePath))
                    {
                        Win32Helper.OpenFileWithShell("explorer.exe", $"/select,\"{filePath}\"");
                    }
                }
                else
                {
                    if (datagridResult.SelectedItems.Count > 30)
                    {
                        PopupHelper.ShowPopupMessage((Button)sender, TextCache.Text["MSG.MaxCount"], 5);
                        return;
                    }

                    if (datagridResult.SelectedItems.Count > 10)
                    {
                        if (MessageBox.Show(App.Current.MainWindow, TextCache.Text["MSG.MsgCount"], TextCache.Text["MSG.Title"], MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    List<string> filePaths = new List<string>();
                    foreach (var item in datagridResult.SelectedItems)
                    {
                        TextSearchResult? fr = item as TextSearchResult;
                        if (fr == null)
                            continue;

                        if (File.Exists(fr.FilePath))
                        {
                            //System.Diagnostics.Process.Start(fr.FilePath);
                            Win32Helper.OpenFileWithShell(fr.FilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DivHelp.Visibility = Visibility.Visible;
                textHelp.Text = ex.Message;
            }
        }

        private void DivParams_MouseLeave(object sender, MouseEventArgs e)
        {
            SetStoryboard(DivParams, 1, 0.5);
        }

        private void DivParams_MouseEnter(object sender, MouseEventArgs e)
        {
            SetStoryboard(DivParams, 0.5, 1);
        }

        private void InputResultFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_results == null || _results.Count <= 0)
                return;

            if (string.IsNullOrEmpty(inputResultFilter.Text))
            {
                datagridResult.ItemsSource = null;
                datagridResult.ItemsSource = _results;
            }
            else
            {
                datagridResult.ItemsSource = null;
                string str = this.inputResultFilter.Text.ToLower();
                datagridResult.ItemsSource = _results.Where(x => x.FilePath.ToLower().IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        private void ListboxHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isEventEnable == false)
                return;

            if (listboxHistory.SelectedItems.Count <= 0)
            {
                return;
            }

            TextSearchResult? fr = listboxHistory.SelectedItem as TextSearchResult;
            if (fr == null)
                return;

            if (fr.FilePath.IndexOf("Data_") > 0 && fr.FilePath.IndexOf("(cache") > 0)
            {
                buttonOpenSelection.Visibility = Visibility.Collapsed;
            }
            else
            {
                buttonOpenSelection.Visibility = Visibility.Visible;
            }

            byte[] data = File.ReadAllBytes(fr.FilePath);

            if (data == null)
                return;

            string json = string.Empty;
            try
            {
                json = Utils.StringCompress.Decompress(data);
            }
            catch { }

            _results = null;
            _results = JsonHelper.Parse<ObservableCollection<TextSearchResult>>(json);
            inputResultFilter.Clear();
            datagridResult.ItemsSource = _results;
        }

        private void ListboxHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listboxHistory.SelectedItem == null)
                return;

            TextSearchResult? fr = listboxHistory.SelectedItem as TextSearchResult;
            if (fr == null)
                return;

            inputSearchText.Text = fr.Title;
        }

        //在Xaml绑定的事件
        private void ListItemButton_Click(object sender, RoutedEventArgs e)
        {
            TextSearchResult? fr = ((Button)sender).Tag as TextSearchResult;
            if (fr == null)
                return;

            if (fr.FilePath.IndexOf("Data_") > 0 && fr.FilePath.IndexOf("(cache)") > 0)
            {
                return;
            }

            Button buttonOK = new Button();
            buttonOK.Content = "❌";
            buttonOK.FontSize = 16;
            buttonOK.Width = 48;
            buttonOK.Height = 26;
            buttonOK.Padding = new Thickness(0);
            buttonOK.Margin = new Thickness(0);
            //buttonOK.Style = App.Current.Resources["ellipseButtonStyle"] as Style;
            buttonOK.Foreground = App.Current.Resources["RedBrush"] as SolidColorBrush;
            buttonOK.Background = App.Current.Resources["TransparentBrush"] as SolidColorBrush;
            buttonOK.Click += (ss, ee) =>
            {
                if (File.Exists(fr.FilePath))
                {
                    File.Delete(fr.FilePath);
                    _listHistory?.Remove(fr);
                }
            };
            PopupHelper.ShowPopupMessage((Button)sender, buttonOK, 2);
        }


        #endregion 事件处理


        #region 方法

        public TextSearchOption GetSearchOptions()
        {
            TextSearchOption rev = new TextSearchOption()
            {
                Path = inputFolder.Text,
                Pattern = inputPattern.Text,
                SearchText = inputSearchText.Text,
                MaxSize = inputMaxSize.Value,
                Ignores = inputIgnores.Text,
                EncodingName = string.Empty,
                IsAllFiles = false,
                IsCaseSensitive = checkboxCaseSensitive.IsChecked.Value,
                IsIgnoreError = checkBoxIgnoreError.IsChecked.Value,
                //FileSearchOption = SearchOption.AllDirectories,
                IsRegex = checkBoxRegex.IsChecked.Value
            };

            if (comboxEncoding.SelectedIndex > 0)
            {
                KeyValue sel = comboxEncoding.SelectedValue as KeyValue;
                if (sel != null)
                {
                    rev.EncodingName = sel.Key;
                }
            }

            //任务里只以Ignores是否为空判断
            if (DivIgnores.Visibility == Visibility.Visible)
                rev.IsAllFiles = true;

            return rev;
        }

        private void SetSearchOptions(TextSearchOption value)
        {
            if (value == null) throw new ArgumentNullException("value");
            inputFolder.Text = value.Path;
            inputPattern.Text = value.Pattern;
            inputIgnores.Text = value.Ignores;
            inputMaxSize.Value = value.MaxSize;
            checkboxCaseSensitive.IsChecked = value.IsCaseSensitive;
            checkBoxIgnoreError.IsChecked = value.IsIgnoreError;
            checkBoxRegex.IsChecked = value.IsRegex;
            //inputSearchText.Text = value.SearchText;

            if (string.IsNullOrEmpty(value.EncodingName))
            {
                comboxEncoding.SelectedIndex = 0;
            }
            else
            {
                foreach (KeyValue item in comboxEncoding.Items)
                {
                    if (item.Key == value.EncodingName)
                    {
                        comboxEncoding.SelectedItem = item;
                        break;
                    }
                }
            }
            if (AppCache.Wildcard.Contains(value.Pattern))
            {
                DivIgnores.Visibility = Visibility.Visible;
            }
            else
            {
                DivIgnores.Visibility = Visibility.Collapsed;
            }
        }

        private void SetStoryboard(UIElement element, double from, double to)
        {
            try
            {
                if (element.Opacity == to)
                    return;

                Duration duration = new Duration(TimeSpan.FromMilliseconds(200));
                DoubleAnimation da1 = new DoubleAnimation() { Duration = duration, From = from, To = to };
                Storyboard sb = new Storyboard() { Duration = duration };
                sb.Children.Add(da1);
                Storyboard.SetTarget(da1, element);
                Storyboard.SetTargetProperty(da1, new PropertyPath(UIElement.OpacityProperty));
                sb.Completed += (sender1, e1) =>
                {
                    sb.Children.Clear();
                };
                sb.Begin();
            }
            catch { }
        }

        private void ShowHelpInfo(string tag)
        {
            if (_lastTag == tag)
            {
                _lastTag = string.Empty;
                DivHelp.Visibility = Visibility.Collapsed;
                textHelp.Text = string.Empty;
            }
            else
            {
                _lastTag = tag;
                string str = TextCache.Text[$"Help.{tag}"];
                if (!string.IsNullOrEmpty(str))
                {
                    DivHelp.Visibility = Visibility.Visible;
                    textHelp.Text = str;
                }
            }
        }

        private List<TextSearchResult> GetAllFilesEx(string folderPath, string filter, int maxSize, string ignoreStr = "")
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                throw new FileNotFoundException(folderPath);

            if (string.IsNullOrEmpty(filter))
                throw new ArgumentNullException(filter);

            string[] defaultIgnores = AppCache.Instance.DefaultIgnores.Replace(" ", string.Empty).Split(AppCache.SeparatorComma, StringSplitOptions.RemoveEmptyEntries);

            List<string> ignores = new List<string>(ignoreStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));

            DirectoryInfo di = new DirectoryInfo(folderPath);

            var dirs = DirectoryHelper.GetAllAccessibleDirectories(folderPath);

            int count = 0;
            List<TextSearchResult> rev = new List<TextSearchResult>();
            foreach (var dir in dirs)
            {
                count++;
                try
                {
                    DirectoryInfo curDir = new DirectoryInfo(dir);
                    var tmp = curDir.EnumerateFiles(filter, SearchOption.TopDirectoryOnly);
                    if (tmp.Any())
                    {
                        foreach (var f in tmp)
                        {
                            if (f.Length <= maxSize && !defaultIgnores.Contains(f.Extension))
                            {
                                if (ignores == null || ignores.Count <= 0)
                                {
                                    rev.Add(new TextSearchResult() { FilePath = f.FullName, Title = f.Extension, Size = f.Length, LastDate = f.LastWriteTime });
                                }
                                else
                                {
                                    if (!ignores.Contains(f.Extension))
                                        rev.Add(new TextSearchResult() { FilePath = f.FullName, Title = f.Extension, Size = f.Length, LastDate = f.LastWriteTime });
                                }
                            }
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
            return rev.OrderByDescending(x => x.LastDate).ToList();
        }

        public void GetAllExtNames()
        {
            string curVals = inputIgnores.Text;
            int maxsize = inputMaxSize.Value * 1024 * 1024;
            ObservableCollection<SelectedValue> list = new ObservableCollection<SelectedValue>();
            listboxExtNames.ItemsSource = null;

            //一次性任务，没必要写一个Worker
            BackgroundWorker bw = new BackgroundWorker() { WorkerReportsProgress = false, WorkerSupportsCancellation = false };
            bw.DoWork += (s1, e1) =>
            {
                List<TextSearchResult> files = GetAllFilesEx(_lastFolder, "*", maxsize);

                foreach (TextSearchResult file in files)
                {
                    if (!list.Where(x => x.Value == file.Title).Any())
                    {
                        SelectedValue val = new SelectedValue() { Value = file.Title };
                        if (curVals.IndexOf(file.Title) >= 0)
                            val.IsSelected = true;

                        list.Add(val);
                    }
                }
            };
            bw.RunWorkerCompleted += (s1, e1) =>
            {
                if (e1.Error != null)
                {
                    //switch (e1.Error)
                    //{
                    //    case UnauthorizedAccessException:
                    //        MessageBox.Show(App.Current.MainWindow, $"{e1.Error.Message}\r\n请检查文件夹权限 或 缩小搜索范围再试", "无访问权限");
                    //        break;

                    //    case FileNotFoundException:
                    //        MessageBox.Show(App.Current.MainWindow, $"{e1.Error.Message}\r\n请检查文件夹路径是否正确", "文件夹不存在");
                    //        break;

                    //    case Exception:
                    //        MessageBox.Show(App.Current.MainWindow, e1.Error.Message, "查找失败");
                    //        break;

                    //}
                }
                else
                {
                    listboxExtNames.ItemsSource = null;
                    listboxExtNames.ItemsSource = list.OrderBy(x => x.Value);
                }
            };
            bw.RunWorkerAsync();
        }

        private void LoadCaches()
        {
            listboxHistory.ItemsSource = null;
            _listHistory.Clear();
            int maxsize = 10 * 1024 * 1024; //超过10M的忽略
            int maxCount = 20; //最多只载入20个

            BackgroundWorker bw = new BackgroundWorker() { WorkerReportsProgress = false, WorkerSupportsCancellation = false };
            bw.DoWork += (s1, e1) =>
            {
                string dir = AppConfigHelper.GetLocalFolder("Cache");
                List<TextSearchResult> files = GetAllFilesEx(dir, "*.cache", maxsize);
                foreach (var file in files)
                {
                    string filename = Path.GetFileName(file.FilePath).Trim();
                    if (!filename.StartsWith("Data_"))
                        continue;

                    string[] buf = filename.Split('_');
                    if (buf.Length >= 3)
                    {
                        Application.Current.Dispatcher.Invoke(() => _listHistory.Add(new TextSearchResult() { Title = buf[1], FilePath = file.FilePath, Position = 1 }));
                    }
                    if (maxCount <= 0)
                        break;
                }

                foreach (var item in files)
                {
                    string filename = Path.GetFileName(item.FilePath);
                    if (filename.StartsWith("Data_"))
                        continue;

                    string str = Path.GetFileNameWithoutExtension(item.FilePath);
                    string[] buf = str.Split('_');
                    if (buf.Length >= 3)
                    {
                        Application.Current.Dispatcher.Invoke(() => _listHistory.Add(new TextSearchResult() { Title = buf[1], FilePath = item.FilePath }));
                    }
                    maxCount--;
                    if (maxCount <= 0)
                        break;
                }
            };

            bw.RunWorkerCompleted += (s1, e1) =>
            {
                if (e1.Error != null)
                {

                }
                else
                {
                    listboxHistory.ItemsSource = _listHistory;
                }
            };
            bw.RunWorkerAsync();
        }

        private void SaveResult()
        {
            string dir = AppConfigHelper.GetLocalFolder("Cache");
            string folder = AppCache.Instance.Configs.SearchOption.Path.Replace(":\\", "：＼");
            folder = folder.Replace("\\", "＼");

            string json = JsonHelper.ToJson(_results);

            byte[] data = Utils.StringCompress.Compress(json);
            string title = AppCache.Instance.Configs.SearchOption.SearchText.Replace("\\", "＼");
            string filename = $"{dir}\\Result_{title}_({folder}).cache";

            File.WriteAllBytes(filename, data);

            //AppConfigHelper.SaveConfig("Cache", filename, json);

            var item = _listHistory.Where(x => x.Title == title);
            if (item.Any())
            {
                listboxHistory.SelectedItem = item.First();
            }
            else
            {
                int idx = _listHistory.Where(x=>x.Position==1).Count() ;
                TextSearchResult sr = new TextSearchResult() { Title = title, FilePath = $"{filename}" };
                _listHistory?.Insert(idx,sr);
                _isEventEnable = false;
                listboxHistory.SelectedItem = sr;
                _isEventEnable = true;
            }
        }

        private async Task SearchTextAsync()
        {
            _lastTag = string.Empty;
            _cancelToken = new CancellationTokenSource();
            _isbusying = true;
            List<TextSearchResult> results = new List<TextSearchResult>();

            WorkerBase worker = new TextSearchWorker(AppCache.Instance.Configs.SearchOption);

            //这么写看起来别扭，但在Completed里绑定不能实时显示结果列表
            _results = (ObservableCollection<TextSearchResult>)worker.ReturnValue;
            datagridResult.ItemsSource = _results;

            worker.ProgressChanged += info =>
            {
                if (info.Current == -1)
                {
                    progressBar1.Visibility = Visibility.Visible;
                    textblockProgressTitle.Text = info.Message;
                }
                else if (info.Current > 0)
                {
                    if (progressBar1.IsIndeterminate == true)
                        progressBar1.IsIndeterminate = false;

                    if (info.Current == 1)
                        progressBar1.Maximum = info.Total;

                    progressBar1.Value = info.Current;
                    textblockProgressInfo.Text = $"{info.Current}/{info.Total} ({info.Percent}%)";
                    textblockProgressTitle.Text = info.Message;
                }
            };

            worker.Completed += (object rev) =>
            {
                if (_results.Count > 0)
                    SaveResult();
            };

            worker.ErrorOccurred += ex =>
            {
                DivHelp.Visibility = Visibility.Visible;
                textHelp.Text = $"{ex.Message}";

            };

            worker.Cancelled += () =>
            {
                progressBar1.Value = 0;
                _cancelToken.Cancel();
                PopupHelper.ShowPopupMessage(inputSearchText, TextCache.Text["Task.Canceled"]);

                if (_results.Count > 0)
                    SaveResult();
            };

            try
            {
                await worker.ExecuteAsync(_cancelToken.Token);

            }
            catch (OperationCanceledException ex)
            {
                textblockProgressTitle.Text = ex.Message;

            }
            catch (Exception ex)
            {
                textblockProgressTitle.Text = ex.Message;

            }
            finally
            {
                _isbusying = false;

            }
        }


        #endregion 方法

    }

}
