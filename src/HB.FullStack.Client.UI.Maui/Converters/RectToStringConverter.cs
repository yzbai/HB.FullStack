using System;
using System.Globalization;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace HB.FullStack.Client.UI.Maui.Converters
{
    public class RectToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is Rect rect)
            {
                return $"x:{rect.X}, y:{rect.Y}, width:{rect.Width}, height:{rect.Height}";
            }
            //else if(value is SKRect skRect)
            //{
            //    return $"x:{skRect.Left}, y:{skRect.Top}, width:{skRect.Width}, height:{skRect.Height}";
            //}
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
