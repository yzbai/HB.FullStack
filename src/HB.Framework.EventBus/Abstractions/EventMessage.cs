using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using HB.Framework.Common.Entity;

namespace HB.Framework.EventBus.Abstractions
{
    public class EventMessage
    {

        public EventMessage(string topic, byte[] body)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentNullException(nameof(topic));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            Topic = topic;

            Body = new byte[body.Length];

            body.CopyTo(Body, 0);
        }

        public string Topic { get; set; }

        public byte[] Body { get; set; }

        public static bool IsValid(EventMessage msg)
        {
            return !(string.IsNullOrEmpty(msg.Topic) || msg.Body == null);
        }

    }
}
