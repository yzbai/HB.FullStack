using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.XamarinForms.Effects.Touch;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.XamarinForms.Base
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseModalDialog : BaseContentPage
    {
        private Frame? _dialogFrame;

        public bool IsBackgroudClickedToDismiss { get; set; }

        public BaseModalDialog()
        {
            InitializeComponent();

            BackgroundColor = Color.FromHex("#80000000");
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

            if (IsBackgroudClickedToDismiss)
            {
                TouchEffect touchEffect = new TouchEffect() { Capture = false };
                touchEffect.TouchAction += TouchEffect_TouchAction;

                modalDialogContainer.Effects.Add(touchEffect);
            }
        }

        private void TouchEffect_TouchAction(object sender, TouchActionEventArgs args)
        {
            if (_dialogFrame == null)
            {
                return;
            }

            Rectangle rectangle = new Rectangle(_dialogFrame.X, _dialogFrame.Y, _dialogFrame.Width, _dialogFrame.Height);

            if (rectangle.Contains(args.Location))
            {
                return;
            }

            if (args.Type == TouchActionType.Released)
            {
                NavigationService.Current.PopModal();
            }
        }

        protected override IList<IBaseContentView?>? GetAllCustomerControls() => null;
    }
}