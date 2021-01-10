using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace HB.FullStack.Client.Converters
{
    /// <summary>
    /// 将IEnumerable<string> 转换为用逗号“,”链接的字符串
    /// </summary>
    public class StringCollectionJoinConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<string> lst)
            {
                return string.Join(',', lst);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
