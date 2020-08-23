using System;
using System.Windows.Input;
using HB.Framework.Client.Base;
using HB.Framework.Client.Styles;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.Framework.Client.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommandCell : BaseContentView
    {
        public string ChevronRight { get; } = MaterialFont.ChevronRight;

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
            InitializeComponent();
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
        }

        public override void OnDisappearing()
        {
        }
    }
}