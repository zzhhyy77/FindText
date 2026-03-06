using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FindText.Helpers;
using FindText.Models;

namespace FindText.Workers
{

    public class GetFilesWorker : WorkerBase
    {
        private ObservableCollection<TextSearchResult> _result;
        static readonly string[] _spor = { "," };
        CancellationToken _cancellationToken;
        string _path;
        string _pattern;
        int _maxSize;
        string[] _ignores;
        int _dirCount;
        List<string> _dirs;

        /// <summary>
        /// Worker_SearchFiles
        /// </summary>
        /// <param name="rootPath">目标文件夹</param>
        /// <param name="pattern">文件通配符</param>
        /// <param name="ignores">要忽略的类型</param>
        /// <param name="result">返回结果集</param>
        public GetFilesWorker(string rootPath, string pattern, string ignores, int maxSize)
        {
            _path = rootPath;
            _pattern = pattern;
            _ignores = ignores.Split(_spor, StringSplitOptions.RemoveEmptyEntries);
            _maxSize = maxSize * 1024 * 1024;
            _result = new ObservableCollection<TextSearchResult>();
            _dirs = new List<string>();
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_path)) throw new ArgumentNullException("Folder");
            if (string.IsNullOrEmpty(_pattern)) throw new ArgumentNullException("Pattern");
            _cancellationToken = cancellationToken;

            await Task.Run(SearchFolderAsync);

            for (int i = 0; i < _dirs.Count; i++)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                ReportProgress(i / _dirCount, i + 1, _dirCount, $"正在搜索文件：{i} / {_dirCount}");
                await Task.Run(() => SearchFilesAsync(_dirs[i]));
            }

            base.ReturnValue = _result;
        }

        private void SearchFolderAsync()
        {
            ReportProgress(0, 0, 0 , $"正在获取可访问文件夹...");
            _dirs = DirectoryHelper.GetAllAccessibleDirectories(_path).ToList();
            _dirCount = _dirs.Count;
        }

        private Task SearchFilesAsync(string dir)
        {
            DirectoryInfo curDir = new DirectoryInfo(dir);
            var files = curDir.EnumerateFiles(_pattern, SearchOption.TopDirectoryOnly);

            if (files.Any())
            {
                foreach (var file in files)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        _cancellationToken.ThrowIfCancellationRequested();
                        break;
                    }

                    if (file.Length <= _maxSize)
                    {
                        if (_ignores.Length == 0 || (_ignores.Length > 0 && !_ignores.Contains(file.Extension)))
                        {
                            _result.Add(new TextSearchResult() { FilePath = file.FullName, Title = file.Extension, Size = file.Length, LastDate = file.LastWriteTime });
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

    }


}
