using AsyncAwaitBestPractices;
using HB.FullStack.XamarinForms.Base;
using System;
using System.Collections.Generic;
using System.Windows.Input;

using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.XamarinForms.Controls
{
    public class MultiplePickerDialog : BaseModalDialog
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IList<MultipleListPickerItem>),
            typeof(MultiplePickerDialog),
            null,
            BindingMode.OneWay);

        public static readonly BindableProperty ConfirmCommandProperty = BindableProperty.Create(
            nameof(ConfirmCommand),
            typeof(ICommand),
            typeof(MultiplePickerDialog),
            null,
            BindingMode.OneWay);

        //public static readonly BindableProperty ConfirmCommandParameterProperty = BindableProperty.Create(
        //    nameof(ConfirmCommandParameter),
        //    typeof(object),
        //    typeof(MultiplePickerDialog),
        //    null,
        //    BindingMode.OneWay);

        public IList<MultipleListPickerItem> ItemsSource
        {
            get { return (IList<MultipleListPickerItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public ICommand ConfirmCommand
        {
            get { return (ICommand)GetValue(ConfirmCommandProperty); }
            set { SetValue(ConfirmCommandProperty, value); }
        }

        //public object ConfirmCommandParameter
        //{
        //    get { return GetValue(ConfirmCommandParameterProperty); }
        //    set { SetValue(ConfirmCommandParameterProperty, value); }
        //}

        private readonly MultipleListPicker _multipleListPicker;

        public MultiplePickerDialog()
        {
            Content = new StackLayout { Children = {
                    new MultipleListPicker{ }.Assign(out _multipleListPicker).Bind(MultipleListPicker.ItemsSourceProperty, nameof(ItemsSource)),
                    
                    new Button{ Text="确定" }
                    .BindCommand(nameof(ConfirmCommand), parameterPath:nameof(MultipleListPicker.SelectedIndexes), parameterSource:_multipleListPicker)
                    .Invoke(v=>v.Clicked+=Confirm_Button_Clicked)
                }
            }.Invoke(v => v.BindingContext = this);
        }

        private async void Confirm_Button_Clicked(object sender, EventArgs e)
        {
            await INavigationService.Current.GoBackAsync().ConfigureAwait(false);
        }
    }
}