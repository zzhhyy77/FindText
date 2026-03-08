using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FindText.Helpers;
using FindText.Models;

namespace FindText.Workers
{

    public class TextSearchWorker : WorkerBase
    {
        private ObservableCollection<TextSearchResult> _results;

        CancellationToken _cancellationToken;

        List<string> _searchTexts; //多关键字搜索
        TextSearchOption _option;
        int _maxSize;
        string[] _defaultIgnores;
        string[] _ignores;
        StringComparison _comparison;
        string _msgGetDirs;
        string _msgGetFiles;
        string _msgSearching;
        RegexOptions _regexOption;

        public TextSearchWorker(TextSearchOption option)
        {
            if (option == null)
                throw new ArgumentNullException("option");

            _option = option;
            //_option.Tag = null;
            _ignores = option.Ignores.Split(AppCache.SeparatorComma, StringSplitOptions.RemoveEmptyEntries);
            _maxSize = option.MaxSize * 1024 * 1024;
            _results = new ObservableCollection<TextSearchResult>();
            _comparison = _option.IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            _msgGetDirs = TextCache.Text["Task.GetDirs"];
            _msgGetFiles = TextCache.Text["Task.GetFiles"];
            _msgSearching = TextCache.Text["Task.Searching"];
            _regexOption = _option.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            _defaultIgnores = AppCache.Instance.DefaultIgnores.Replace(" ",string.Empty).Split(AppCache.SeparatorComma, StringSplitOptions.RemoveEmptyEntries);
            base.ReturnValue = _results;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            if (_option == null) throw new ArgumentNullException("Options");
            if (string.IsNullOrEmpty(_option.Path)) throw new ArgumentNullException("Folder");
            if (string.IsNullOrEmpty(_option.Pattern)) throw new ArgumentNullException("Pattern");
            if (string.IsNullOrEmpty(_option.SearchText)) throw new ArgumentNullException("searchText");

            _searchTexts = _option.SearchText.Split(AppCache.SseparatorSpace, StringSplitOptions.RemoveEmptyEntries).ToList();
            _cancellationToken = cancellationToken;

            await Task.Run(SearchFilesAsync);
        }

        private Task SearchFilesAsync()
        {
            ReportProgress(0, -2, 0, _msgGetDirs);
            List<TextSearchResult> files = new List<TextSearchResult>();

            //1、先查找可访问的文件夹
            var dirs = DirectoryHelper.GetAllAccessibleDirectories(_option.Path).ToList();

            //2、筛选需处理的文件
            ReportProgress(0, -1, dirs.Count, _msgGetFiles);
            for (int i = 0; i < dirs.Count; i++)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                DirectoryInfo curDir = new DirectoryInfo(dirs[i]);
                var dirFiles = curDir.EnumerateFiles(_option.Pattern, SearchOption.TopDirectoryOnly);
                if (dirFiles.Any())
                {
                    foreach (var file in dirFiles)
                    {
                        if (file.Length <= _maxSize && !_defaultIgnores.Contains(file.Extension.ToLower()))
                        {
                            if (_option.IsAllFiles == false || (_option.IsAllFiles = true && !_ignores.Contains(file.Extension.ToLower())))
                            {
                                files.Add(new TextSearchResult() { FilePath = file.FullName, Title = file.Extension, Size = file.Length, LastDate = file.LastWriteTime });
                            }
                        }
                    }
                }
                //await Task.Delay(50); 
            }

            //3、逐文件查找
            if (files.Any())
            {
                for (int i = 0; i < files.Count; i++)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    TextSearchResult? sf;
                    ReportProgress(Math.Round((i * 1.0 / files.Count) * 100, 0), i + 1, files.Count, $"{_msgSearching}{i + 1}/{files.Count}");

                    if (_option.IsIgnoreError)
                    {
                        try
                        {
                            sf = SearchFiles(files[i].FilePath);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    else
                    {
                        sf = SearchFiles(files[i].FilePath);
                    }

                    if (sf != null)
                    {
                        Application.Current.Dispatcher.Invoke(() => _results.Add(sf));
                    }
                    //await Task.Delay(50);
                }
            }
        
            return Task.CompletedTask;
        }

        private TextSearchResult? SearchFiles(string filePath)
        {

            FileInfo fi = new FileInfo(filePath);
            if (!fi.Exists)
                return null;

            if (fi.Length > _maxSize)
                return null;

            List<int> ints = new List<int>();
            string content = string.Empty;
            string preview = string.Empty;

            if (!string.IsNullOrEmpty(_option.EncodingName))
            {
                content = File.ReadAllText(filePath, _option.FileEncoding);
            }
            else
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                Encoding encoding = EncodingHelper.DetectEncoding(bytes);
                content = encoding.GetString(bytes);
            }


            if (_option.IsRegex)
            {
                if (RegexMatch(content, _option.SearchText))
                {
                    preview = content.Substring(0, content.Length < 30 ? content.Length : 30); //一般开头是备注
                    return new TextSearchResult()
                    {
                        FilePath = filePath,
                        Size = Math.Round(fi.Length * 1.0 / 1024, 0),
                        LastDate = fi.LastWriteTime,
                        Preview = preview
                    };
                }
            }
            else
            {
                for (int i = 0; i < _searchTexts.Count; i++)
                {
                    int pos = content.IndexOf(_searchTexts[i], _comparison);
                    if (pos >= 0)
                        ints.Add(pos);
                }

                if (ints.Count == _searchTexts.Count)
                {
                    preview = content.Substring(0, content.Length < 30 ? content.Length : 30);
                    return new TextSearchResult()
                    {
                        FilePath = filePath,
                        Size = Math.Round(fi.Length * 1.0 / 1024, 0),
                        Position = ints[0],
                        LastDate = fi.LastWriteTime,
                        Preview = preview
                    };
                }
            }
            return null;
        }


        #region 速度太慢 改为勾选项，需要时勾选

        private bool WholeWordSearch(string content, string searchText)
        {
            return Regex.IsMatch(content, $@"\b{Regex.Escape(searchText)}\b", _regexOption);
        }

        private bool MatchesSearch(string content)
        {
            if (_searchTexts == null || _searchTexts.Count <= 0)
                throw new ArgumentNullException("SearchText");

            string pattern = string.Concat(_searchTexts.Select(x => $"(?=.*{Regex.Escape(x)})"));
            Regex regex = new Regex(pattern, _regexOption);
            return regex.IsMatch(content);
        }

        #endregion

        private bool RegexMatch(string content,string searchText)
        {
            return Regex.IsMatch(content, searchText,_regexOption);
        }   
    }
}
