using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Common;

namespace HB.Infrastructure.Redis.EventBus
{
    public class EventMessageEntity
    {
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        public long Timestamp { get; set; } = TimeUtil.CurrentTimestampSeconds();

        public string EventName { get; set; }

        public string JsonData { get; set; }

        public EventMessageEntity(string eventName, string jsonData)
        {
            EventName = eventName;
            JsonData = jsonData;
        }

    }
        
}
