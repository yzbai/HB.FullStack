using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common;

namespace HB.Infrastructure.Redis.EventBus
{
    public class EventMessageEntity
    {
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        public long Timestamp { get; set; } = TimeUtil.UtcNowUnixTimeSeconds;

        public string EventName { get; set; } = null!;

        public string JsonData { get; set; } = null!;

        public EventMessageEntity()
        {

        }
        public EventMessageEntity(string eventName, string jsonData)
        {
            EventName = eventName;
            JsonData = jsonData;
        }
    }

}
