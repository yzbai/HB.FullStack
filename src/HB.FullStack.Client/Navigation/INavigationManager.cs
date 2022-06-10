using System.Collections.Generic;
using System.Threading.Tasks;


namespace HB.FullStack.Client.Navigation
{
    public interface INavigationManager
    {
        /// <summary>
        /// Gets the current IPlatformApplication.
        /// This must be set in each implementation manually, as we can't
        /// have a true static be used in the implementation.
        /// </summary>
        public static INavigationManager Current { get; set; } = null!;

        Task GotoAsync(string uri, IDictionary<string, object?>? parameters = null);

        Task GoBackAsync(IDictionary<string, object?>? parameters = null);

        Task PushAsync(string pageFullName);

        Task PushAsync(IBaseViewModel baseViewModel);

        Task PushAsync(object page);

        Task PopAsync();

        Task PushModalAsync(string pageFullName);

        Task PushModalAsync(IBaseViewModel baseViewModel);

        Task PushModalAsync(object page);

        Task PopModalAsync();

        bool Animated { get; }

    }
}