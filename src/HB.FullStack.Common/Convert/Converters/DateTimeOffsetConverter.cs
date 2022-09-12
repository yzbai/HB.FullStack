using System;
using System.Globalization;
using System.Web;

namespace HB.FullStack.Common.Convert.Converters
{
    public class DateTimeOffsetConverter : IStringConverter
    {
        public Type ObjectType { get; } = typeof(DateTimeOffset);

        public object? ConvertFromString(string? str, StringConvertPurpose purpose)
        {
            if (str == null)
            {
                return null;
            }

            string str2 = str;

            switch (purpose)
            {
                case StringConvertPurpose.NONE:
                    break;
                case StringConvertPurpose.HTTP_QUERY:
                    str2 = HttpUtility.UrlDecode(str);
                    break;
                default:
                    break;
            }

            return DateTimeOffset.Parse(str2, CultureInfo.InvariantCulture);
        }

        public string? ConvertToString(object? obj, StringConvertPurpose purpose)
        {
            string? str = obj?.ToString();

            return purpose switch
            {
                StringConvertPurpose.NONE => str,
                StringConvertPurpose.HTTP_QUERY => HttpUtility.UrlEncode(str),
                _ => str,
            };
        }
    }

}
