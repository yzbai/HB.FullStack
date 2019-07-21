﻿using System;
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
        Task<bool> PublishAsync(string brokerName, EventMessage eventMessage);
        bool SubscribeHandler(string brokerName, string eventType, IEventHandler eventHandler);
        void UnSubscribeHandler(string eventyType);
        void StartHandle(string eventType);
        void Close();
    }
}
