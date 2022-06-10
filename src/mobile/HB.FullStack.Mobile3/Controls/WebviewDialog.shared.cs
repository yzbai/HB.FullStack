﻿using AsyncAwaitBestPractices;
using HB.FullStack.Mobile.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace HB.FullStack.Mobile.Controls
{
    public class WebviewDialog : BaseModalDialog
    {
        private readonly HybridWebView _hybridWebView;
        private readonly ActivityIndicator _indicator;

#pragma warning disable CA1056 // URI-like properties should not be strings
        public string Url { get { return _hybridWebView.Uri; } set { _hybridWebView.Uri = value; _hybridWebView.Reload(); } }
#pragma warning restore CA1056 // URI-like properties should not be strings


        public WebviewDialog()
        {
            _indicator = new ActivityIndicator() { VerticalOptions = LayoutOptions.CenterAndExpand, HorizontalOptions = LayoutOptions.Fill };
            _indicator.BindingContext = this;
            _indicator.SetBinding(ActivityIndicator.IsRunningProperty, "IsBusy");

            _hybridWebView = new HybridWebView() { HeightRequest = 400 };

            _hybridWebView.Navigating += (sender, e) => { IsBusy = true; };
            _hybridWebView.Navigated += (sender, e) => { IsBusy = false; };


            Button retButton = new Button
            {
                Text = "返回"
            };
            retButton.Clicked += (sender, e) =>
            {
                NavigationService.Current.PopModal();
            };


            StackLayout stackLayout = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,

                Children = { _hybridWebView, retButton }
            };

            AbsoluteLayout.SetLayoutBounds(stackLayout, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(stackLayout, AbsoluteLayoutFlags.All);

            AbsoluteLayout.SetLayoutBounds(_indicator, new Rectangle(0.5, 0.5, -1, -1));
            AbsoluteLayout.SetLayoutFlags(_indicator, AbsoluteLayoutFlags.PositionProportional);

            Content = new AbsoluteLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    stackLayout,
                    _indicator
                 }
            };
        }

        protected override IList<IBaseContentView?>? GetAllCustomerControls()
        {
            return new List<IBaseContentView?> { _hybridWebView };
        }
    }
}