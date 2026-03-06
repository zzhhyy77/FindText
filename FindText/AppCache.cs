using FindText.Helpers;
using FindText.Models;
using FindText.Themes;
using System.Collections.Generic;
using System.Diagnostics;

namespace FindText
{
    /// <summary>
    /// 放全局变量
    /// </summary>
    public class AppCache
    {

        #region 

        volatile static AppCache? _instance = null;
        static readonly object locker = new object();

        Dictionary<ThemeCode, string> _themes;

        AppConfigValue _configs;

        AppConfigValue _configsOriginal;

        string _defaultIgnores;

        //TextSearchOption _textSearchOption;
        internal static readonly string[] SeparatorComma = { "," };
        internal static readonly string[] SseparatorSpace ={ " " };
        internal static readonly string[] Wildcard = { "*", "*.*", "*.", ".*", "." };

        #endregion


        #region 成员及初始化

        public static AppCache Instance
        {
            get
            {
                lock (locker)
                {
                    if (_instance == null) _instance = new AppCache();
                    return _instance;
                }
            }
        }

        private AppCache()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            _themes = new Dictionary<ThemeCode, string>();
            _themes.Add(ThemeCode.Dark, TextCache.Text["App.Dark"]);
            _themes.Add(ThemeCode.Light, TextCache.Text["App.Light"]);
            _defaultIgnores = ".docx,.xlsx,.ico,.tiff,.png,.bmp,.jpg,.mp4,.avi,.mkv,.7z,.gz,.tar,.rar,.zip,.iso";
            _configs = new AppConfigValue();
        }


        #endregion 成员及初始化


        #region 运行参数

        /// <summary>
        /// 程序运行时的配置，用于判断是否需要重新保存配置文件
        /// </summary>
        internal AppConfigValue ConfigsOriginal
        {
            get
            {
                return _configsOriginal;
            }
        }


        internal AppConfigValue Configs
        {
            get
            {
                return _configs;
            }
        }

        internal Dictionary<ThemeCode, string> Themes
        {
            get
            {
                return _themes;
            }
        }

        internal string DefaultIgnores
        {
            get
            {
                return _defaultIgnores;
            }
            set
            {
                this._defaultIgnores = value;
            }
        }

        #endregion


        #region MyRegion
        /// <summary>
        /// 保存的AppCache参数
        /// </summary>
        /// <param name="folder"></param>
        internal void LoadAppConfigs()
        {
            string filePath = $"{Process.GetCurrentProcess().ProcessName}.json";
            string json = AppConfigHelper.LoadJson(string.Empty, filePath);

            if (!string.IsNullOrEmpty(json))
            {
                var val = JsonHelper.Parse<AppConfigValue>(json);
                if (val != null)
                {
                    _configs.SearchOption = val.SearchOption;
                    _configs.MainLeft = val.MainLeft;
                    _configs.MainTop = val.MainTop;
                    _configs.MainWidth = val.MainWidth;
                    _configs.MainHeight = val.MainHeight;
                    _configs.Language = val.Language;
                    _configs.Theme = val.Theme;

                    _configsOriginal = JsonHelper.Clone<AppConfigValue>(_configs);
                }
            }
        }


        internal void SaveAppConfigs()
        {
            string json = JsonHelper.ToJson(AppCache.Instance.Configs, false);
            //json = Utils.StringCompress.Compress(json);
            AppConfigHelper.SaveConfig(string.Empty, $"{Process.GetCurrentProcess().ProcessName}.json", json);
        }

        #endregion

    }
}
