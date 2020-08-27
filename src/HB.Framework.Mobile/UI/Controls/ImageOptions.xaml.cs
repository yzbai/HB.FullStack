using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HB.Framework.Client.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.Framework.Client.Controls
{
    public class ImageOptionItem : ObservableObject
    {
        public int Key { get; set; }

        public string? Title { get; set; }

        public string? UnSelectedImage { get; set; }

        public string? SelectedImage { get; set; }

        public ICommand? SelectedCommand { get; set; }

        public object? SelectedCommandParameter { get; set; }

        public ICommand? UnSelectedCommand { get; set; }

        public object? UnSelectedCommandParameter { get; set; }

        private bool _isSelected;

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImageOptions : BaseContentView
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList<ImageOptionItem>), typeof(ImageOptions), null);
        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(ImageOptionItem), typeof(ImageOptions), null, BindingMode.TwoWay);

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ImageOptionItem>? ItemsSource { get => (IList<ImageOptionItem>)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
#pragma warning restore CA2227 // Collection properties should be read only

        public ImageOptionItem? SelectedItem { get => (ImageOptionItem)GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }

        public ICommand SingleSelectedCommand { get; private set; }

        public int ImageHeightRequest { get; set; } = 80;

        public int ImageWidthRequest { get; set; } = 80;

        public int VerticalItemSpacing { get; set; } = 24;

        public int Column { get; set; } = 2;

        public Point ImageCenter { get => new Point(ImageWidthRequest / 2.0, ImageHeightRequest / 2.0); }



        public ImageOptions()
        {
            InitializeComponent();

            SingleSelectedCommand = new Command<int>(Action_SingleSelectedCommand);
        }

        private void Action_SingleSelectedCommand(int key)
        {
            ImageOptionItem curItem = ItemsSource.First(item => item.Key == key);

            if (!curItem.IsSelected)
            {
                SelectedItem = null;

                if (curItem.UnSelectedCommand != null && curItem.UnSelectedCommand.CanExecute(curItem.UnSelectedCommandParameter))
                {
                    curItem.UnSelectedCommand.Execute(curItem.UnSelectedCommandParameter);
                }
            }
            else
            {
                ImageOptionItem? lastItem = ItemsSource.FirstOrDefault(item => item.IsSelected && item.Key != key);

                if (lastItem != null)
                {
                    lastItem.IsSelected = false;

                    if (lastItem.UnSelectedCommand != null && lastItem.UnSelectedCommand.CanExecute(lastItem.UnSelectedCommandParameter))
                    {
                        lastItem.UnSelectedCommand.Execute(lastItem.UnSelectedCommandParameter);
                    }
                }

                SelectedItem = curItem;

                if (curItem.SelectedCommand != null && curItem.SelectedCommand.CanExecute(curItem.SelectedCommandParameter))
                {
                    curItem.SelectedCommand.Execute(curItem.SelectedCommandParameter);
                }
            }
        }

        public override void OnAppearing()
        {

        }

        public override void OnDisappearing()
        {

        }
    }
}