#nullable disable
using System.ComponentModel;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 这是一个Bindable 的Task，可以让空间binding 这个task，等task完成后，更新结果。
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public sealed class ObservableTask<TResult> : ObservableObject
    {
        public Task<TResult> Task { get; private set; }

        public Task TaskCompletion { get; private set; }

        public TResult Result { get { return (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default; } }

        public TaskStatus Status { get { return Task.Status; } }

        public bool IsCompleted { get { return Task.IsCompleted; } }

        public bool IsNotCompleted { get { return !Task.IsCompleted; } }

        public bool IsSuccessfullyCompleted { get { return Task.Status == TaskStatus.RanToCompletion; } }

        public bool IsCanceled { get { return Task.IsCanceled; } }

        public bool IsFaulted { get { return Task.IsFaulted; } }

        public AggregateException Exception { get { return Task.Exception; } }

        public Exception InnerException { get { return Exception?.InnerException; } }

        public string ErrorMessage { get { return InnerException?.Message; } }

        //public event PropertyChangedEventHandler PropertyChanged;

        public ObservableTask(Task<TResult> task, Action<Exception> onException = null, bool continueOnCapturedContext = false)
        {
            ThrowIf.Null(task, nameof(task));

            Task = task;

            if (!task.IsCompleted)
            {
                TaskCompletion = WatchTaskAsync(task, onException, continueOnCapturedContext);
            }
        }
        private async Task WatchTaskAsync(Task task, Action<Exception> onException, bool continueOnCapturedContext)
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception obj) when (onException != null)
            {
                onException!(obj);
            }

            //PropertyChangedEventHandler propertyChanged = PropertyChanged;

            //if (propertyChanged == null)
            //{
            //    return;
            //}

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsNotCompleted));

            if (task.IsCanceled)
            {
                OnPropertyChanged(nameof(IsCanceled));
            }
            else if (task.IsFaulted)
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