using System;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("HB.Infrastructure.Redis.EventBus")]
namespace HB.FullStack.EventBus
{
    internal static class EventBusErrorCodes
    {
        public static ErrorCode NoHandler { get; set; } = new ErrorCode(3000, nameof(NoHandler), "");
        public static ErrorCode HandlerAlreadyExisted { get; set; } = new ErrorCode(3000, nameof(HandlerAlreadyExisted), "");
        public static ErrorCode SettingsError { get; set; } = new ErrorCode(3000, nameof(SettingsError), "");
    }

    internal static class Exceptions
    {
        internal static Exception SettingsError(string eventName, string cause)
        {
            EventBusException exception = new EventBusException(EventBusErrorCodes.SettingsError);

            exception.Data["EventName"] = eventName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception NoHandler(string eventType)
        {
            EventBusException exception = new EventBusException(EventBusErrorCodes.NoHandler);

            exception.Data["EventType"] = eventType;

            return exception;
        }

        internal static Exception HandlerAlreadyExisted(string eventType, string brokerName)
        {
            EventBusException exception = new EventBusException(EventBusErrorCodes.HandlerAlreadyExisted);

            exception.Data["EventType"] = eventType;
            exception.Data["BrokerName"] = brokerName;

            return exception;
        }
    }
}