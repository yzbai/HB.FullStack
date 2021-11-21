using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.CommunityToolkit.Effects;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace HB.FullStack.XamarinForms.Base
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseModalDialog : BaseContentPage
    {
        //private Frame? _dialogFrame;

        private ICommand DismissCommand { get; set; }

        public bool IsBackgroudClickedToDismiss { get; set; }

        public BaseModalDialog()
        {
            InitializeComponent();
            
            Shell.SetPresentationMode(this, PresentationMode.ModalAnimated);

            BackgroundColor = Color.FromHex("#80000000");

            DismissCommand = new AsyncCommand(OnDismissAsync);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ControlTemplate = (ControlTemplate)Resources["DialogControlTemplate"];
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //_dialogFrame = (Frame)GetTemplateChild("ModalDialogFrame");

            StackLayout modalDialogContainer = (StackLayout)GetTemplateChild("ModalDialogContainer");

            TouchEffect.SetCommand(modalDialogContainer, DismissCommand);
            TouchEffect.SetShouldMakeChildrenInputTransparent(modalDialogContainer, false);
        }

        private async Task OnDismissAsync()
        {
            if (!IsBackgroudClickedToDismiss)
            {
                return;
            }


            await NavigationService.Current.GoBackAsync().ConfigureAwait(false);
        }

        protected override IList<IBaseContentView?>? GetAllCustomerControls() => null;
    }
}