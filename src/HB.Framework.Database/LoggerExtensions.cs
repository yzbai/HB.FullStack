using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggerExtensions
    {
        internal static void Error_BatchDelete_Thrown(this ILogger logger, Exception ex, string lastUser)
        {
            logger.LogCritical(ex, $"LastUser : {lastUser}");
        }
    }
}
