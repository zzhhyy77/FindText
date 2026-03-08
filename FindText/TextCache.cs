using FindText.Helpers;
using FindText.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FindText
{
    /// <summary>
    /// 此方案效果一般，还是应改为 资源文件 + 转Json工具类 的方案
    /// </summary>
    internal class TextCache : VModelsBase
    {
        Dictionary<string, int> _index;
        volatile static TextCache _instance;
        static readonly object locker = new object();
        static ObservableCollection<KeyValue> _appText;

        public static TextCache Text
        {
            get
            {
                lock (locker)
                {
                    if (_instance == null) _instance = new TextCache();
                    return _instance;
                }
            }
        }

        private TextCache()
        {
            InitializeText();
            this.TakeIndex();
        }

        //public ObservableCollection<KeyValue> AppText
        //{
        //    get
        //    {
        //        return _appText;
        //    }
        //}

        public string this[string TextKey]
        {
            get
            {
                int begin = this.GetStartPos(TextKey);
                for (int i = begin; i < _appText.Count; i++)
                {
                    if (_appText[i].Key == TextKey)
                        return _appText[i].Value;
                }
                return null;
            }
            set
            {
                foreach (var key in _appText)
                {
                    if (key.Key == TextKey)
                    {
                        key.Value = value;
                        break;
                    }
                }
            }
        }

        public static void SetLanguage(string languageCode)
        {
            try
            {
                string path = $"{Environment.CurrentDirectory}\\Language\\{languageCode}.json";
                if (File.Exists(path))
                {
                    string str = File.ReadAllText(path);
                    ObservableCollection<KeyValue>? list = JsonHelper.Parse<ObservableCollection<KeyValue>>(str);
                    if (list != null)
                    {
                        foreach (var app in _appText)
                        {
                            foreach (var cfg in list)
                            {
                                if (app.Key == cfg.Key && app.Value != cfg.Value)
                                {
                                    app.Value = cfg.Value;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception(Text["CM.NoLangFile"]);
                }
            }
            catch
            {
                throw;
            }
        }

        public static void ExportLanguage()
        {
            try
            {
                string path = $"{Environment.CurrentDirectory}\\Language";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path += "\\LangTemplate.json";
                string js = JsonHelper.ToJson(_appText,false);
                File.WriteAllText(path, js);
            }
            catch
            {
                throw;
            }
        }

        private int GetStartPos(string textkey)
        {
            string tmp = textkey.Split('.')[0];
            if (this._index.ContainsKey(tmp))
                return this._index[tmp];
            else
                return 0;
        }

        private void TakeIndex()
        {
            //创建简单的索引，提升性能
            if (this._index != null) return;
            this._index = new Dictionary<string, int>();
            string preVal = string.Empty;
            string curVal = string.Empty;
            for (int i = 0; i < _appText.Count; i++)
            {
                curVal = _appText[i].Key.Split('.')[0];
                if (curVal != preVal)
                {
                    this._index.Add(curVal, i);
                    preVal = curVal;
                }
            }
        }

        /// <summary>
        /// 按前缀建立索引，同一类别必须写在一起
        /// </summary>
        private void InitializeText()
        {
            _appText = new ObservableCollection<KeyValue>();

            _appText.Add(KeyValue.New("CM.NoLangFile", "没有相关的语言配置文件"));
            _appText.Add(KeyValue.New("CM.Save", "保存"));
            _appText.Add(KeyValue.New("CM.OK", "确定"));
            _appText.Add(KeyValue.New("CM.Cancel", "取消"));
            _appText.Add(KeyValue.New("CM.Close", "关闭"));
            _appText.Add(KeyValue.New("CM.Refresh", "刷新"));
            _appText.Add(KeyValue.New("CM.Completed", "已完成"));

            _appText.Add(KeyValue.New("App.Dark", "深色"));
            _appText.Add(KeyValue.New("App.Light", "浅色"));

            //MainWindow
            _appText.Add(KeyValue.New("Main.Title", "找文本"));

            //UCSetting
            _appText.Add(KeyValue.New("Tool.Theme", "颜色主题："));
            _appText.Add(KeyValue.New("Tool.UnixTitle", "Unix时间戳转换 (精确到秒)"));
            _appText.Add(KeyValue.New("Tool.UnixLabel", "Unix时间戳："));
            _appText.Add(KeyValue.New("Tool.DateLabel", "日期格式："));
            _appText.Add(KeyValue.New("Tool.UnixError", "无效值"));
            _appText.Add(KeyValue.New("Tool.CacheDir", "缓存文件夹"));
            _appText.Add(KeyValue.New("Tool.ToolsWnd", "工 具"));

            //UCFindText
            _appText.Add(KeyValue.New("FT.MaxSizeLabel", "忽略 "));
            _appText.Add(KeyValue.New("FT.MaxSize", " MB以上文件"));
            _appText.Add(KeyValue.New("FT.Case", "区分大小写"));
            _appText.Add(KeyValue.New("FT.Whole", "精确匹配"));
            _appText.Add(KeyValue.New("FT.Encoding", "编码："));
            _appText.Add(KeyValue.New("FT.Ignore", "忽略错误"));
            _appText.Add(KeyValue.New("FT.Date", "日期："));
            _appText.Add(KeyValue.New("FT.Regex", "正则表达式"));
            _appText.Add(KeyValue.New("FT.Total", "共找到："));
            _appText.Add(KeyValue.New("FT.SelCount", "已选中："));
            _appText.Add(KeyValue.New("FT.Again", "缓存中已经有【{0}】\r\n是否要重新查找，并覆盖上次结果？"));

            //DataGrid
            _appText.Add(KeyValue.New("DG.Path", "路径"));
            _appText.Add(KeyValue.New("DG.Position", "位置"));
            _appText.Add(KeyValue.New("DG.Size", "大小 (KB)"));
            _appText.Add(KeyValue.New("DG.Preview", "预览"));
            _appText.Add(KeyValue.New("DG.Note", "📝  备注"));
            _appText.Add(KeyValue.New("DG.Find", "筛选..."));
            _appText.Add(KeyValue.New("DG.OpenPath", "打开所在文件夹"));
            _appText.Add(KeyValue.New("DG.OpenSel", "打开所选项"));

            //SearchTextWorker
            _appText.Add(KeyValue.New("Task.Canceled", "任务已取消"));
            _appText.Add(KeyValue.New("Task.GetDirs", "正在检索可访问文件夹..."));
            _appText.Add(KeyValue.New("Task.GetFiles", "正在筛选文件..."));
            _appText.Add(KeyValue.New("Task.Searching", "正在查找："));

            //水印
            _appText.Add(KeyValue.New("WM.Search", "查找内容..."));

            //错误消息
            _appText.Add(KeyValue.New("Err.FolderNull", "请选择目标文件夹"));
            _appText.Add(KeyValue.New("Err.NoFolder", "文件夹不存在"));
            _appText.Add(KeyValue.New("Err.PatternNull", "请输入文件类型"));
            _appText.Add(KeyValue.New("Err.TextNull", "请输入搜索内容"));

            //常用编码
            _appText.Add(KeyValue.New("BM.Auto", "自动判断"));
            _appText.Add(KeyValue.New("BM.GB2312", "简体中文"));


            //各种消息
            _appText.Add(KeyValue.New("MSG.MaxCount", "为避免误操作，\r\n一次最多只能同时打开30个文件，\r\n多文件请分批打开"));
            _appText.Add(KeyValue.New("MSG.MsgCount", "选中文件较多\r\n确定要同时打开吗？"));
            _appText.Add(KeyValue.New("MSG.OK", "选中文件较多\r\n确定要同时打开吗？"));
            _appText.Add(KeyValue.New("MSG.Title", "消息框..."));
            _appText.Add(KeyValue.New("MSG.Del", "确认删除"));
            _appText.Add(KeyValue.New("MSG.IsRoot", "所选为根目录\r\n在根目录查找可能较为耗时，\r\n建议选择具体的目录"));
            _appText.Add(KeyValue.New("MSG.SaveAs", "将当前数据转换为数据标签，转换后不可在程序里修改，\r\n但可在【Cache】文件夹删除或改名为普通数据，\r\n是否要转换？"));

            //工具窗口
            _appText.Add(KeyValue.New("WT.New", "创建数据标签"));
            _appText.Add(KeyValue.New("WT.Label", "标签名："));
            _appText.Add(KeyValue.New("WT.Content", "内容："));
            _appText.Add(KeyValue.New("WT.TitleNull", "请输入文件标题"));
            _appText.Add(KeyValue.New("WT.TextNull", "请输入内容"));
            _appText.Add(KeyValue.New("WT.CacheMgr", "缓存 <-> Json"));
            _appText.Add(KeyValue.New("WT.OpenCache", "缓存->Json"));
            _appText.Add(KeyValue.New("WT.Convert", "Json->缓存"));
            _appText.Add(KeyValue.New("WT.JsonError", "无效Json文件"));
            _appText.Add(KeyValue.New("WT.TitleError", "无效标题（不能包括\\/?*等字符）"));

            InitializeHelpInfo();
        }

        /// <summary>
        /// 帮助内容
        /// </summary>
        private void InitializeHelpInfo()
        {
            //提示消息
            _appText.Add(KeyValue.New("Help.Pattern", @"关于文件类型： 指要搜索的文件后缀名 或 通配符,如：*.txt，*.md 等
例: 
    *.lua : 搜索所有lua文件
    abc*.txt : 搜索所有以abc开头的txt文件

* 或 *.* 代表搜索所有符合大小的文件

注：即使使用了*.*等全文件通配符，程序会也自动忽略常见图像，视频，压缩包等类型的文件。"));

            _appText.Add(KeyValue.New("Help.Encoding", @"关于编码：
如果发现匹配不到内容，可能因为文件用的编码不对，可用其他编码再查找试试。

自动判断 = 自动判断每个文件的编码 (只能识别常用的几种编码)
简体中文 = GB2312

注：自动判断会影响查找效率，除非文件夹内有各种编码的文件，否则应指定具体的编码"));


            _appText.Add(KeyValue.New("Help.His", @"关于缓存：
1、 每次的结果保存在Cache文件夹，以避免重复查找。
      命名规则：Result_【查找内容】_(【目标文件夹】).cache， 
      可手工修改文件名中的【查找内容】，以便识别。

2、 常用数据可存为数据标签，减少查数据时在不同程序间来回切换。
      命名规则：Data_【标签名称】_(Cache).cache
      创建方法：
            1、在【缓存列表框】右击，点击【创建数据标签】
            2、把整理好的数据复制粘贴到【内容】框（一行一条数据），输入标签名称， 点击【保存为缓存文件】。

3、程序只显示最近20次记录，如历史记录过多，可在Cache文件夹删除无用记录。
     删除文件即可删除对应标签数据。"));

            _appText.Add(KeyValue.New("Help.Ignore", @"关于忽略类型：
对于图片，视频，压缩包 等文件，查找文本意义不大，
可将其后缀名添加到忽略列表中，搜索时将忽略这些后缀名的文件。
多个类型用逗号分隔。
例: *.exe , *.dll , *.bin

注：程序会自动忽略常见的图像，视频，压缩包等类型的文件。
"));

            _appText.Add(KeyValue.New("Help.Find", @"关于查找内容：    
1、多个查找内容用空格分隔，当查找多个内容时，只显示同时包含所有内容的文件。
     不区分内容出现顺序。

例: 输入 12345 abcde
查找结果只显示同时包含 12345 和 abcde 的文件。

2、如果使用正则表达式，查找结果的标题可能为随机字符串（因为正则表达式可能包括特殊字符），
     需可将对应的文件名改为有意义的名称。"));

            _appText.Add(KeyValue.New("Help.IgnoreError", @"关于忽略错误：    
查找时可能因为无权限、文件被占用，被删除等原因报错，
启用此选项后，程序将忽略错误继续执行。

注：启用此选项可能会遗漏文件而不被察觉。"));

            _appText.Add(KeyValue.New("Help.OpenFile", @"关于打开所选：
推荐使用 Notepad++ 等支持多标签的文本编辑器作为系统默认打开方式，
以便同时打开多个文件进行批量替换等操作，
否则多选文件时会新建多个窗口，不能多文件查找/替换。"));

            _appText.Add(KeyValue.New("Help.Whole", @"关于精确匹配：只匹配完全相同的内容。
例：如勾选此选项，输入 abc ，
则只匹配 abc，而不匹配 abcd 或 123abc456"));

            _appText.Add(KeyValue.New("Help.Regex", @"关于启用正则表达式： 
将查找内容视为正则表达式

注：启用此选项会降低效率"));


        }
    }
}



/*

xaml用法：

方式1：
    1： 添加名空间： xmlns:app="clr-namespace:FindText"
    2： <TextBlock Text="{Binding Path=[keyName],Source={x:Static app:TextCache.Text}}" />

方式2：
    1： 添加名空间： xmlns:app="clr-namespace:FindText"
    2： 在父元素添加 DataContext="{x:Static app:TextCache.Text}" 
    3： <TextBlock Text="{Binding [keyName]}" />


后台代码：
    string str = TextCache.Text["Main.Title"]

 

*/


