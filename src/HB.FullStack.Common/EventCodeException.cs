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