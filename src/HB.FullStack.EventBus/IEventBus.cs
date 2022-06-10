﻿
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace HB.FullStack.EventBus.Abstractions
{
    /// <summary>
    /// 以EventType为中心的时间总线
    /// 需要保证事件的不丢失，每一个event都有追溯，都有因果
    /// 
    /// 同一个事件，只支持被处理一次，即同一个type的事件，只有一个handler对应
    /// 
    /// EventType 全局唯一
    /// 
    /// One EventType, One Queue, One Handler
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <returns>是否发布成功，只有返回true才能确保消息不丢失</returns>
        
        Task PublishAsync(string eventName, string jsonData);

        
        void Subscribe(string eventName, IEventHandler handler);

        
        Task UnSubscribeAsync(string eventName);

        
        void StartHandle(string eventName);

        void Close();
    }
}
