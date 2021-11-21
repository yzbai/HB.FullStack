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
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        /// <summary>
        /// string to enum
        /// </summary>
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if NETSTANDARD2_1
            if (value is string str && Enum.TryParse(targetType, str, out object result))
            {
                return result;
            }

            return null;
#elif NETSTANDARD2_0

            try
            {
                if (value is string str)
                {
                    return Enum.Parse(targetType, str);
                }

                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
#endif
        }
    }
}