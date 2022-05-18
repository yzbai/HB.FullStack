using System.Collections.Generic;
using System.Threading.Tasks;


namespace HB.FullStack.Client
{
    public interface INavigationManager
    {
        /// <summary>
        /// Gets the current IPlatformApplication.
        /// This must be set in each implementation manually, as we can't
        /// have a true static be used in the implementation.
        /// </summary>
        public static INavigationManager? Current { get; set; }

        Task GotoAsync(string uri, bool animated = false);

        Task GotoAsync(string uri, IDictionary<string, string> parameters, bool animated = false);

        Task GoBackAsync(bool animated = false);

        Task PushAsync(string pageFullName, bool animated = false);

        Task PushAsync(BaseViewModel baseViewModel, bool animated = false);

        Task PushAsync(object page, bool animated = false);

        Task PopAsync(bool animated = false);

        Task PushModalAsync(string pageFullName, bool animated = false);

        Task PushModalAsync(BaseViewModel baseViewModel, bool animated = false);

        Task PushModalAsync(object page, bool animated = false);

        Task PopModalAsync(bool animated = false);

    }
}