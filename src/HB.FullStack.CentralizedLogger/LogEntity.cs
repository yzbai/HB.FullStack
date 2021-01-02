using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.CentralizedLogger
{
    public class LogEntity
    {
        [Nest.Date]
        public DateTime DateTime { get; set; }

        public string HostName { get; set; }

        public string LoggerName { get; set; }

        public string LogLevel { get; set; }

        public EventId EventId { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }
    }
}
