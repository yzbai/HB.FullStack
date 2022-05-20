using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.UI.Maui.Converters
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
            if (value is string str && Enum.TryParse(targetType, str, out object? result))
            {
                return result;
            }

            return null;
        }
    }
}