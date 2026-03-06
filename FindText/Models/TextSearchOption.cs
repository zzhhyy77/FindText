using System;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace FindText.Models
{
    public class TextSearchOption
    {
        string _path;
        string _pattern;
        string _searchText;
        string _ignores;
        int _maxsize;
        bool _caseSensitive;
        bool _ignoreError;
        bool _wholeWord;
        bool _isRegex;
        string _encodingName;
        bool _isAllFiles;

        //DateTime? _date;
        SearchOption _fileSearchOption;
        
        public TextSearchOption()
        {
            _isAllFiles = false;
            _ignores = string.Empty;
            _maxsize = 1;
            _caseSensitive = true;
            _ignoreError = false;
            _wholeWord = false;
            _encodingName = string.Empty;
            //_date = null;
            _fileSearchOption = SearchOption.AllDirectories;
        }

        /// <summary>
        /// 目标文件夹
        /// </summary>
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        /// <summary>
        /// 文件类型，搜索通配符
        /// </summary>
        public string Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }

        /// <summary>
        /// 要查找的内容
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; }
        }

        /// <summary>
        /// 忽略文件类型，默认值 = null
        /// </summary>
        public string Ignores
        {
            get { return _ignores; }
            set { _ignores = value; }
        }

        /// <summary>
        /// 文件大小限制，默认值 = 1 MB
        /// </summary>
        public int MaxSize
        {
            get { return _maxsize; }
            set { _maxsize = value; }
        }

        /// <summary>
        /// 区分大小写，默认值 = true
        /// </summary>
        public bool IsCaseSensitive
        {
            get { return _caseSensitive; }
            set { _caseSensitive = value; }
        }

        /// <summary>
        /// 忽略文件错误，默认值 = false
        /// </summary>
        public bool IsIgnoreError
        {
            get { return _ignoreError; }
            set { _ignoreError = value; }
        }

        /// <summary>
        /// 是否精确匹配，默认值 = false
        /// </summary>
        public bool IsWholeWord
        {
            get { return _wholeWord; }
            set { _wholeWord = value; }
        }

        /// <summary>
        /// 是否使用正则表达
        /// </summary>
        public bool IsRegex
        {
            get { return _isRegex; }
            set { _isRegex = value; }
        }

        public string EncodingName
        {
            get
            {
                return _encodingName;
            }
            set 
            {
                _encodingName = value;
            }
        }

        [JsonIgnore]
        /// <summary>
        /// 文件使用的编码，默认值 = ANSI （操作系统默认值）
        /// </summary>        
        public Encoding FileEncoding
        {
            get
            {
                if (!string.IsNullOrEmpty(_encodingName))
                {
                    return Encoding.GetEncoding(_encodingName);
                }

                return Encoding.Default; 
            }
        }

        public bool IsAllFiles
        {
            get
            {
                return _isAllFiles;
            }
            set
            {
                _isAllFiles = value;
            }
        }

        //public DateTime? Date
        //{
        //    get { return _date; }
        //    set { _date = value; }
        //}

        /// <summary>
        /// 是否搜索子文件夹，默认 = 是 （ SearchOption.AllDirectories ）
        /// </summary>
        public SearchOption FileSearchOption
        {
            get { return _fileSearchOption; }
            set { _fileSearchOption = value; }
        }

    }
}
