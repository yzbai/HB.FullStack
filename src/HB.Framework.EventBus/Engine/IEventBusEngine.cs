using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.EventBus
{
    public interface IEventBusEngine : IDisposable
    {
        Task<PublishResult> PublishString(string serverName, string eventName, string data);

        Task SubscribeAndConsume(string serverName, string subscriberGroup, string eventName, IEventHandler handler);
    }
}
