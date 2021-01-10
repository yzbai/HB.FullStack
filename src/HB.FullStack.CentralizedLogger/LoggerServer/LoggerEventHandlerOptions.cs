using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.CentralizedLogger.LoggerServer
{
    //TODO: 现在还只能处理一个事件名称，之后要扩展到多个
    public class LoggerEventHandlerOptions : IOptions<LoggerEventHandlerOptions>
    {
        public LoggerEventHandlerOptions Value => this;

        public string LogEventName { get; set; }

        public string ServerName { get; set; }

        public string SubscribeGroup { get; set; }
    }
}
