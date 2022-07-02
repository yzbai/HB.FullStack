using Microsoft.Extensions.Logging;

using System;

namespace HB.FullStack.Identity
{
    internal static class IdentityErrorCodes
    {
      
        
        public static ErrorCode FoundTooMuch { get; } = new ErrorCode(nameof(FoundTooMuch), "");
        public static ErrorCode NotFound { get; } = new ErrorCode(nameof(NotFound), "");
        public static ErrorCode IdentityNothingConfirmed { get; } = new ErrorCode(nameof(IdentityNothingConfirmed), "");
        public static ErrorCode IdentityMobileEmailLoginNameAllNull { get; } = new ErrorCode(nameof(IdentityMobileEmailLoginNameAllNull), "");
        public static ErrorCode IdentityAlreadyTaken { get; } = new ErrorCode(nameof(IdentityAlreadyTaken), "");
        public static ErrorCode ServiceRegisterError { get; } = new ErrorCode(nameof(ServiceRegisterError), "");
        public static ErrorCode TryRemoveRoleFromUserError { get; } = new ErrorCode(nameof(TryRemoveRoleFromUserError), "");
        public static ErrorCode AudienceNotFound { get; } = new ErrorCode(nameof(AudienceNotFound), "");
    }

    internal static class IdentityExceptions
    {
        internal static Exception AuthorizationNotFound(SignInContext signInContext)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationNotFound);
            exception.Data["Context"] = signInContext;

            return exception;
        }

        internal static Exception AuthorizationPasswordWrong(SignInContext signInContext)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationPasswordWrong);
            exception.Data["Context"] = signInContext;

            return exception;
        }

        internal static Exception AuthorizationTooFrequent(RefreshContext context)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationTooFrequent);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationInvalideAccessToken(RefreshContext context, Exception innerException)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationInvalideAccessToken, innerException);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationInvalideAccessToken(RefreshContext context)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationInvalideAccessToken);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationInvalideDeviceId(RefreshContext context)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationInvalideDeviceId);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception FoundTooMuch(Guid userId, Guid roleId, string cause)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.FoundTooMuch);
            exception.Data["UserId"] = userId;
            exception.Data["RoleId"] = roleId;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationInvalideUserId(RefreshContext context)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationInvalideUserId);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationNoTokenInStore(string cause)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationNoTokenInStore);
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationRefreshTokenExpired()
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationRefreshTokenExpired);

            return exception;
        }

        internal static Exception AuthorizationUserSecurityStampChanged(string cause)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationUserSecurityStampChanged);
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationMobileNotConfirmed(Guid userId)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationMobileNotConfirmed);
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AuthorizationEmailNotConfirmed(Guid userId)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationEmailNotConfirmed);
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AuthorizationLockedOut(DateTimeOffset? lockoutEndDate, Guid userId)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationLockedOut);
            exception.Data["UserId"] = userId;
            exception.Data["LockoutEndDate"] = lockoutEndDate;

            return exception;
        }

        internal static Exception AuthorizationOverMaxFailedCount(Guid userId)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationOverMaxFailedCount);
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AudienceNotFound(SignInContext context)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AudienceNotFound);
            exception.Data["SignInContext"] = SerializeUtil.ToJson(context);

            return exception;
        }

        internal static Exception ServiceRegisterError(string cause)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.ServiceRegisterError);
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception NotFound(Guid userId, Guid roleId, string cause)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.NotFound);
            exception.Data["UserId"] = userId;
            exception.Data["RoleId"] = roleId;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception IdentityAlreadyTaken(string? mobile, string? email, string? loginName)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.IdentityAlreadyTaken);
            exception.Data["Mobile"] = mobile;
            exception.Data["Email"] = email;
            exception.Data["LoginName"] = loginName;

            return exception;
        }

        internal static Exception IdentityMobileEmailLoginNameAllNull()
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.IdentityMobileEmailLoginNameAllNull);

            return exception;
        }

        internal static Exception IdentityNothingConfirmed()
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.IdentityNothingConfirmed);

            return exception;
        }
    }

    public static class IdentityLoggerExtensions
    {
        private static readonly Action<ILogger, Guid, Guid, string?, Exception?> _logTryRemoveRoleFromUserError = LoggerMessage.Define<Guid, Guid, string?>(
            LogLevel.Error,
            IdentityErrorCodes.TryRemoveRoleFromUserError.ToEventId(),
            "TryRemoveRoleFromUserError. UserId={UserId}, RoleId={RoleId}, LastUser={LastUser}");

        public static void LogTryRemoveRoleFromUserError(this ILogger logger, Guid userId, Guid roleId, string? lastUser, Exception? innerEx)
        {
            _logTryRemoveRoleFromUserError(logger, userId, roleId, lastUser, innerEx);
        }
    }
}