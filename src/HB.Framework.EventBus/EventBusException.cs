using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.EventBus
{
    public class EventBusException : ServerException
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

        public EventBusException(ServerErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public EventBusException(ServerErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }

        public EventBusException(ServerErrorCode errorCode) : base(errorCode)
        {
        }
    }
}
