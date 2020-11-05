using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
        public int Index { get; set; }

        public string? Title { get; set; }

        public string? UnSelectedImage { get; set; }

        public string? SelectedImage { get; set; }

        public ICommand? SelectedCommand { get; set; }

        public object? SelectedCommandParameter { get; set; }

        public ICommand? UnSelectedCommand { get; set; }

        public object? UnSelectedCommandParameter { get; set; }

        private bool _isSelected;

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

        public object? Tag { get; set; }
    }

    /// <summary>
    /// 单选图形列表
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImageOptions : BaseContentView
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList<ImageOptionItem>), typeof(ImageOptions), null);
        public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(ImageOptions), -1, BindingMode.TwoWay, propertyChanged: (b, o, n) => ((ImageOptions)b).OnSelectedIndexChanged());

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        public IList<ImageOptionItem>? ItemsSource { get => (IList<ImageOptionItem>)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

        public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }

        public ImageOptionItem? SelectedItem { get => SelectedIndex == -1 ? null : ItemsSource![SelectedIndex]; }

        public ICommand SingleSelectedCommand { get; private set; }

        public ICommand SingleUnSelectedCommand { get; private set; }

        private int _imageHeightRequest = 80;
        public int ImageHeightRequest { get { return _imageHeightRequest; } set { _imageHeightRequest = value; OnPropertyChanged(); } }

        private int _imageWidthRequest = 80;
        public int ImageWidthRequest { get { return _imageWidthRequest; } set { _imageWidthRequest = value; OnPropertyChanged(); } }

        private int _verticalItemSpacing = 24;
        public int VerticalItemSpacing { get { return _verticalItemSpacing; } set { _verticalItemSpacing = value; OnPropertyChanged(); } }

        private int _column = 2;
        public int Column { get { return _column; } set { _column = value; OnPropertyChanged(); } }

        public Point ImageCenter { get => new Point(ImageWidthRequest / 2.0, ImageHeightRequest / 2.0); }

        public ImageOptions()
        {
            InitializeComponent();

            SingleSelectedCommand = new Command<int>(SingleSelected);
            SingleUnSelectedCommand = new Command<int>(SingleUnSelected);
        }

        private void SingleUnSelected(int index)
        {
            SelectedIndex = -1;
        }

        private void SingleSelected(int index)
        {
            SelectedIndex = index;
        }

        private void OnSelectedIndexChanged()
        {
            ImageOptionItem? lastItem = ItemsSource.FirstOrDefault(item => item.IsSelected && item.Index != SelectedIndex);

            if (lastItem != null)
            {
                lastItem.IsSelected = false;

                if (lastItem.UnSelectedCommand != null && lastItem.UnSelectedCommand.CanExecute(lastItem.UnSelectedCommandParameter))
                {
                    lastItem.UnSelectedCommand.Execute(lastItem.UnSelectedCommandParameter);
                }
            }

            if (SelectedIndex == -1)
            {
                return;
            }

            ImageOptionItem curItem = ItemsSource.First(item => item.Index == SelectedIndex);

            if (curItem.SelectedCommand != null && curItem.SelectedCommand.CanExecute(curItem.SelectedCommandParameter))
            {
                curItem.SelectedCommand.Execute(curItem.SelectedCommandParameter);
            }
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        public override IList<IBaseContentView?>? GetAllCustomerControls()
        {
            return null;
        }
    }
}