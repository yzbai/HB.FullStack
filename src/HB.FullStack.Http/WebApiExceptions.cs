﻿using System;
using System.Collections.Generic;

namespace HB.FullStack.WebApi
{
    public static class WebApiExceptions
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

        internal static Exception UploadError(string cause, Exception? innerEx, object? context)
        {
            return new WebApiException(ErrorCodes.UploadError, cause, innerEx, context);
        }
    }
}