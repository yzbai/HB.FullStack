using CommunityToolkit.Maui.Markup;

using HB.FullStack.Client.Maui.Base;
using HB.FullStack.Common;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace HB.FullStack.Client.Maui.Controls
{
    public class ImageOptionItem : ObservableObject
    {
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
    public class ImageOptions : BaseContentView
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList<ImageOptionItem>), typeof(ImageOptions), propertyChanged: (b, o, n) => ((ImageOptions)b).OnItemsSourceChanged());
        public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(ImageOptions), -1, BindingMode.TwoWay, propertyChanged: (b, o, n) => ((ImageOptions)b).OnSelectedIndexChanged());
        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(ImageOptionItem), typeof(ImageOptions), null, BindingMode.TwoWay, propertyChanged: (b, o, n) => ((ImageOptions)b).OnSelectedItemChanged());
        public static readonly BindableProperty SelectedChangedCommandProperty = BindableProperty.Create(nameof(SelectedChangedCommand), typeof(ICommand), typeof(ImageOptions), null);

        public IList<ImageOptionItem>? ItemsSource { get => (IList<ImageOptionItem>)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

        public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }

        public ImageOptionItem? SelectedItem { get => (ImageOptionItem)GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }

        public ICommand? SelectedChangedCommand { get => (ICommand)GetValue(SelectedChangedCommandProperty); set => SetValue(SelectedChangedCommandProperty, value); }

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
            Content = new CollectionView {
                SelectionMode = SelectionMode.None,
                
                ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical) { }
                .Bind(GridItemsLayout.SpanProperty, nameof(Column))
                .Bind(GridItemsLayout.VerticalItemSpacingProperty, nameof(VerticalItemSpacing)),

                ItemTemplate = new DataTemplate(()=>new StackLayout {
                    Orientation = StackOrientation.Vertical,
                    Children = { 
                        new SelectableImage{ 
                            Aspect = Aspect.AspectFill, 
                            Clip = new EllipseGeometry{ }
                            .Bind(EllipseGeometry.CenterProperty, nameof(ImageCenter), source:this)
                            .Bind(EllipseGeometry.RadiusXProperty, "ImageCenter.X", source:this)
                            .Bind(EllipseGeometry.RadiusYProperty, "ImageCenter.Y", source:this)
                        }
                        .Center()
                        .Bind(SelectableImage.SourceProperty, nameof(ImageOptionItem.UnSelectedImage))
                        .Bind(SelectableImage.SelectedImageSourceProperty, nameof(ImageOptionItem.SelectedImage))
                        .Bind(SelectableImage.IsSelectedProperty, nameof(ImageOptionItem.IsSelected))
                        .Bind(SelectableImage.SelectedCommandProperty, nameof(SingleSelectedCommand), source:this)
                        .Bind(SelectableImage.SelectedCommandParameterProperty, ".")
                        .Bind(SelectableImage.UnSelectedCommandProperty, nameof(SingleUnSelectedCommand), source:this)
                        .Bind(SelectableImage.UnSelectedCommandParameterProperty, ".")
                        .Bind(SelectableImage.HeightRequestProperty, nameof(ImageHeightRequest), source:this)
                        .Bind(SelectableImage.WidthProperty, nameof(ImageWidthRequest), source:this),

                        new Label{ }
                        .Center()
                        .Bind(Label.TextProperty, nameof(ImageOptionItem.Title))
                    }
                }.Center())
            }.Center()
            .Bind(CollectionView.ItemsSourceProperty, nameof(ItemsSource))
            .Invoke(v => v.BindingContext = this);

            SingleSelectedCommand = new Command<ImageOptionItem>(SingleSelected);
            SingleUnSelectedCommand = new Command<ImageOptionItem>(SingleUnSelected);
        }

        private void SingleUnSelected(ImageOptionItem unSelectedItem)
        {
            SelectedItem = null;
        }

        private void SingleSelected(ImageOptionItem selectedItem)
        {
            SelectedItem = selectedItem;
        }

        private void OnItemsSourceChanged()
        {
            SelectedIndex = -1;
        }

        /// <summary>
        /// OnSelectedIndexChanged
        /// </summary>
        
        private void OnSelectedIndexChanged()
        {
            if (ItemsSource == null)
            {
                return;
            }

            if (SelectedIndex == -1)
            {
                SelectedItem = null;
            }
            else if (SelectedIndex > ItemsSource!.Count - 1)
            {
                throw ClientExceptions.ImageOptionsOutOfRange(selectedIndex: SelectedIndex, cause:"ImageOptions SelectedIndex Out of Range.");
            }

            SelectedItem = ItemsSource[SelectedIndex];
        }

        private void OnSelectedItemChanged()
        {
            if (ItemsSource == null)
            {
                SelectedIndex = -1;
                return;
            }

            ImageOptionItem? lastItem = ItemsSource.FirstOrDefault(item => item.IsSelected && item != SelectedItem);

            //反选上一个选择
            if (lastItem != null)
            {
                lastItem.IsSelected = false;

                if (lastItem.UnSelectedCommand != null && lastItem.UnSelectedCommand.CanExecute(lastItem.UnSelectedCommandParameter))
                {
                    lastItem.UnSelectedCommand.Execute(lastItem.UnSelectedCommandParameter);
                }
            }

            //选择当前
            if (SelectedItem == null)
            {
                SelectedIndex = -1;
            }
            else
            {
                if (SelectedItem.SelectedCommand != null && SelectedItem.SelectedCommand.CanExecute(SelectedItem.SelectedCommandParameter))
                {
                    SelectedItem.SelectedCommand.Execute(SelectedItem.SelectedCommandParameter);
                }

                SelectedIndex = ItemsSource.IndexOf(SelectedItem);
            }

            //执行SelectedChangedCommand
            if (SelectedChangedCommand != null && SelectedChangedCommand.CanExecute(SelectedItem))
            {
                SelectedChangedCommand.Execute(SelectedItem);
            }
        }

        public override void OnPageAppearing()
        {
            base.OnPageAppearing();
        }

        public override void OnPageDisappearing()
        {
            base.OnPageDisappearing();
        }
    }
}