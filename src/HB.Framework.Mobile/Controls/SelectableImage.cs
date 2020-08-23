﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace HB.Framework.Client.Controls
{
    public class SelectableImage : Image
    {
        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(SelectableImage), false, BindingMode.TwoWay, propertyChanged: OnIsSelectedChanged);
        public static readonly BindableProperty SelectedImageSourceProperty = BindableProperty.Create(nameof(SelectedImageSource), typeof(ImageSource), typeof(SelectableImage), null);

        public static readonly BindableProperty SelectedCommandProperty = BindableProperty.Create(nameof(SelectedCommand), typeof(ICommand), typeof(SelectableImage), null);
        public static readonly BindableProperty UnSelectedCommandProperty = BindableProperty.Create(nameof(UnSelectedCommand), typeof(ICommand), typeof(SelectableImage), null);

        public static readonly BindableProperty SelectedCommandParameterProperty = BindableProperty.Create(nameof(SelectedCommandParameter), typeof(object), typeof(SelectableImage), null);
        public static readonly BindableProperty UnSelectedCommandParameterProperty = BindableProperty.Create(nameof(UnSelectedCommandParameter), typeof(object), typeof(SelectableImage), null);

        private static void OnIsSelectedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SelectableImage image)
            {
                if (image.UnSelectedImageSource == null)
                {
                    image.UnSelectedImageSource = image.Source;
                }

                if (image.IsSelected && image.SelectedImageSource != null)
                {
                    image.Source = image.SelectedImageSource;
                }

                if (!image.IsSelected && image.UnSelectedImageSource != null)
                {
                    image.Source = image.UnSelectedImageSource;
                }
            }
        }

        public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }

        public ImageSource? SelectedImageSource { get => (ImageSource)GetValue(SelectedImageSourceProperty); set => SetValue(SelectedImageSourceProperty, value); }

        public ImageSource? UnSelectedImageSource { get; set; }

        public ICommand? SelectedCommand { get => (ICommand)GetValue(SelectedCommandProperty); set => SetValue(SelectedCommandProperty, value); }

        public ICommand? UnSelectedCommand { get => (ICommand)GetValue(UnSelectedCommandProperty); set => SetValue(UnSelectedCommandProperty, value); }

        public object? SelectedCommandParameter { get => (object)GetValue(SelectedCommandParameterProperty); set => SetValue(SelectedCommandParameterProperty, value); }

        public object? UnSelectedCommandParameter { get => (object)GetValue(UnSelectedCommandParameterProperty); set => SetValue(UnSelectedCommandParameterProperty, value); }

        public SelectableImage()
        {
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;

            GestureRecognizers.Add(tapGestureRecognizer);
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            IsSelected = !IsSelected;

            if (IsSelected && SelectedCommand != null)
            {
                if (SelectedCommand.CanExecute(SelectedCommandParameter))
                {
                    SelectedCommand.Execute(SelectedCommandParameter);
                }
            }

            else if (!IsSelected && UnSelectedCommand != null)
            {
                if (UnSelectedCommand.CanExecute(UnSelectedCommandParameter))
                {
                    UnSelectedCommand.Execute(UnSelectedCommandParameter);
                }
            }
        }
    }
}
