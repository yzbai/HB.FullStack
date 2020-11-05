using System;
using System.Globalization;
using Xamarin.Forms;

namespace HB.Framework.Client.Converters
{
    public class TupleMultipleBindingConverter : IMultiValueConverter
    {
        public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values != null)
            {
                return values.Length switch
                {
                    0 => null,
                    1 => values[0],
                    2 => new Tuple<object?, object?>(values[0], values[1]),
                    3 => new Tuple<object?, object?, object?>(values[0], values[1], values[2]),
                    4 => new Tuple<object?, object?, object?, object?>(values[0], values[1], values[2], values[3]),
                    5 => new Tuple<object?, object?, object?, object?, object?>(values[0], values[1], values[2], values[3], values[4]),
                    6 => new Tuple<object?, object?, object?, object?, object?, object?>(values[0], values[1], values[2], values[3], values[4], values[5]),
                    7 => new Tuple<object?, object?, object?, object?, object?, object?, object?>(values[0], values[1], values[2], values[3], values[4], values[5], values[6]),
                    8 => new Tuple<object?, object?, object?, object?, object?, object?, object?, object?>(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]),
                    _ => null,
                };
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
