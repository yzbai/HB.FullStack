using System.Globalization;

namespace System
{
    public static class GlobalSettings
    {
        public static CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        public static StringComparison Comparison { get; set; } = StringComparison.InvariantCulture;

        public static StringComparison ComparisonIgnoreCase { get; set; } = StringComparison.InvariantCultureIgnoreCase;

    }
}
