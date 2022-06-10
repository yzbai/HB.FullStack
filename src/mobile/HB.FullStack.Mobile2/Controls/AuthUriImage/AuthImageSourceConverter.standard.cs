using HB.FullStack.XamarinForms.Controls;

using System;
using System.Globalization;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Extensions
{
    public class AuthImageSourceConverter : IValueConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (!Uri.TryCreate(strValue, UriKind.Absolute, out Uri result) || !(result.Scheme != "file"))
                {
                    return ImageSource.FromFile(strValue);
                }

                return new AuthUriImageSource { Uri = result };
            }

            throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(ImageSource)}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }
    }
}
