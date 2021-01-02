using System;
using System.ComponentModel;
using System.Globalization;
using Xamarin.Forms;

namespace HB.FullStack.Mobile.Converters
{
    public class HasValueConverter : IValueConverter
    {
        public bool EmptyStringIsNull { get; set; } = true;

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (EmptyStringIsNull)
            {
                if (value is string stringValue)
                    return !string.IsNullOrEmpty(stringValue);
            }

            return value != null;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
