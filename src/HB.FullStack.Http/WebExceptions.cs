using System.Collections.Generic;
using System.Linq;

using HB.FullStack.Common.Api;
using HB.FullStack.Common.PropertyTrackable;

namespace System
{
    public static partial class WebExceptions
    {
        

        public static Exception StartupError(object? value, string cause)
        {
            WebException exception = new WebException(ErrorCodes.StartupError, nameof(StartupError));

            exception.Data["Value"] = value;
            exception.Data["Cause"] = cause;

            return exception;
        }

        public static Exception UploadError(string cause, Exception? innerEx, object? context)
        {
            return new WebException(ErrorCodes.UploadError, cause, innerEx, context);
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
            WebException ex = new WebException(ErrorCodes.InnerError, nameof(ShouldSetGlobalWebApplicationAccessorAtBegining), null, null);

            return ex;
        }
    }
}