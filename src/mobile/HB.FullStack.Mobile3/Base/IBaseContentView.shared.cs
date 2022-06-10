using System.Collections.Generic;

namespace HB.FullStack.Mobile.Base
{
    public interface IBaseContentView
    {
        bool IsAppearing { get; }

        void OnAppearing();
        void OnDisappearing();

        IList<IBaseContentView?>? GetAllCustomerControls();
    }
}
