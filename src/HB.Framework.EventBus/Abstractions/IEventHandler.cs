using HB.Framework.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.EventBus
{
    /// <summary>
    /// 单例启动
    /// </summary>
    public interface IEventHandler : IDisposable
    {
        EventHandlerConfig GetConfig();

        void Handle(string jsonString);
    }
}
