using Microsoft.Maui.Controls;

using System;
using System.Windows.Input;

namespace HB.FullStack.Client.Maui.Controls
{
    public class SelectableImage : Image
    {
        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(SelectableImage), false, BindingMode.TwoWay, propertyChanged: (b, o, n) => ((SelectableImage)b).OnIsSelectedChagned());
        public static readonly BindableProperty SelectedImageSourceProperty = BindableProperty.Create(nameof(SelectedImageSource), typeof(ImageSource), typeof(SelectableImage), null);
        public static readonly BindableProperty SelectedCommandProperty = BindableProperty.Create(nameof(SelectedCommand), typeof(ICommand), typeof(SelectableImage), null);
        public static readonly BindableProperty UnSelectedCommandProperty = BindableProperty.Create(nameof(UnSelectedCommand), typeof(ICommand), typeof(SelectableImage), null);
        public static readonly BindableProperty SelectedCommandParameterProperty = BindableProperty.Create(nameof(SelectedCommandParameter), typeof(object), typeof(SelectableImage), null);
        public static readonly BindableProperty UnSelectedCommandParameterProperty = BindableProperty.Create(nameof(UnSelectedCommandParameter), typeof(object), typeof(SelectableImage), null);

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
            tapGestureRecognizer.Tapped += OnTapGestureRecognizerTapped;

            GestureRecognizers.Add(tapGestureRecognizer);
        }

        private void OnTapGestureRecognizerTapped(object? sender, EventArgs e)
        {
            IsSelected = !IsSelected;
        }

        private void OnIsSelectedChagned()
        {
            //Source
            if (UnSelectedImageSource == null)
            {
                UnSelectedImageSource = Source;
            }

            if (IsSelected && SelectedImageSource != null)
            {
                Source = SelectedImageSource;
            }

            if (!IsSelected && UnSelectedImageSource != null)
            {
                Source = UnSelectedImageSource;
            }

            //Command
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
