using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.EventBus.Abstractions
{
    /// <summary>
    /// 从哪个事件总线服务器上，读取哪个事件，自己是什么组别
    /// </summary>
    public class EventHandlerConfig
    {
        /// <summary>
        /// 事件所属的服务器，就跟Entity所属那个database一样
        /// </summary>
        public string ServerName { get; set; }

        public string EventName { get; set; }

        /// <summary>
        /// 处理者所属的组别，如果为空，则用EventBus设置的默认组别
        /// </summary>
        public string SubscribeGroup { get; set; }
    }
}
