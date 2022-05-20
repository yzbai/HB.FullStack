using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.UI.Maui.Converters
{
    /// <summary>
    /// 将Boolean值转换为“上午” 和 “下午”
    /// true：上午
    /// false：下午
    /// </summary>
    public class BooleanToAmPmConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isAm = (bool)value;

            return isAm ? "上午" : "下午";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? text = value?.ToString();

            return "上午".Equals(text, GlobalSettings.Comparison);
        }
    }
}
