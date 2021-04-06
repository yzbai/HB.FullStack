namespace System
{
    public class EventCodeException : Exception
    {
        public EventCodeException(EventCode eventCode) : base(eventCode.Message)
        {
            EventCode = eventCode;
        }

        public EventCodeException(EventCode eventCode, Exception? innerException) : base(eventCode.Message, innerException)
        {
            EventCode = eventCode;
        }

        public EventCodeException(EventCode eventCode, string? message) : base(eventCode.Message + " - " + message)
        {
            EventCode = eventCode;
        }

        public EventCodeException(EventCode eventCode, string? message, Exception? innerException) : base(eventCode.Message + " - " + message, innerException)
        {
            EventCode = eventCode;
        }

        public EventCode EventCode
        {
            get
            {
                return (EventCode)Data[nameof(EventCode)];
            }
            private set
            {
                Data[nameof(EventCode)] = value;
            }
        }
    }
}