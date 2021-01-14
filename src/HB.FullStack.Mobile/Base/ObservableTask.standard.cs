#nullable disable
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Xamarin.CommunityToolkit.ObjectModel;

namespace System
{
    /// <summary>
    /// 这是一个Bindable 的Task，可以让空间binding 这个task，等task完成后，更新结果。
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public sealed class ObservableTask<TResult> : ObservableObject
    {
        public TResult InitialResult { get; private set; }

        public Task<TResult> Task { get; private set; }

        public Task TaskCompletion { get; private set; }

        public TResult Result => (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : InitialResult;

        public TaskStatus Status { get { return Task.Status; } }

        public bool IsCompleted { get { return Task.IsCompleted; } }

        public bool IsNotCompleted { get { return !Task.IsCompleted; } }

        public bool IsSuccessfullyCompleted { get { return Task.Status == TaskStatus.RanToCompletion; } }

        public bool IsCanceled { get { return Task.IsCanceled; } }

        public bool IsFaulted { get { return Task.IsFaulted; } }

        public AggregateException Exception { get { return Task.Exception; } }

        public Exception InnerException { get { return Exception?.InnerException; } }

        public string ErrorMessage { get { return InnerException?.Message; } }

        private readonly Func<Task<TResult>> _taskFunc;

        private readonly Action<Exception> _exceptionHandler;

        private readonly bool _continueOnCapturedContext;

        public ObservableTask(Func<Task<TResult>> taskFunc, TResult initialResult = default, Action<Exception> onException = null, bool continueOnCapturedContext = false)
        {
            _taskFunc = taskFunc;
            InitialResult = initialResult;
            _exceptionHandler = onException;
            _continueOnCapturedContext = continueOnCapturedContext;

            TriggerTask();
        }

        public async Task RePlayAsync()
        {
            await TaskCompletion.ConfigureAwait(false);

            TriggerTask();
        }

        private void TriggerTask()
        {
            Task = _taskFunc();

            if (!Task.IsCompleted)
            {
                TaskCompletion = WatchTaskAsync();
            }
        }

        private async Task WatchTaskAsync()
        {
            try
            {
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                await Task.ConfigureAwait(_continueOnCapturedContext);
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
            }
            catch (Exception obj) when (_exceptionHandler != null)
            {
                _exceptionHandler!(obj);
            }

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsNotCompleted));

            if (Task.IsCanceled)
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
#nullable restore