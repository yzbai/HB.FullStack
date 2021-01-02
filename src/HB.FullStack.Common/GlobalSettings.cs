#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Microsoft.Extensions.Logging;

namespace System
{
    public static class GlobalSettings
    {
        [NotNull] public static CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        [NotNull] public static StringComparison Comparison { get; set; } = StringComparison.InvariantCulture;

        [NotNull] public static StringComparison ComparisonIgnoreCase { get; set; } = StringComparison.InvariantCultureIgnoreCase;

        [MaybeNull, DisallowNull] public static ILogger? Logger { get; set; }

        public static readonly string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static readonly string LogTag = "=HB_HB_HB=";

        public static void ExceptionHandler(Exception ex, string? message)
        {
            Logger.LogError(ex, message);
        }
    }
}

#nullable restore