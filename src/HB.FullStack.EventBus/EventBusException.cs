using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.EventBus;

namespace System
{
    public class EventBusException : ErrorCode2Exception
    {
        public EventBusException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public EventBusException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}
