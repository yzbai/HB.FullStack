using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HB.FullStack.Mobile.Base
{
    public interface IBaseContentView
    {
        bool IsAppearing { get; }

        void OnAppearing();
        void OnDisappearing();

        IList<IBaseContentView?>? GetAllCustomerControls();
    }

    public abstract class BaseContentView : ContentView, IBaseContentView
    {
        public virtual void OnAppearing()
        {
            IsAppearing = true;

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
            IsAppearing = false;

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

        public bool IsAppearing
        {
            get; private set;
        }
    }
}
