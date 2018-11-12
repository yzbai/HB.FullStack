using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Logging
{
    public static class LogHelper
    {
        private static ILogger _logger;

        public static void SetGlobalLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static ILogger GlobalLogger
        {
            get
            {
                return _logger;
            }
        }
    }
}
