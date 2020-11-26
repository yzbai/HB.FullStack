using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.EventBus
{
    public class EventBusException : FrameworkException
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

        public EventBusException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public EventBusException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }

        public EventBusException(ErrorCode errorCode) : base(errorCode)
        {
        }
    }
}
