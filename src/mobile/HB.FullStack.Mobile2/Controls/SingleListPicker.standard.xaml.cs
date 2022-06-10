﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HB.FullStack.XamarinForms.Base;

using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.CommunityToolkit.Markup;

namespace HB.FullStack.XamarinForms.Controls
{
    public class SingleListPicker : BaseContentView
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IList<SingleListPickerItem>),
            typeof(SingleListPicker),
            null,
            BindingMode.OneWay);

        public IList<SingleListPickerItem> ItemsSource
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
            Content = new StackLayout { Children = { 
                    new ListView{ 
                        SelectionMode = ListViewSelectionMode.None,
                        ItemTemplate = new DataTemplate(()=>
                            new ViewCell{ View = new StackLayout{ 
                                Orientation = StackOrientation.Horizontal,
                                Children={ 
                                    new Label{ }.StartExpand().TextCenterVertical().Bind(Label.TextProperty, nameof(SingleListPickerItem.Text)),

                                    new RadioButton{ }.EndExpand()
                                    .Bind(RadioButton.IsCheckedProperty, nameof(SingleListPickerItem.IsChecked))
                                    .Bind(RadioButton.GroupNameProperty, nameof(GroupName), source: this)
                                    .Invoke(view=>view.CheckedChanged += RadioButton_CheckedChanged)
                                }
                            }
                        })
                    }
                    .Bind(ListView.ItemsSourceProperty, nameof(ItemsSource))
                    .Invoke(lst=>lst.ItemTapped += RadioList_ItemTapped)
                }
            }.Invoke(layout=>layout.BindingContext = this);

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