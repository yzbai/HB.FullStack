using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HB.FullStack.Mobile.Base;

using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.Mobile.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SingleListPicker : BaseContentView
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IList<SingleListPickerItem>),
            typeof(SingleListPicker),
            null,
            BindingMode.OneWay);

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<SingleListPickerItem> ItemsSource
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get { return (IList<SingleListPickerItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private int _selectedIndexe;

        /// <summary>
        /// 选择结果字符串
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectedIndexe; }
            set { _selectedIndexe = value; OnPropertyChanged(); }
        }

        public string GroupName { get; } = SecurityUtil.CreateUniqueToken();

        public SingleListPicker()
        {
            InitializeComponent();
        }

        private void RadioList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ItemsSource[e.ItemIndex].IsChecked = !ItemsSource[e.ItemIndex].IsChecked;
        }

        private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (ItemsSource == null)
            {
                return;
            }

            for (int i = 0; i < ItemsSource.Count; ++i)
            {
                if (ItemsSource[i].IsChecked)
                {
                    SelectedIndex = i;
                    return;
                }
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

    public class SingleListPickerItem : ObservableObject
    {
        private string? _text;

        public string? Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }
    }
}