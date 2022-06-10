using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Converters
{
    public class EnumStringConverter : IValueConverter
    {
        /// <summary>
        /// Enum to string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        /// <summary>
        /// string to enum
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && Enum.TryParse(targetType, str, out object result))
            {
                return result;
            }

            return null;
        }
    }
}
