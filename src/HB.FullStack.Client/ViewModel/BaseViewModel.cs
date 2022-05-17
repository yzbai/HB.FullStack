using System.Threading.Tasks;

using HB.FullStack.Common;

namespace HB.FullStack.Client
{
    //TODO: 是否使用Toolkit中的ObservableObject
    public abstract class BaseViewModel : ObservableObject
    {
        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _title = string.Empty;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// page将要呈现时执行，
        /// page和viewmodel的生命周期不一定一致。
        /// </summary>
        /// <param name="pageTypeName"></param>
        /// <returns></returns>
        public virtual Task OnAppearingAsync(string pageTypeName)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// page将要消失时执行，
        /// page和viewmodel的生命周期不一定一致。
        /// </summary>
        /// <param name="pageTypeName"></param>
        /// <returns></returns>
        public virtual Task OnDisappearingAsync(string pageTypeName)
        {
            return Task.CompletedTask;
        }
    }
}