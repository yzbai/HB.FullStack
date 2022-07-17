namespace System
{
    public class EventBusException : ErrorCode2Exception
    {

        [Obsolete("DoNotUse")]
        public EventBusException()
        {
        }

        [Obsolete("DoNotUse")]
        public EventBusException(string message) : base(message)
        {
        }

        [Obsolete("DoNotUse")]
        public EventBusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EventBusException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(errorCode, cause, innerException, context)
        {
        }
    }
}
