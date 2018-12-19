﻿using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Common;

namespace HB.Infrastructure.RabbitMQ
{
    public class EventMessageEntity
    {
        public string Id { get; set; } = SecurityHelper.CreateUniqueToken();

        public long Timestamp { get; set; } = DataConverter.CurrentTimestampSeconds();

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
