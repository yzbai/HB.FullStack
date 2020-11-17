using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.CentralizedLogger
{
    public class LoggerOptions : IOptions<LoggerOptions>
    {
        public LoggerOptions Value => this;

        /// <summary>
        /// 关于哪一个站点或应用的日志
        /// </summary>
        public string HostName { get; set; } = "";

        public string LogEventName { get; set; }

        public string ServerName { get; set; }

        public string LogLevel { get; set; }

        public bool IncludeScopes { get; set; }
    }
}
