using System.Collections.Generic;
using System.Linq;

using HB.FullStack.Common.Api;
using HB.FullStack.Common.PropertyTrackable;

namespace System
{
    public static partial class WebApiExceptions
    {
        public static Exception DatabaseInitLockError(string dbSchemaName)
        {
            WebApiException exception = new WebApiException(ErrorCodes.DatabaseInitLockError, nameof(DatabaseInitLockError));

            exception.Data["DbSchemaName"] = dbSchemaName;

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

        public static Exception ChangedPropertyPackError(string cause, PropertyChangePack? changePack, string? modelFullName)
        {
            DbException ex = new DbException(ErrorCodes.ChangedPackError, cause, null, null);

            ex.Data["ModelFullName"] = modelFullName;
            ex.Data["PropertyNames"] = changePack?.PropertyChanges.Select(c => c.PropertyName).ToJoinedString(",");

            return ex;
        }

        internal static Exception ShouldSetGlobalWebApplicationAccessorAtBegining()
        {
            WebApiException ex = new WebApiException(ErrorCodes.InnerError, nameof(ShouldSetGlobalWebApplicationAccessorAtBegining), null, null);

            return ex;
        }
    }
}