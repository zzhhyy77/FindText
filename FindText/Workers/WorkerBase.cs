using System;
using System.Threading;
using System.Threading.Tasks;
using FindText.Models;

namespace FindText.Workers
{
    public abstract class WorkerBase
    {
        //public event Action<string> StatusChanged; //在客户端处理状态更新
        public event Action<ProgressInfo> ProgressChanged;
        public event Action<Exception> ErrorOccurred;

        /// <summary>
        /// object 为返回值
        /// </summary>
        public event Action<object> Completed;
        public event Action Cancelled;
        public object ReturnValue;

        bool _isCancelling = false;

        protected IProgress<ProgressInfo> Progress { get; private set; }
        protected CancellationToken CancellationToken { get; private set; }

        private bool _isRunning;
        public bool IsRunning => _isRunning;

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
                throw new InvalidOperationException("任务正在运行中");

            _isRunning = true;
            CancellationToken = cancellationToken;

            Progress = new Progress<ProgressInfo>(OnProgressReported);

            try
            {
                _isCancelling = false;
                OnStarted();
                await DoWorkAsync(CancellationToken);
                OnCompleted(ReturnValue);
            }
            catch (OperationCanceledException)
            {
                OnCancelled();
            }
            catch (Exception ex)
            {
                OnError(ex);
                throw;
            }
            finally
            {
                _isRunning = false;
                _isCancelling = false;
            }
        }

        protected virtual void OnProgressReported(ProgressInfo info)
        {
            info.Time = DateTime.Now;
            ProgressChanged?.Invoke(info);
        }

        protected virtual void OnStarted()
        {
            _isRunning = true;
        }

        /// <summary>
        /// <param name="rev">返回结果</param>
        /// </summary>
        protected virtual void OnCompleted(object rev)
        {
            Completed?.Invoke(rev);
        }

        protected virtual void OnCancelled()
        {
            if (_isCancelling == false)
            {
                _isCancelling = true;
                Cancelled?.Invoke();
            }
        }

        protected virtual void OnError(Exception ex)
        {
            ErrorOccurred?.Invoke(ex);
        }

        protected void ReportProgress(double percent,int current ,int total, string message = null, object tag = null)
        {
            Progress?.Report(new ProgressInfo
            {
                Percent = percent,
                Current = current,
                Total = total,
                Message = message,
                Tag = tag
            });
        }

        protected void ThrowIfCancellationRequested()
        {
            CancellationToken.ThrowIfCancellationRequested();
        }

        protected abstract Task DoWorkAsync(CancellationToken cancellationToken);
    }

}


/*
 一般用法
         private async Task xxxxAsync()
        {
            WorkerBase worker = new xxxWorker();
            worker.ProgressChanged += info =>
            {
                //进度变化
            };

            worker.Completed += (object rev) =>
            {
                //任务完成
            };

            worker.ErrorOccurred += ex =>
            {
                //任务出错
            };

            worker.Cancelled += () =>
            {
                //任务取消
            };

            try
            {
                await worker.ExecuteAsync(_cancelToken.Token);
            }
            catch (OperationCanceledException ex)
            {
                //任务取消
            }
            catch (Exception ex)
            {
                //未知异常
            }
            finally
            {
                //任务结束
            }
        }
 
 */