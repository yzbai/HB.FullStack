using System.Globalization;

namespace System
{
    public class IntStringConverter : IStringConverter
    {
        public Type ObjectType { get; } = typeof(int);

        public object? ConvertFromString(string? str, StringConvertPurpose purpose)
        {
            if (string.IsNullOrEmpty(str)) return null;

            return int.Parse(str, CultureInfo.InvariantCulture);
        }

        public string? ConvertToString(object? obj, StringConvertPurpose purpose)
        {
            return obj?.ToString();
        }
    }
}
