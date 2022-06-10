using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace HB.FullStack.Client.Maui.Converters
{
    public class IntTupleToColonStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Tuple<int, int> tuple)
            {
                return $"{tuple.Item1}:{tuple.Item2}";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
