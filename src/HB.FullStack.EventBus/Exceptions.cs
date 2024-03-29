﻿using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HB.Infrastructure.Redis.EventBus")]
namespace HB.FullStack.EventBus
{
    /// <summary>
    /// from 3000 - 3999
    /// </summary>

    internal static class Exceptions
    {
        internal static Exception SettingsError(string eventName, string cause)
        {
            EventBusException exception = new EventBusException(ErrorCodes.SettingsError, cause);

            exception.Data["EventName"] = eventName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception NoHandler(string eventType)
        {
            EventBusException exception = new EventBusException(ErrorCodes.NoHandler, nameof(NoHandler));

            exception.Data["EventType"] = eventType;

            return exception;
        }

        internal static Exception HandlerAlreadyExisted(string eventType, string brokerName)
        {
            EventBusException exception = new EventBusException(ErrorCodes.HandlerAlreadyExisted, nameof(HandlerAlreadyExisted));

            exception.Data["EventType"] = eventType;
            exception.Data["BrokerName"] = brokerName;

            return exception;
        }
    }
}