using System.Collections.Generic;
using System.Linq;

using HB.FullStack.Common.Api;

namespace System
{
    public static partial class WebApiExceptions
    {
        public static Exception DatabaseInitLockError(string dbSchema)
        {
            WebApiException exception = new WebApiException(ErrorCodes.DatabaseInitLockError, nameof(DatabaseInitLockError));

            exception.Data["DbSchema"] = dbSchema;

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

        internal static Exception ShouldSetGlobalWebApplicationAccessorAtBegining()
        {
            WebApiException ex = new WebApiException(ErrorCodes.InnerError, nameof(ShouldSetGlobalWebApplicationAccessorAtBegining), null, null);

            return ex;
        }
    }
}