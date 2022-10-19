using System;
using System.Web;

namespace HB.FullStack.Common.Convert.Converters
{
    public class StringStringConverter : IStringConverter
    {
        public Type ObjectType { get; } = typeof(string);

        public object? ConvertFromString(string? str, StringConvertPurpose purpose)
        {
            return purpose switch
            {
                StringConvertPurpose.NONE => str,
                StringConvertPurpose.HTTP_QUERY => HttpUtility.UrlDecode(str),
                _ => throw new NotImplementedException(),
            };
        }

        public string? ConvertToString(object? obj, StringConvertPurpose purpose)
        {
            return purpose switch
            {
                StringConvertPurpose.NONE => (string?)obj,
                StringConvertPurpose.HTTP_QUERY => HttpUtility.UrlEncode((string?)obj),
                _ => throw new NotImplementedException(),
            };
        }
    }

}
