using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Framework.EventBus
{
    public interface IEventBus
    {
        /// <summary>
        /// you can fire away to publish
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        Task<PublishResult> Publish(string eventName, string jsonString);

        /// <summary>
        /// Start to handle events，需要提前把eventhandler放到DI中
        /// </summary>
        void Handle();

        /// <summary>
        /// 动态注册事件，也可以在appsetting中设置
        /// </summary>
        /// <param name="eventConfig"></param>
        void RegisterEvent(EventConfig eventConfig);
    }
}
