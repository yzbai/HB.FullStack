using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Logging
{
    public static class LoggingExtensions
    {
        public static void SessionCommitCanceled(this ILogger logger, OperationCanceledException ex)
        {
            logger.LogError(ex, ex.Message);
        }

        public static void ErrorClosingTheSession(this ILogger logger, Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }
}
