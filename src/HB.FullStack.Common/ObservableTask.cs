//#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace HB.FullStack.Common
{
    /// <summary>
    /// 这是一个Bindable 的Task，可以让空间binding 这个task，等task完成后，更新结果。
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public sealed class ObservableTask<TResult> : ObservableObject
    {
        public TResult? InitialResult { get; private set; }

        public Task<TResult>? Task { get; private set; }

        public Task? TaskCompletion { get; private set; }

        [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "已经判断过TaskStaus为RanToCompletion了")]
        [SuppressMessage("Usage", "VSTHRD104:Offer async methods", Justification = "已经判断过TaskStaus为RanToCompletion了")]
        public TResult? Result => (Task == null || Task.Status != TaskStatus.RanToCompletion) ? InitialResult : Task.Result;

        public TaskStatus Status => Task == null ? TaskStatus.RanToCompletion : Task.Status;

        public bool IsCompleted => Task == null || Task.IsCompleted;

        public bool IsNotCompleted => Task != null && !Task.IsCompleted;

        public bool IsSuccessfullyCompleted => Task == null || Task.Status == TaskStatus.RanToCompletion;

        public bool IsCanceled => Task != null && Task.IsCanceled;

        public bool IsFaulted => Task != null && Task.IsFaulted;

        public AggregateException? Exception { get { return Task?.Exception; } }

        public Exception? InnerException { get { return Exception?.InnerException; } }

        public Action<Exception>? ExceptionHandler { get; private set; }
        public bool ContinueOnCapturedContext { get; private set; }

        public string? ErrorMessage { get { return InnerException?.Message; } }

        private readonly Func<Task<TResult>>? _taskFunc;

        public ObservableTask(TResult? initialResult, Func<Task<TResult>>? taskFunc, Action<Exception>? onException = null, bool continueOnCapturedContext = false)
        {
            InitialResult = initialResult;
            ExceptionHandler = onException;

            _taskFunc = taskFunc;
            ContinueOnCapturedContext = continueOnCapturedContext;

            TriggerTask();
        }

        public async Task RePlayAsync()
        {
            if (TaskCompletion != null)
            {
                await TaskCompletion.ConfigureAwait(false);
            }

            TriggerTask();
        }

        private void TriggerTask()
        {
            if (_taskFunc != null)
            {
                Task = _taskFunc();

                if (!Task.IsCompleted)
                {
                    TaskCompletion = WatchTaskAsync();
                }
            }
        }

        private async Task WatchTaskAsync()
        {
            try
            {
                await Task!.ConfigureAwait(ContinueOnCapturedContext);
            }
            catch (Exception obj) when (ExceptionHandler != null)
            {
                ExceptionHandler!(obj);
            }

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsNotCompleted));

            if (Task!.IsCanceled)
            {
                OnPropertyChanged(nameof(IsCanceled));
            }
            else if (Task.IsFaulted)
            {
                OnPropertyChanged(nameof(IsFaulted));
                OnPropertyChanged(nameof(Exception));
                OnPropertyChanged(nameof(InnerException));
                OnPropertyChanged(nameof(ErrorMessage));
            }
            else
            {
                OnPropertyChanged(nameof(IsSuccessfullyCompleted));
                OnPropertyChanged(nameof(Result));
            }
        }
    }
}
//