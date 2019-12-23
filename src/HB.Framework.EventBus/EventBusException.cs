using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.EventBus
{
    public class EventBusException : Exception
    {
        public EventBusException(string message) : base(message)
        {
        }

        public EventBusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EventBusException()
        {
        }
    }
}
