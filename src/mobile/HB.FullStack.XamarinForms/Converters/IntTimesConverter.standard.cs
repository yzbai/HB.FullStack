using System;
using System.Globalization;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Converters
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
