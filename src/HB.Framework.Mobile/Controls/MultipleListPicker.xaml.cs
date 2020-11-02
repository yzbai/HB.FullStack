using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HB.Framework.Client.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.Framework.Client.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MultipleListPicker : BaseContentView
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IList<MultipleListPickerItem>),
            typeof(MultipleListPicker),
            null,
            BindingMode.OneWay);

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<MultipleListPickerItem> ItemsSource
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get { return (IList<MultipleListPickerItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private IList<int> _selectedIndexes = new List<int>();

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<int> SelectedIndexes
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get { return _selectedIndexes; }
            set { _selectedIndexes = value; OnPropertyChanged(); }
        }

        public MultipleListPicker()
        {
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ItemsSource[e.ItemIndex].IsChecked = !ItemsSource[e.ItemIndex].IsChecked;
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            SelectedIndexes = new List<int>();

            for (int i = 0; i < ItemsSource.Count; ++i)
            {
                if (ItemsSource[i].IsChecked)
                {
                    SelectedIndexes.Add(i);
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

    public class MultipleListPickerItem : ObservableObject
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