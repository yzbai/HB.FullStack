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
        Task PublishAsync(string brokerName, string eventName, string jsonData);

        /// <exception cref="EventBusException"></exception>
        void SubscribeHandler(string brokerName, string eventName, IEventHandler eventHandler);

        /// <exception cref="EventBusException"></exception>
        void UnSubscribeHandler(string eventyName);

        /// <exception cref="EventBusException"></exception>
        void StartHandle(string eventName);

        void Close();
    }
}
