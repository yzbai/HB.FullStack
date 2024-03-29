﻿using CommunityToolkit.Maui.Views;

using HB.FullStack.Client.MauiLib.Utils;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace HB.FullStack.Client.MauiLib.Controls
{
    public class WebviewPopup : Popup
    {
        private readonly HybridWebView _hybridWebView;
        private readonly ActivityIndicator _indicator;

        public string? Url
        {
            get { return (_hybridWebView.Source as UrlWebViewSource)?.Url; }
            set { _hybridWebView.Source = value; }
        }

        public WebviewPopup()
        {
            _indicator = new ActivityIndicator() { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Fill };
            _indicator.BindingContext = this;
            _indicator.SetBinding(ActivityIndicator.IsRunningProperty, "IsBusy");

            _hybridWebView = new HybridWebView() { HeightRequest = 400 };

            //_hybridWebView.Navigating += (sender, e) => { IsBusy = true; };
            //_hybridWebView.Navigated += (sender, e) => { IsBusy = false; };


            Button retButton = new Button
            {
                Text = "返回"
            };
            retButton.Clicked += async (sender, e) =>
            {
                await Currents.Shell.GoBackAsync();
            };


            StackLayout stackLayout = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,

                Children = { _hybridWebView, retButton }
            };

            AbsoluteLayout.SetLayoutBounds(stackLayout, new Rect(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(stackLayout, AbsoluteLayoutFlags.All);

            AbsoluteLayout.SetLayoutBounds(_indicator, new Rect(0.5, 0.5, -1, -1));
            AbsoluteLayout.SetLayoutFlags(_indicator, AbsoluteLayoutFlags.PositionProportional);

            Content = new AbsoluteLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Children = {
                    stackLayout,
                    _indicator
                 }
            };
        }
    }
}