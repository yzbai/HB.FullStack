using Microsoft.Maui.Controls;

using System;
using System.Globalization;

namespace HB.FullStack.Client.Maui.Converters
{
    /// <summary>
    /// Int值的倍数，parameter表示几倍
    /// </summary>
    public class IntTimesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int original = (int)value;
            double factor = (double)parameter;
            return Math.Floor(original * factor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int original = (int)value;
            double factor = (double)parameter;
            return Math.Floor(original / factor);
        }
    }
}
