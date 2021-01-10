using System;
using System.Collections.Generic;
using System.Text;
using HB.FullStack.Client.Base;
using Xamarin.Forms;

namespace HB.FullStack.Client.Behaviors
{
    public class EntryLineValidationBehavior : BaseBehavior<Entry>
    {
        public static readonly BindableProperty IsValidProperty = BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(EntryLineValidationBehavior), true, propertyChanged: OnIsValidChanged);

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }

            set { SetValue(IsValidProperty, value); }
        }

        private static void OnIsValidChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is EntryLineValidationBehavior behavior && newValue is bool isValid)
            {
                behavior.AssociatedObject.PlaceholderColor = isValid ? Color.Default : Color.Red;
            }
        }
    }
}
