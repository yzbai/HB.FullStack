using System;
using System.Globalization;

namespace HB.FullStack.Common.Convert.Converters
{
    public class GuidStringConverter : IStringConverter
    {
        public Type ObjectType { get; } = typeof(Guid);

        public object? ConvertFromString(string? str, StringConvertPurpose purpose)
        {
            return str == null ? null : Guid.Parse(str);
        }

        public string? ConvertToString(object? obj, StringConvertPurpose purpose)
        {
            return obj?.ToString();
        }
    }

}
