using AsyncAwaitBestPractices;
using HB.Framework.Client.Base;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.Framework.Client.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MultiplePickerDialog : BaseModalDialog
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

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<MultipleListPickerItem> ItemsSource
#pragma warning restore CA2227 // Collection properties should be read only
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

        public MultiplePickerDialog()
        {
            InitializeComponent();
        }

        private void Confirm_Button_Clicked(object sender, EventArgs e)
        {
            Shell.Current.Navigation.PopModalAsync().Fire();
        }
    }
}