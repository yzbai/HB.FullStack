﻿using System.Collections.Generic;
using System.Linq;

using HB.FullStack.Common.Api;

namespace System
{
    public static partial class WebApiExceptions
    {
        public static Exception DatabaseInitLockError(IEnumerable<string> databases)
        {
            WebApiException exception = new WebApiException(ErrorCodes.DatabaseInitLockError, nameof(DatabaseInitLockError));

            exception.Data["Databases"] = databases;

            return exception;
        }

        public static Exception StartupError(object? value, string cause)
        {
            WebApiException exception = new WebApiException(ErrorCodes.StartupError, nameof(StartupError));

            exception.Data["Value"] = value;
            exception.Data["Cause"] = cause;

            return exception;
        }

        public static Exception UploadError(string cause, Exception? innerEx, object? context)
        {
            return new WebApiException(ErrorCodes.UploadError, cause, innerEx, context);
        }

        public static Exception ChangedPropertyPackError(string cause, ChangedPackDto? changedPropertyPack, string? modelFullName)
        {
            DatabaseException ex = new DatabaseException(ErrorCodes.ChangedPackError, cause, null, null);

            ex.Data["ModelFullName"] = modelFullName;
            ex.Data["PropertyNames"] = changedPropertyPack?.ChangedProperties.Select(c => c.PropertyName).ToJoinedString(",");

            return ex;
        }
    }
}