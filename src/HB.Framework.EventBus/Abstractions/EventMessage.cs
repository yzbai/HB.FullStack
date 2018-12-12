using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using HB.Framework.Common.Entity;

namespace HB.Framework.EventBus.Abstractions
{
    public class EventMessage
    {

        public EventMessage(string type, byte[] body)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            Type = type;

            Body = new byte[body.Length];

            body.CopyTo(Body, 0);
        }

        public string Type { get; set; }

        public byte[] Body { get; set; }

        public static bool IsValid(EventMessage msg)
        {
            if (msg == null)
            {
                return false;
            }

            return !(string.IsNullOrEmpty(msg.Type) || msg.Body == null);
        }

        public static bool IsValid(object data)
        {
            if (data == null)
            {
                return false;
            }

            EventMessage ev = (EventMessage)data;

            if (ev == null)
            {
                return false;
            }

            return IsValid(ev);
        }
    }
}
