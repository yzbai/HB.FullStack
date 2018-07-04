using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.EventBus
{
    public interface IEventBusEngine : IDisposable
    {
        /// <summary>
        /// 向总线发布消息
        /// </summary>
        /// <param name="serverName">总线服务器名称</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        Task<PublishResult> PublishString(string serverName, string eventName, string data);

        /// <summary>
        /// 订阅并消费，使用IEventHandler处理
        /// </summary>
        /// <param name="serverName">发向哪个总线</param>
        /// <param name="subscriberGroup">是哪个订阅组</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="handler">事件处理者</param>
        /// <returns></returns>
        Task SubscribeAndConsume(string serverName, string subscriberGroup, string eventName, IEventHandler handler);
    }
}
