using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.EventBus.Abstractions;

namespace HB.FullStack.EventBus
{
    public interface IEventBusEngine
    {
        EventBusSettings EventBusSettings { get; }

        /// <summary>
        /// Engine负责确保，将Event发布
        /// </summary>
        /// <param name="eventMessage"></param>
        
        Task PublishAsync(string brokerName, string eventName, string jsonData);

        
        void SubscribeHandler(string brokerName, string eventName, IEventHandler eventHandler);


        
        Task UnSubscribeHandlerAsync(string eventyName);

        
        void StartHandle(string eventName);

        void Close();
    }
}
