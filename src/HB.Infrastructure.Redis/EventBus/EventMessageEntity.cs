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

        public string Type { get; set; }

        public string JsonData { get; set; }

        public EventMessageEntity() { }

        public EventMessageEntity(string type, string jsonData)
        {
            Type = type;
            JsonData = jsonData;
        }

    }
        
}
