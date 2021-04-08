using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HB.Infrastructure.Redis.EventBus")]
namespace HB.FullStack.EventBus
{
    /// <summary>
    /// from 3000 - 3999
    /// </summary>
    internal static class EventBusErrorCodes
    {
        public static ErrorCode NoHandler { get; } = new ErrorCode(ErrorCodeStartIds.EVENT_BUS + 0, nameof(NoHandler), "");
        public static ErrorCode HandlerAlreadyExisted { get; } = new ErrorCode(ErrorCodeStartIds.EVENT_BUS + 1, nameof(HandlerAlreadyExisted), "");
        public static ErrorCode SettingsError { get; } = new ErrorCode(ErrorCodeStartIds.EVENT_BUS + 2, nameof(SettingsError), "");
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