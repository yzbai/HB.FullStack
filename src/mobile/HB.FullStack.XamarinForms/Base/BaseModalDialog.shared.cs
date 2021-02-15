using AsyncAwaitBestPractices;
using HB.FullStack.XamarinForms.Effects;
using HB.FullStack.XamarinForms.Effects.Touch;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.XamarinForms.Base
{
    [ContentProperty("Content")]
    public class BaseModalDialog : BaseContentPage
    {
        private Frame? _dialogFrame;
        public bool IsBackgroudClickedToDismiss { get; set; }

        private ControlTemplate _controlTemplate;

        public BaseModalDialog()
        {
            _controlTemplate = new ControlTemplate(() => new StackLayout { 
                Children = { 
                    new Frame{ 
                        CornerRadius = 10,
                        HasShadow = true,
                        BackgroundColor = Color.White,
                        Content = new StackLayout{ 
                            Children = { 
                                new ContentPresenter()
                            }
                        }
                    }.Width(280).CenterExpand()
                }
            });

            BackgroundColor = Color.FromHex("#80000000");
            //ControlTemplate = (ControlTemplate)Resources["DialogControlTemplate"];
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ControlTemplate = _controlTemplate;
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

        private void TouchEffect_TouchAction(object? sender, TouchActionEventArgs args)
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

                if (Shell.Current != null)
                {
                    Shell.Current.Navigation.PopModalAsync(false).Fire();
                }
                else
                {
                    Navigation.PopModalAsync(false).Fire();
                }

            }
        }

        protected override IList<IBaseContentView?>? GetAllCustomerControls()
        {
            return null;
        }
    }
}