using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.EventBus.Abstractions;

namespace HB.Framework.EventBus
{
    public interface IEventBusEngine
    {
        EventBusSettings EventBusSettings { get; }

        /// <summary>
        /// Engine负责确保，将Event发布
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <exception cref="EventBusException"></exception>
        Task PublishAsync(string brokerName, EventMessage eventMessage);

        /// <exception cref="EventBusException"></exception>
        void SubscribeHandler(string brokerName, string eventType, IEventHandler eventHandler);

        /// <exception cref="EventBusException"></exception>
        void UnSubscribeHandler(string eventyType);
        
        /// <exception cref="EventBusException"></exception>
        void StartHandle(string eventType);
        
        void Close();
    }
}
