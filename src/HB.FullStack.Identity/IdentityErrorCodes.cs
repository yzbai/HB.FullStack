using Microsoft.Extensions.Logging;

using System;

namespace HB.FullStack.Identity
{
    internal static class IdentityErrorCodes
    {
        public static ErrorCode AuthorizationNotFound { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 0, nameof(AuthorizationNotFound), "");
        public static ErrorCode AuthorizationPasswordWrong { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 1, nameof(AuthorizationPasswordWrong), "");
        public static ErrorCode AuthorizationTooFrequent { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 2, nameof(AuthorizationTooFrequent), "");
        public static ErrorCode AuthorizationInvalideAccessToken { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 3, nameof(AuthorizationInvalideAccessToken), "");
        public static ErrorCode AuthorizationInvalideDeviceId { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 4, nameof(AuthorizationInvalideDeviceId), "");
        public static ErrorCode AuthorizationInvalideUserId { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 5, nameof(AuthorizationInvalideUserId), "");
        public static ErrorCode AuthorizationUserSecurityStampChanged { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 6, nameof(AuthorizationUserSecurityStampChanged), "");
        public static ErrorCode AuthorizationRefreshTokenExpired { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 7, nameof(AuthorizationRefreshTokenExpired), "");
        public static ErrorCode AuthorizationNoTokenInStore { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 8, nameof(AuthorizationNoTokenInStore), "");
        public static ErrorCode AuthorizationMobileNotConfirmed { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 9, nameof(AuthorizationMobileNotConfirmed), "");
        public static ErrorCode AuthorizationEmailNotConfirmed { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 10, nameof(AuthorizationEmailNotConfirmed), "");
        public static ErrorCode AuthorizationLockedOut { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 11, nameof(AuthorizationLockedOut), "");
        public static ErrorCode AuthorizationOverMaxFailedCount { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 12, nameof(AuthorizationOverMaxFailedCount), "");
        public static ErrorCode JwtSigningCertNotFound { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 13, nameof(JwtSigningCertNotFound), "");
        public static ErrorCode JwtEncryptionCertNotFound { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 14, nameof(JwtEncryptionCertNotFound), "");
        public static ErrorCode FoundTooMuch { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 15, nameof(FoundTooMuch), "");
        public static ErrorCode NotFound { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 16, nameof(NotFound), "");
        public static ErrorCode IdentityNothingConfirmed { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 17, nameof(IdentityNothingConfirmed), "");
        public static ErrorCode IdentityMobileEmailLoginNameAllNull { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 18, nameof(IdentityMobileEmailLoginNameAllNull), "");
        public static ErrorCode IdentityAlreadyTaken { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 19, nameof(IdentityAlreadyTaken), "");
        public static ErrorCode ServiceRegisterError { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 20, nameof(ServiceRegisterError), "");
        public static ErrorCode TryRemoveRoleFromUserError { get; } = new ErrorCode(ErrorCodeStartIds.IDENTITY + 21, nameof(TryRemoveRoleFromUserError), "");
    }

    internal static class IdentityExceptions
    {
        internal static Exception AuthorizationNotFound(SignInContext signInContext)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationNotFound);
            exception.Data["Context"] = signInContext;

            return exception;
        }

        internal static Exception AuthorizationPasswordWrong(SignInContext signInContext)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationPasswordWrong);
            exception.Data["Context"] = signInContext;

            return exception;
        }

        internal static Exception AuthorizationTooFrequent(RefreshContext context)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationTooFrequent);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationInvalideAccessToken(RefreshContext context, Exception innerException)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationInvalideAccessToken, innerException);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationInvalideAccessToken(RefreshContext context)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationInvalideAccessToken);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationInvalideDeviceId(RefreshContext context)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationInvalideDeviceId);
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
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationInvalideUserId);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationNoTokenInStore(string cause)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationNoTokenInStore);
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationRefreshTokenExpired()
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationRefreshTokenExpired);

            return exception;
        }

        internal static Exception AuthorizationUserSecurityStampChanged(string cause)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationUserSecurityStampChanged);
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationMobileNotConfirmed(Guid userId)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationMobileNotConfirmed);
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AuthorizationEmailNotConfirmed(Guid userId)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationEmailNotConfirmed);
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AuthorizationLockedOut(DateTimeOffset? lockoutEndDate, Guid userId)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationLockedOut);
            exception.Data["UserId"] = userId;
            exception.Data["LockoutEndDate"] = lockoutEndDate;

            return exception;
        }

        internal static Exception AuthorizationOverMaxFailedCount(Guid userId)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AuthorizationOverMaxFailedCount);
            exception.Data["UserId"] = userId;

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