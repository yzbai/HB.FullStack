using System.Runtime.CompilerServices;

namespace System
{
    public static partial class CommonExceptions
    {
        public static Exception AddtionalPropertyNeeded(string model, string property, [CallerMemberName] string? caller = null)
        {
            Exception ex = new CommonException(ErrorCodes.ChangedPackError, nameof(AddtionalPropertyNeeded), null, null);

            ex.Data["Model"] = model;
            ex.Data["Property"] = property;
            ex.Data["Caller"] = caller;

            return ex;
        }

        public static Exception CertNotFound(string? subject, string? fullPath)
        {
            return new CommonException(ErrorCodes.CertNotFound, nameof(CertNotFound), null, new { Subject = subject, FullPath = fullPath });
        }

        internal static Exception EnvironmentVariableError(object? value, string cause)
        {
            return new CommonException(ErrorCodes.EnvironmentVariableError, cause, null, new { Value = value });
        }

        public static Exception UnkownEventSender(object? sender, [CallerMemberName] string? callerMethodName = null)
        {
            Exception ex = new CommonException(ErrorCodes.EventError, nameof(UnkownEventSender), null, null);

            ex.Data["SenderType"] = sender?.GetType().FullName;
            ex.Data["CallerMethodName"] = callerMethodName;

            return ex;
        }
    }
}
