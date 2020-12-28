using System;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(HB.FullStack.Client.Droid.Effects.FocusEffect), nameof(HB.FullStack.Client.Effects.FocusEffect))]
namespace HB.FullStack.Client.Droid.Effects
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
                Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
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
                    if (((Android.Graphics.Drawables.ColorDrawable)Control.Background).Color == _backgroundColor)
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
                Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
            }
        }
    }
}
