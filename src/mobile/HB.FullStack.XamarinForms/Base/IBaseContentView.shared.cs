using System.Collections.Generic;

namespace HB.FullStack.XamarinForms.Base
{
    public interface IBaseContentView
    {
        bool IsAppearing { get; }

        void OnAppearing();
        void OnDisappearing();

        IList<IBaseContentView?>? GetAllCustomerControls();
    }
}
