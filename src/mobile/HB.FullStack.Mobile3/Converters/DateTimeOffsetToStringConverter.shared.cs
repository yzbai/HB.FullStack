using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace HB.FullStack.Mobile.Converters
{
    public class DateTimeOffsetToStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DateTimeOffset dateTimeOffset)
            {
                if(parameter is string format)
                {
                    return dateTimeOffset.ToString(format, culture);
                }

                return dateTimeOffset.ToString(culture);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
