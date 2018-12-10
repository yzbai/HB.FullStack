using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Framework.EventBus.Abstractions
{
    /// <summary>
    /// 以Topic为中心的时间总线
    /// 需要保证事件的不丢失，每一个event都有追溯，都有因果
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <returns>是否发布成功，只有返回true才能确保消息不丢失</returns>
        Task<bool> PublishAsync(EventMessage eventMessage);
    }
}
