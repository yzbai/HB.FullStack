using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.CentralizedLogger
{
    public class CentralizedLoggerOptions : IOptions<CentralizedLoggerOptions>
    {
        public CentralizedLoggerOptions Value => this;

        public string HostName { get; set; } = "";

        public string LogEventName { get; set; }

        public string MessageQueueServerName { get; set; }

        public string LogLevel { get; set; }

        public bool IncludeScopes { get; set; }
    }
}
