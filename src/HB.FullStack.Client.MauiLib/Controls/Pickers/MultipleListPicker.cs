﻿/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.Collections.Generic;

using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Mvvm.ComponentModel;

using HB.FullStack.Client.MauiLib.Base;
using HB.FullStack.Common;

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.MauiLib.Controls
{
    public class MultipleListPicker : BaseView
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IList<MultipleListPickerItem>),
            typeof(MultipleListPicker),
            null,
            BindingMode.OneWay);

        public IList<MultipleListPickerItem> ItemsSource
        {
            get { return (IList<MultipleListPickerItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private IList<int> _selectedIndexes = new List<int>();

        public IList<int> SelectedIndexes
        {
            get { return _selectedIndexes; }
            set { _selectedIndexes = value; OnPropertyChanged(); }
        }

        public MultipleListPicker()
        {
            Content = new StackLayout
            {
                Children = {
                    new ListView{
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                        SelectionMode = ListViewSelectionMode.None,
                        ItemTemplate = new DataTemplate(()=>new ViewCell{View = new StackLayout{
                            Orientation = StackOrientation.Horizontal,
                            Children={
                                new Label{ }.Start().TextCenterVertical().Bind(Label.TextProperty, nameof(MultipleListPickerItem.Text)),
                                new CheckBox{ }.End().Bind(CheckBox.IsCheckedProperty, nameof(MultipleListPickerItem.IsChecked)).Invoke(v=>v.CheckedChanged+=CheckBox_CheckedChanged)
                            }
                        } })
                    }.Bind(ListView.ItemsSourceProperty, nameof(ItemsSource))
                    .Invoke(v=>v.ItemTapped+=ListView_ItemTapped)
                }
            }.Top().Invoke(v => v.BindingContext = this);
        }

        private void ListView_ItemTapped(object? sender, ItemTappedEventArgs e)
        {
            ItemsSource[e.ItemIndex].IsChecked = !ItemsSource[e.ItemIndex].IsChecked;
        }

        private void CheckBox_CheckedChanged(object? sender, CheckedChangedEventArgs e)
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

        public override void OnPageAppearing()
        {
            base.OnPageAppearing();
        }

        public override void OnPageDisappearing()
        {
            base.OnPageDisappearing();
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