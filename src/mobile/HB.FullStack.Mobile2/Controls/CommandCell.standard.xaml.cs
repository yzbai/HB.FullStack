using System;
using System.Collections.Generic;
using System.Windows.Input;

using HB.FullStack.XamarinForms.Base;
using HB.FullStack.XamarinForms.Styles;

using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace HB.FullStack.XamarinForms.Controls
{
    public class CommandCell : BaseContentView
    {
        //public string ChevronRight { get; } = MaterialFont.ChevronRight;

        #region Input

        //public static readonly BindableProperty MarginProperty = BindableProperty.Create(
        //    nameof(Margin),
        //    typeof(Thickness),
        //    typeof(CommandCell),
        //    null,
        //    BindingMode.OneWay);

        //public Thickness Margin
        //{
        //    get { return (Thickness)GetValue(MarginProperty); }
        //    set { SetValue(MarginProperty, value); }
        //}

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(CommandCell),
            null,
            BindingMode.OneWay);

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
           nameof(Command),
           typeof(ICommand),
           typeof(CommandCell),
           null,
           BindingMode.OneWay);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
           nameof(CommandParameter),
           typeof(ICommand),
           typeof(CommandCell),
           null,
           BindingMode.OneWay);

        public object CommandParameter
        {
            get { return (ICommand)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        #endregion

        #region Output
        #endregion

        #region InputOutput

        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(CommandCell),
            null,
            BindingMode.TwoWay);

        public string? Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        public CommandCell()
        {
            Content = new Grid
            {
                RowDefinitions = Rows.Define(Star),
                ColumnDefinitions = Columns.Define(Auto, Star, Auto),
                Children = {
                    new Label{ }
                    .Row(0)
                    .Column(0)
                    .Start()
                    .Font(size:Device.GetNamedSize(NamedSize.Medium, typeof(Label)))
                    .TextCenterVertical()
                    .Bind(Label.TextProperty, nameof(Title)),

                    new Label{ TextColor = Color.Gray }
                    .Row(0)
                    .Column(1)
                    .End()
                    .Font(size:Device.GetNamedSize(NamedSize.Small, typeof(Label)))
                    .TextCenterVertical()
                    .Bind(Label.TextProperty, nameof(Text)),

                    new Label{ Text = MaterialFont.ChevronRight }
                    .Row(0)
                    .Column(2)
                    .EndExpand()
                    .TextCenterVertical()
                    .Font("MaterialIcon", Device.GetNamedSize(NamedSize.Medium, typeof(Label)))
                }
            }.Bind(Grid.MarginProperty, nameof(Margin))
            .Invoke(v => v.GestureRecognizers.Add(new TapGestureRecognizer { }.Invoke(v => v.Tapped += TapGestureRecognizer_Tapped)))
            .Invoke(v => v.BindingContext = this);
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
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