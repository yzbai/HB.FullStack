using System.Globalization;

namespace System
{
    public static class GlobalSettings
    {
        //private static CultureInfo GlobalSettings.Culture = CultureInfo.InvariantCulture;
        //private static StringComparison _comparison = StringComparison.InvariantCulture;
        //private static StringComparison _comparisonIgnoreCase = StringComparison.InvariantCultureIgnoreCase;

        //public static CultureInfo Culture { get => GlobalSettings.Culture; }

        //public static StringComparison Comparison { get => _comparison; }

        //public static StringComparison ComparisonIgnoreCase { get => _comparisonIgnoreCase; }

        //public static void SetCulture(CultureInfo cultureInfo)
        //{
        //    GlobalSettings.Culture = cultureInfo;
        //}

        //public static void SetComparison(StringComparison stringComparison)
        //{
        //    _comparison = stringComparison;
        //}

        //public static void SetComparisonIgnoreCase(StringComparison stringComparison)
        //{
        //    _comparisonIgnoreCase = stringComparison;
        //}

        public static CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        public static StringComparison Comparison { get; set; } = StringComparison.InvariantCulture;

        public static StringComparison ComparisonIgnoreCase { get; set; } = StringComparison.InvariantCultureIgnoreCase;

    }
}
