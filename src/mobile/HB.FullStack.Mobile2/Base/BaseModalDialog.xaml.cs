using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.CommunityToolkit.Effects;
using System.Windows.Input;

namespace HB.FullStack.XamarinForms.Base
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseModalDialog : BaseContentPage
    {
        private Frame? _dialogFrame;

        private ICommand DismissCommand { get; set; }

        public bool IsBackgroudClickedToDismiss { get; set; }

        public BaseModalDialog()
        {
            InitializeComponent();

            BackgroundColor = Color.FromHex("#80000000");

            DismissCommand = new Command(OnDismiss);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ControlTemplate = (ControlTemplate)Resources["DialogControlTemplate"];
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dialogFrame = (Frame)GetTemplateChild("ModalDialogFrame");

            StackLayout modalDialogContainer = (StackLayout)GetTemplateChild("ModalDialogContainer");

            TouchEffect.SetCommand(modalDialogContainer, DismissCommand);
            TouchEffect.SetShouldMakeChildrenInputTransparent(modalDialogContainer, false);
        }

        private void OnDismiss()
        {
            if (!IsBackgroudClickedToDismiss)
            {
                return;
            }

            NavigationService.Current.PopModal();
        }

        protected override IList<IBaseContentView?>? GetAllCustomerControls() => null;
    }
}