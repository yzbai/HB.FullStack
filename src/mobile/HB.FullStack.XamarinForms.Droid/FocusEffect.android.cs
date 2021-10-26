using System;

using Android.Graphics.Drawables;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(HB.FullStack.XamarinForms.Droid.Effects.FocusEffect), nameof(HB.FullStack.XamarinForms.Effects.FocusEffect))]
namespace HB.FullStack.XamarinForms.Droid.Effects
{
    public class FocusEffect : PlatformEffect
    {
        Android.Graphics.Color _originalBackgroundColor = new Android.Graphics.Color(0, 0, 0, 0);
        Android.Graphics.Color _backgroundColor;
        /// <summary>
        /// OnAttached
        /// </summary>
        
        protected override void OnAttached()
        {
            try
            {
                _backgroundColor = Android.Graphics.Color.LightGreen;
                Control.SetBackgroundColor(_backgroundColor);
            }
            catch (Exception ex)
            {
                GlobalSettings.ExceptionHandler.Invoke(ex);
            }
        }

        protected override void OnDetached()
        {
        }

        /// <summary>
        /// OnElementPropertyChanged
        /// </summary>
        /// <param name="args"></param>
        
        protected override void OnElementPropertyChanged(System.ComponentModel.PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);
            try
            {
                if (args.PropertyName == "IsFocused")
                {
                    if(Control.Background is ColorDrawable colorDrawable && colorDrawable.Color == _backgroundColor)
                    {
                        Control.SetBackgroundColor(_originalBackgroundColor);
                    }
                    else
                    {
                        Control.SetBackgroundColor(_backgroundColor);
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalSettings.ExceptionHandler.Invoke(ex);
            }
        }
    }
}
