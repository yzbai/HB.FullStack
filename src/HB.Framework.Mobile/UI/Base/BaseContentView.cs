using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HB.Framework.Client.Base
{
    public interface IBaseContentView
    {
        void OnAppearing();
        void OnDisappearing();
    }

    public abstract class BaseContentView : ContentView, IBaseContentView
    {
        public abstract void OnAppearing();
        public abstract void OnDisappearing();
    }
}
