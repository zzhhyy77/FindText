using FindText.Helpers;
using FindText.Themes;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace FindText
{
    public partial class MainWindow : Window
    {

        #region 字段

        BlurEffect _blurEffect;
        bool _isSearching = false;
        bool _caseSensitive;
        bool _wholeWord;
        double _settiingPanelWidth;

     


        #endregion 字段


        #region 属性



        #endregion 属性


        #region 构造函数及初始化

        public MainWindow()
        {
            InitializeComponent();
            InitializeMembers();
            InitializeEvents();
        }

        private void InitializeMembers()
        {
            _settiingPanelWidth = 300;
            _blurEffect = new BlurEffect() { Radius = 3 };
            //_results = new ObservableCollection<SearchResult>();
            //dgResults.ItemsSource = _results;
            DivSettings.Visibility = Visibility.Collapsed;
            this.Title = $"{TextCache.Text["Main.Title"]} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";
            textblockVer.Text = $"Ver {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";

            this.Left = AppCache.Instance.Configs.MainLeft >= 10 ? AppCache.Instance.Configs.MainLeft : 10;
            this.Top = AppCache.Instance.Configs.MainTop >= 10 ? AppCache.Instance.Configs.MainTop : 10;
            this.Width = AppCache.Instance.Configs.MainWidth >= 640 ? AppCache.Instance.Configs.MainWidth : 640;
            this.Height = AppCache.Instance.Configs.MainHeight >= 480 ? AppCache.Instance.Configs.MainHeight : 480;

        }

        private void InitializeEvents()
        {
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

            #region WindowStyle.None

            //btnClose.Click += (s1, e1) => { App.Current.Shutdown(); };
            //btnMin.Click += (s1, e1) => { this.WindowState = WindowState.Minimized; };
            //btnMax.Click += (s1, e1) =>
            //{
            //    if (WindowState != WindowState.Maximized)
            //    {
            //        btnMax.IsChecked = true;
            //        this.WindowState = WindowState.Maximized;
            //    }
            //    else
            //    {
            //        this.WindowState = WindowState.Normal;
            //        btnMax.IsChecked = false;
            //    }
            //};
            //MouseLeftButtonDown += (s1, e1) => { DragMove();};

            #endregion

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            DivMask.MouseDown += (s1, e1) => { HideSettingPanel(); };

            buttonSetting.Click += (s1,e1)=>
            {
                if (DivSettings.Visibility == Visibility.Visible)
                    HideSettingPanel();
                else
                    ShowSettingPanel();
               // DivSettings.Visibility = Visibility.Visible;
            };
        }


        #endregion 构造函数及初始化


        #region 事件处理

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            try
            {
                if (e.Category == UserPreferenceCategory.General)
                {
                    SetThemes();
                }
            }
            catch
            { 
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (ucFindText1 == null)
                return;

            ucFindText1.inputSearchText.Focus();

        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                AppCache.Instance.Configs.SearchOption = ucFindText1.GetSearchOptions();

                if(this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;

                AppCache.Instance.Configs.MainLeft = this.Left;
                AppCache.Instance.Configs.MainTop = this.Top;
                AppCache.Instance.Configs.MainWidth = this.Width;
                AppCache.Instance.Configs.MainHeight= this.Height;

                if(!AppCache.Instance.Configs.Equals(AppCache.Instance.ConfigsOriginal))
                    AppCache.Instance.SaveAppConfigs();
            }
            catch
            {
                
            }           
        }


        #endregion 事件处理


        #region 方法

        private void ShowSettingPanel()
        {
            DivSettings.Visibility = Visibility.Visible;
            Duration duration = new Duration(TimeSpan.FromMilliseconds(200));
            DoubleAnimation da1 = new DoubleAnimation() { Duration = duration, From = 0, To = _settiingPanelWidth };
            Storyboard sb = new Storyboard() { Duration = duration };
            sb.Children.Add(da1);
            Storyboard.SetTarget(da1, DivSettings);
            Storyboard.SetTargetProperty(da1, new PropertyPath(FrameworkElement.WidthProperty));
            sb.Completed += (sender1, e1) =>
            {
                textblockVer.Visibility = Visibility.Visible;
                DivContent.Effect = _blurEffect;
                DivContent.IsHitTestVisible = false;
                DivMask.Visibility = Visibility.Visible;
                sb.Children.Clear();
            };
            sb.Begin();
        }

        private void HideSettingPanel()
        {
            if (DivSettings.Visibility == Visibility.Collapsed)
                return;

            textblockVer.Visibility = Visibility.Collapsed;
            Duration duration = new Duration(TimeSpan.FromMilliseconds(200));
            DoubleAnimation da1 = new DoubleAnimation() { Duration = duration, From = this._settiingPanelWidth, To = 0 };
            Storyboard sb = new Storyboard() { Duration = duration };
            sb.Children.Add(da1);
            Storyboard.SetTarget(da1, DivSettings);
            Storyboard.SetTargetProperty(da1, new PropertyPath(FrameworkElement.WidthProperty));
            sb.Completed += (sender1, e1) =>
            {
                DivMask.Visibility = Visibility.Collapsed;
                DivContent.Effect = null;
                DivSettings.Visibility = Visibility.Collapsed;
                DivContent.IsHitTestVisible = true;
                DivMask.Visibility = Visibility.Collapsed;
                DivContent.Effect = null;
                sb.Children.Clear();
            };
            sb.Begin();
        }

        private void SetThemes()
        {
            string flag = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1)?.ToString() ?? "1";
            if (flag == "0")
            {
                ThemeManager.ApplyTheme("Dark");
            }
            else
            {
                ThemeManager.ApplyTheme("Light");
              
            }
        }

        #endregion 方法

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AppCache.Instance.Configs.SearchOption = ucFindText1.GetSearchOptions();

            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;

            AppCache.Instance.Configs.MainLeft = this.Left;
            AppCache.Instance.Configs.MainTop = this.Top;
            AppCache.Instance.Configs.MainWidth = this.Width;
            AppCache.Instance.Configs.MainHeight = this.Height;
            
            var sss = AppCache.Instance.Configs.Equals(AppCache.Instance.ConfigsOriginal);

            MessageBox.Show($"{sss}");
        }
    }
}