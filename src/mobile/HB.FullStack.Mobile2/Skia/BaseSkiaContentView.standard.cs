using System;
using System.Collections.Generic;
using System.Text;
using HB.FullStack.XamarinForms.Base;

namespace HB.FullStack.XamarinForms.Skia
{
    public abstract class BaseSkiaContentView : BaseContentView
    {
        public override void OnAppearing()
        {
            base.OnAppearing();

            ReAddFigures();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();

            DisposeFigures();
        }

        protected abstract void DisposeFigures();

        protected abstract void ReAddFigures();
    }
}
