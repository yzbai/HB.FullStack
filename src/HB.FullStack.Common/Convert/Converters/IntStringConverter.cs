using System;

namespace HB.FullStack.Common.Convert.Converters
{
    public class IntStringConverter : IStringConverter
    {
        public Type ObjectType { get; } = typeof(int);

        public object? ConvertFromString(string? str, StringConvertPurpose purpose)
        {
            if (str.IsNullOrEmpty()) return null;

            return int.Parse(str, Globals.Culture);
        }

        public string? ConvertToString(object? obj, StringConvertPurpose purpose)
        {
            return obj?.ToString();
        }
    }

}
