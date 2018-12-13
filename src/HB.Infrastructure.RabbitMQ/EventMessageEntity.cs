using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Common;

namespace HB.Infrastructure.RabbitMQ
{
    public class EventMessageEntity
    {
        public string Id { get; set; } = SecurityHelper.CreateUniqueToken();

        public double Timestamp { get; set; } = DataConverter.ToTimestamp(DateTimeOffset.UtcNow);

        public string Type { get; set; }

        public byte[] Body { get; set; }

        public EventMessageEntity(string type, byte[] body)
        {
            Type = type;
            Body = body;
        }

        public static bool IsValid(EventMessageEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            return !(string.IsNullOrEmpty(entity.Id) || string.IsNullOrEmpty(entity.Type) || entity.Body == null);
        }

        public static bool IsValid(object data)
        {
            if (data == null)
            {
                return false;
            }

            EventMessageEntity ev = (EventMessageEntity)data;

            if (ev == null)
            {
                return false;
            }

            return IsValid(ev);
        }
    }
}
