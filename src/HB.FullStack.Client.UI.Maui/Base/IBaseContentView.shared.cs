using System.Collections.Generic;

namespace HB.FullStack.Client.UI.Maui
{
    public interface IBaseContentView
    {
        bool IsAppearing { get; }

        void OnAppearing();

        void OnDisappearing();

        IList<IBaseContentView?>? GetAllCustomerControls();
    }
}
