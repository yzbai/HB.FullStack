using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using System.Linq;

namespace HB.Framework.Client.Converters
{
    /// <summary>
    /// 获取viewmodel里的验证结果，parameter为key
    /// </summary>
    public class ValidationResultConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<ValidationResult> results && parameter is string key)
            {
                return results.ErrorMessageOf(key);
            }

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// 获取是否有错误
    /// </summary>
    public class ValidationResultExistConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<ValidationResult> results && parameter is string key)
            {
                bool flag = true;

                if (key.StartsWith('!'))
                {
                    flag = false;
                    key = key.Substring(1);
                }

                bool existed = results.ExistErrorOf(key);

                return flag ? existed : !existed;
            }

            return false;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
