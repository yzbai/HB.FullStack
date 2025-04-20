using System;
using System.Web;

namespace System
{
    public class DefaultStringConverter : IStringConverter
    {
        public Type ObjectType { get; }

        public object? ConvertFromString(string? str, StringConvertPurpose purpose)
        {
            throw new NotImplementedException();
        }

        public string? ConvertToString(object? obj, StringConvertPurpose purpose)
        {
            return purpose switch
            {
                StringConvertPurpose.NONE => obj?.ToString(),
                StringConvertPurpose.HTTP_QUERY => HttpUtility.UrlEncode(obj?.ToString()),
                _ => throw new NotImplementedException(),
            };
        }

        public DefaultStringConverter(Type objectType)
        {
            ObjectType = objectType;
        }
    }
}
