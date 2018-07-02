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
        /// Start to handle events
        /// </summary>
        void Handle();

        void AddEventConfig(EventConfig eventConfig);
    }
}
