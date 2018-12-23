using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;

namespace HB.Infrastructure.Redis.EventBus
{
    public class RedisEventBusEngine : IEventBusEngine
    {
        public Task<bool> PublishAsync(string brokerName, EventMessage eventMessage)
        {
            throw new NotImplementedException();
        }

        public bool SubscribeHandler(string brokerName, string eventType, IEventHandler eventHandler)
        {
            throw new NotImplementedException();
        }

        public bool UnSubscribeHandler(string eventyType, string handlerId)
        {
            throw new NotImplementedException();
        }
    }
}
