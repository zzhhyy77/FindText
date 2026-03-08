using FindText.Helpers;
using FindText.Models;
using FindText.Workers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
        ObservableCollection<TextSearchResult>? _listCaches;
        ObservableCollection<TextSearchResult>? _results;

        string _lastFolder;
        bool _isEventEnable;
        int _curFlagType; //1 = 数据标签， 0 = 普通标签
        bool _isModifying; // 绑定数据源是否正在修改
        string _selNote; //进入编辑模式前的备注内容

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
            _curFlagType = 0;
            _isModifying = false;

            _results = new ObservableCollection<TextSearchResult>();
            _listCaches = new ObservableCollection<TextSearchResult>();
            listboxCache.ItemsSource = _listCaches;

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


            listboxCache.MouseDoubleClick += ListboxCache_MouseDoubleClick;
            listboxCache.SelectionChanged += ListboxCache_SelectionChanged;

            buttonCloseExtPanel.Click += (s, e) => { buttonGetExtName.IsChecked = false; };
            buttonHelpClose.Click += (s, e) => { DivHelp.Visibility = Visibility.Collapsed; };

            msgSelectedInfo.MouseDoubleClick += (s1, e1) => { msgSelectedInfo.SelectAll(); };

            buttonDataGridSaveAs.Click += ButtonDataGridSaveAs_Click;
            buttonDataGridSave.Click += ButtonDataGridSave_Click;
            buttonDataGridDelete.Click += ButtonDataGridDelete_Click;

        }

        #endregion 构造函数及初始化


        #region 事件处理

        private async void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(inputFolder.Text))
                {
                    PopupHelper.ShowPopupMessage(inputFolder, TextCache.Text["Err.FolderNull"]);
                    return;
                }

                if (!Directory.Exists(inputFolder.Text))
                {
                    PopupHelper.ShowPopupMessage(inputFolder, TextCache.Text["Err.NoFolder"]);
                }

                if (string.IsNullOrEmpty(inputPattern.Text))
                {
                    PopupHelper.ShowPopupMessage(inputPattern, TextCache.Text["Err.PatternNull"]);
                    ShowHelpInfo("Pattern");
                    return;
                }

                if (string.IsNullOrEmpty(inputSearchText.Text))
                {
                    PopupHelper.ShowPopupMessage(inputSearchText, TextCache.Text["Err.TextNull"]);
                    return;
                }

                AppCache.Instance.Configs.SearchOption = GetSearchOptions();
                string filename = AppConfigHelper.GetCacheFileName(inputSearchText.Text, AppCache.Instance.Configs.SearchOption.Path);      
                if (_listCaches != null && _listCaches.Where(x => x.FilePath == filename).Any())
                {
                    if (MessageWnd.Show(TextCache.Text["MSG.Title"], string.Format(TextCache.Text["FT.Again"], inputSearchText.Text)) != true)
                    {
                        return;
                    }
                }

                DivProgress.Visibility = Visibility.Visible;
                DivHelp.Visibility = Visibility.Collapsed;
                DivProgress.Visibility = Visibility.Visible;
                progressBar1.IsIndeterminate = true;
                textblockProgressInfo.Text = string.Empty;
                buttonCancel.IsEnabled = true;


                datagridResult.ItemsSource = null;

                //var watch = new Stopwatch();
                //watch.Start();

                await SearchTextAsync();

                //watch.Stop();
                //msgSelectedInfo.Text = $"{watch.ElapsedMilliseconds}"; //测试3关键字搜索， 正则比indexof慢不少

                buttonCancel.IsEnabled = false;
                DivProgress.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DivProgress.Visibility = Visibility.Collapsed;
                DivHelp.Visibility = Visibility.Visible;
                textHelp.Text = ex.Message;
            }
        }

        private void InputPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AppCache.Wildcard.Contains(inputPattern.Text) || inputPattern.Text.IndexOf("**") >= 0 || inputPattern.Text.IndexOf("..") >= 0)
            {
                if (DivIgnores.Visibility != Visibility.Visible)
                    DivIgnores.Visibility = Visibility.Visible;
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
            dialog.InitialDirectory = inputFolder.Text;
            if (dialog.ShowDialog() == true)
            {
                inputFolder.Text = dialog.FolderName;
                DirectoryInfo? parent = Directory.GetParent(dialog.FolderName);
                if (parent == null)
                {
                    PopupHelper.ShowPopupMessage(inputFolder, TextCache.Text["MSG.IsRoot"], 5);
                }
            }
        }

        private void ButtonGetExtName_Unchecked(object sender, RoutedEventArgs e)
        {
            if (listboxExtNames.Items.Count <= 0)
                return;

            try
            {
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
            catch (Exception ex)
            {
                DivHelp.Visibility = Visibility.Visible;
                textHelp.Text = ex.Message;
            }
        }

        private void ButtonGetExtName_Checked(object sender, RoutedEventArgs e)
        {
            if (_lastFolder == inputFolder.Text)
                return;
            try
            {
                if (!string.IsNullOrEmpty(inputFolder.Text))
                {
                    _lastFolder = inputFolder.Text;
                    GetAllExtNames();
                }
            }
            catch (Exception ex)
            {
                DivHelp.Visibility = Visibility.Visible;
                textHelp.Text = ex.Message;
            }
        }

        private void DatagridResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_curFlagType == 1)
                return;

            TextSearchResult? sel = datagridResult.SelectedItem as TextSearchResult;
            if (sel == null)
                return;

            if (string.IsNullOrEmpty(sel.FilePath))
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
            if (_isModifying) return; //正在改数据源         
            try
            {
                foreach (TextSearchResult item in e.RemovedItems)
                {
                    item.IsSelected = false;
                }

                foreach (TextSearchResult item in e.AddedItems)
                {
                    item.IsSelected = true;
                }

                if (datagridResult.SelectedItems == null || datagridResult.SelectedItems.Count <= 0)
                {
                    DivOpenSel.Visibility = Visibility.Collapsed;
                    DivDataGridButtons.Visibility = Visibility.Collapsed; 

                    return;
                }

                TextSearchResult data = (TextSearchResult)datagridResult.SelectedItem;
                if (_curFlagType == 1)
                {
                    msgSelectedInfo.Text = data.FilePath;
                    return; //数据标签
                }

                DivOpenSel.Visibility = Visibility.Visible;
                DivDataGridButtons.Visibility = Visibility.Visible;


                _selNote = data.Note; //用于判断是否需要保存

                if (datagridResult.SelectedItems.Count == 1)
                {
                    msgSelectedInfo.Text = data.FilePath;
                    buttonOpenSelection.Content = TextCache.Text["DG.OpenPath"];
                }
                else
                {
                    msgSelectedInfo.Text = $"{TextCache.Text["FT.SelCount"]}{datagridResult.SelectedItems.Count}";
                    buttonOpenSelection.Content = TextCache.Text["DG.OpenSel"];
                }

                switch ($"{datagridResult.CurrentCell.Column.SortMemberPath}")
                {
                    case "Note":
                        datagridResult.BeginEdit();                    
                        break;
                }

            }
            catch (Exception ex)
            {
                DivHelp.Visibility = Visibility.Visible;
                textHelp.Text = ex.Message;
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

        private void InputResultFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_results == null || _results.Count <= 0)
                return;

            try
            {
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
            catch { }
        }

        private void ListboxCache_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listboxCache.SelectedItems.Count <= 0)
                return;

            try
            {
                buttonDataGridSave.IsEnabled = false;
                TextSearchResult? fr = listboxCache.SelectedItem as TextSearchResult;
                if (fr == null)
                    return;

                if (fr.FilePath.IndexOf("data_", StringComparison.InvariantCultureIgnoreCase) >= 0 && fr.FilePath.IndexOf("(cache", StringComparison.InvariantCultureIgnoreCase) > 0)
                {
                    DivOpenSel.Visibility = Visibility.Collapsed;
                    DivDataGridButtons.Visibility = Visibility.Collapsed; 
                    _curFlagType = 1;
                }
                else
                {
                    DivOpenSel.Visibility = Visibility.Visible;
                    DivDataGridButtons.Visibility = Visibility.Visible;
                    _curFlagType = 0;
                }

                if (_isEventEnable == false) //保存缓存的过程中为false
                    return;

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
            catch (Exception ex)
            {
                DivHelp.Visibility = Visibility.Visible;
                textHelp.Text = ex.Message;
            }
        }

        //逻辑不合理，已取消
        private void ListboxCache_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*
            重新搜相同内容应手工收入，且给个提示，不应该提供方便

            if (_curFlagType == 1)
                return;

            if (listboxCache.SelectedItem == null)
                return;

            TextSearchResult? fr = listboxCache.SelectedItem as TextSearchResult;
            if (fr == null)
                return;

            try
            {
                inputSearchText.Text = fr.Title;
            }
            catch (Exception ex)
            {
                DivHelp.Visibility = Visibility.Visible;
                textHelp.Text = ex.Message;
            }
            */
        }

        private void ButtonDataGridSaveAs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sel = listboxCache.SelectedItem as TextSearchResult;
                if (sel == null) return;
                if (_results == null || _results.Count <= 0) return;
                if (string.IsNullOrEmpty(sel.FilePath)) return;
                if (!File.Exists(sel.FilePath)) return;
                if (string.IsNullOrEmpty(sel.Title)) return;

                string? dir = Path.GetDirectoryName(sel.FilePath);
                string newName = $"{dir}\\Data_{sel.Title}_(Cache).cache";

                if (MessageWnd.Show(TextCache.Text["MSG.Title"], TextCache.Text["MSG.SaveAs"]) == true)
                {
                    File.Move(sel.FilePath, newName);
                    LoadCaches();
                }
            }
            catch(Exception ex)
            {
                PopupHelper.ShowPopupMessage((Button)sender,ex.Message);
            }
        }

        private void ButtonDataGridSave_Click(object sender, RoutedEventArgs e)
        {
            var sel = listboxCache.SelectedItem as TextSearchResult;
            if (sel == null) return;

            SaveResult(sel.Title, sel.FilePath);
            buttonDataGridSave.IsEnabled = false;

        }

        private void ButtonDataGridDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //先切换到备注列，避免执行双击动作
                DataGridCellInfo dci = new DataGridCellInfo(datagridResult.CurrentCell, this.ColumnNote);
                datagridResult.CurrentCell = dci;

                if (datagridResult.SelectedItems == null)
                    return;

                int index = datagridResult.SelectedIndex;

                _isModifying = true;

                for (int i = datagridResult.SelectedItems.Count - 1; i >= 0; i--)
                {
                    _results.Remove((TextSearchResult)datagridResult.SelectedItems[i]);
                }
                _isModifying = false;

                if (_results.Count <= 0)
                {
                    DivDataGridButtons.Visibility = Visibility.Collapsed;
                    buttonDataGridSave.IsEnabled = false; 

                }
                else
                {
                    buttonDataGridSave.IsEnabled = true; //只一个按钮需要控制状态，没必要用Command消耗性能
                    if (_results.Count> index)
                        datagridResult.SelectedIndex = index;
                    else
                        datagridResult.SelectedIndex = _results.Count-1;

                }

            }
            catch { }
        }

        //Xaml绑定的事件
        private void ListItemButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextSearchResult? fr = ((Button)sender).Tag as TextSearchResult;
                if (fr == null)
                    return;

                if (fr.FilePath.IndexOf("Data_") > 0 && fr.FilePath.IndexOf("(cache)") > 0)
                {
                    return;
                }

                int idx = listboxCache.SelectedIndex;
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
                        _listCaches?.Remove(fr);
                        _results?.Clear();
                        if(idx>=0)
                            listboxCache.SelectedIndex = (idx - 1);
                    }
                };
                PopupHelper.ShowPopupMessage((Button)sender, buttonOK, 2);
            }
            catch (Exception ex)
            {
                DivHelp.Visibility = Visibility.Visible;
                textHelp.Text = ex.Message;
            }
        }

        private void editboxNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (buttonDataGridSave.IsEnabled)
                return;

            TextBox? tb = sender as TextBox;
            if (tb == null)
                return;

            if ($"{_selNote}" != $"{tb.Text}")
                buttonDataGridSave.IsEnabled = true;
            else
                buttonDataGridSave.IsEnabled = false;
        }


        #endregion 事件处理

        #region Commands

        //全局命令
        private void HelpCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            ShowHelpInfo($"{e.Parameter}");
        }

        private void OpenCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                switch ($"{e.Parameter}")
                {
                    case "Cache":
                        OpenToolWindow();
                        break;
                    case "Reload":
                        LoadCaches();
                        break;                       
                }
            }
            catch { }
        }

        private void CanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion

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

            List<string> ignores = ignoreStr.Split(AppCache.SeparatorComma, StringSplitOptions.RemoveEmptyEntries).ToList();

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
            _isEventEnable = false;
            listboxCache.ItemsSource = null; 
            _listCaches.Clear();
            _results.Clear(); 

            int maxsize = 10 * 1024 * 1024; //超过10M的忽略
            int maxCount = 20; //最多只载入20个

            BackgroundWorker bw = new BackgroundWorker() { WorkerReportsProgress = false, WorkerSupportsCancellation = false };
            bw.DoWork += (s1, e1) =>
            {
                string dir = AppConfigHelper.GetLocalFolder("Cache");
                List<TextSearchResult> files = GetAllFilesEx(dir, "*.cache", maxsize);

                //用Position 区分标签类别 ，0 = 查找结果，1 = 标签数据
                //标签数据放前面，数据量小，用两次循环影响不大
                foreach (var file in files)
                {
                    string filename = Path.GetFileName(file.FilePath).Trim();
                    if (!filename.StartsWith("Data_"))
                        continue;

                    string[] buf = filename.Split('_');
                    if (buf.Length >= 3)
                    {
                        Application.Current.Dispatcher.Invoke(() => _listCaches.Add(new TextSearchResult() { Title = buf[1], FilePath = file.FilePath, Position = 1 }));
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
                        Application.Current.Dispatcher.Invoke(() => _listCaches.Add(new TextSearchResult() { Title = buf[1], FilePath = item.FilePath }));
                    }
                    maxCount--;
                    if (maxCount <= 0)
                        break;
                }
            };

            bw.RunWorkerCompleted += (s1, e1) =>
            {
                _isEventEnable = true;
                if (e1.Error != null)
                {

                }
                else
                {
                    listboxCache.ItemsSource = _listCaches;
                }
            };
            bw.RunWorkerAsync();
        }

        private void SaveResult(string title,string fileName = "")
        {

            /*
            提取到 AppConfigHelper.GetCacheFileName
            string dir = AppConfigHelper.GetLocalFolder("Cache");
            string folder = AppCache.Instance.Configs.SearchOption.Path.Replace(":\\", "-");
            folder = folder.Replace("\\", "-");

            if (title.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                string guid = Guid.NewGuid().ToString("N");
                guid = guid.Substring(0, 16);
                title = $"{guid}";
            }
            string name = fileName;

            if (string.IsNullOrEmpty(name))
                name = $"{dir}\\Result_{title}_({folder}).cache";
            */

            string name = fileName;
            if (string.IsNullOrEmpty(name))
                name = AppConfigHelper.GetCacheFileName(title,AppCache.Instance.Configs.SearchOption.Path);

            string json = JsonHelper.ToJson(_results);
            byte[] data = Utils.StringCompress.Compress(json);
            //string title = AppCache.Instance.Configs.SearchOption.SearchText;


            File.WriteAllBytes(name, data);

            //AppConfigHelper.SaveConfig("Cache", filename, json);

            var item = _listCaches.Where(x => x.FilePath == name);
            if (item.Any())
            {
                listboxCache.SelectedItem = item.First();
            }
            else
            {
                int idx = _listCaches.Where(x => x.Position == 1).Count();
                TextSearchResult sr = new TextSearchResult() { Title = title, FilePath = $"{name}" };
                _listCaches?.Insert(idx, sr);
                _isEventEnable = false;
                listboxCache.SelectedItem = sr;
                _isEventEnable = true;
            }
        }

        private async Task SearchTextAsync()
        {
            _lastTag = string.Empty;
            _cancelToken = new CancellationTokenSource();
            _isbusying = true;

            var notes = _results?.Where(x=>!string.IsNullOrEmpty(x.Note)); //缓存备注，以免重新搜索丢缓存

            _results = null;
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
                {
                    //搜索完后填回备注
                    if (notes != null && notes.Any())
                    {
                        foreach (var tsr in _results)
                        {
                            foreach (var item in notes)
                            {
                                if (tsr.FilePath == item.FilePath)
                                {
                                    tsr.Note = item.Note;
                                }
                            }
                        }
                    }

                    if(checkBoxRegex.IsChecked==false) //正则不自动清空
                        inputSearchText.Text = string.Empty;

                    SaveResult(AppCache.Instance.Configs.SearchOption.SearchText,string.Empty);
                }
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
                    SaveResult(AppCache.Instance.Configs.SearchOption.SearchText, string.Empty); 

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

        private void OpenToolWindow()
        {

            Window wnd = new Window();
            wnd.Owner = Application.Current.MainWindow;
            wnd.Height = Application.Current.MainWindow.Height;
            wnd.Width = Application.Current.MainWindow.Width;
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wnd.Title = $"{TextCache.Text["Main.Title"]} - {TextCache.Text["Tool.ToolsWnd"]}";
            UCTools uc = new UCTools();
            wnd.Content = uc;
            wnd.Closed += (s1, e1) =>
            {
                if (uc.CacheChanged)
                {
                    LoadCaches();
                }
            };
            wnd.ShowDialog();
        }

     

        #endregion 方法

    }

}
