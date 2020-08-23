using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace HB.Framework.Client.Converters
{
    public class IntArrayToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IEnumerable<int> array)
            {
                string seprator = ",";
                if (parameter is string sep)
                {
                    seprator = sep;
                }

                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendJoin<int>(seprator, array);

                return stringBuilder.ToString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
