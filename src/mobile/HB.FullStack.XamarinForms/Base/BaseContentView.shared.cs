using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Base
{

    public abstract class BaseContentView : ContentView, IBaseContentView
    {
        public virtual void OnAppearing()
        {
            IsAppearred = true;

            IList<IBaseContentView?>? contentViews = GetAllCustomerControls();

            if (contentViews == null)
            {
                return;
            }

            foreach (IBaseContentView? baseContentView in contentViews)
            {
                baseContentView?.OnAppearing();
            }
        }
        public virtual void OnDisappearing()
        {
            IsAppearred = false;

            IList<IBaseContentView?>? contentViews = GetAllCustomerControls();

            if (contentViews == null)
            {
                return;
            }

            foreach (IBaseContentView? baseContentView in contentViews)
            {
                baseContentView?.OnDisappearing();
            }
        }

        public abstract IList<IBaseContentView?>? GetAllCustomerControls();

        public bool IsAppearred
        {
            get; private set;
        }
    }
}
