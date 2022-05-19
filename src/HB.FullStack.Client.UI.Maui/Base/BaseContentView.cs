using Microsoft.Maui.Controls;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Client.UI.Maui.Base
{
    public abstract class BaseContentView : ContentView, IBaseContentView
    {
        public virtual void OnPageAppearing()
        {
            Parallel.ForEach(GetAllCustomerControls(), v => v.OnPageAppearing());
        }

        public virtual void OnPageDisappearing()
        {
            Parallel.ForEach(GetAllCustomerControls(), v => v.OnPageDisappearing());
        }

        public abstract IList<IBaseContentView> GetAllCustomerControls();
    }
}
