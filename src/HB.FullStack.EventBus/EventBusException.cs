using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.EventBus;

namespace System
{
    public class EventBusException : Exception
    {
        public EventBusErrorCode ErrorCode { get; set; }
        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public EventBusException(EventBusErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public EventBusException(EventBusErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public EventBusException(EventBusErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
