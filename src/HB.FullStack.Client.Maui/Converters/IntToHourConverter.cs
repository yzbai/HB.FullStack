using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.Maui.Converters
{
    /// <summary>
    /// 将Int值表示为两位的字符串
    /// 7 --> 07
    /// </summary>
    public class IntToHourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int time = (int)value;

            return string.Format(GlobalSettings.Culture, "{0:d2}", time);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value, GlobalSettings.Culture);
        }
    }
}
