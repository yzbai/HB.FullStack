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

        public static Action<Exception?, string?, LogLevel> MessageExceptionHandler { get; set; } = (ex, msg, level) => { Logger.Log(level, ex, $"开火失败 : {msg}"); };

        public static Action<Exception> ExceptionHandler { get; } = ex => MessageExceptionHandler.Invoke(ex, $"开火失败", LogLevel.Error);

        public static readonly string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static readonly string LogTag = "=HB_HB_HB=";
    }
}

#nullable restore