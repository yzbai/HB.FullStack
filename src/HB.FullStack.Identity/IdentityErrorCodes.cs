﻿using System;
using System.Collections.Generic;

using HB.FullStack.Identity.Models;

using Microsoft.Extensions.Logging;

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
        public static ErrorCode AlreadyHaveRoles { get; } = new ErrorCode(nameof(AlreadyHaveRoles), "用户已经有了一些你要添加的Role");
    }

    internal static class IdentityExceptions
    {
        internal static Exception AuthorizationNotFound(SignInContext signInContext)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationNotFound, nameof(AuthorizationNotFound));
            exception.Data["Context"] = signInContext;

            return exception;
        }

        internal static Exception AuthorizationPasswordWrong(SignInContext signInContext)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationPasswordWrong, nameof(AuthorizationPasswordWrong));
            exception.Data["Context"] = signInContext;

            return exception;
        }

        internal static Exception AccessTokenRefreshing(RefreshContext context)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AccessTokenRefreshing, nameof(AccessTokenRefreshing));
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception RefreshAccessTokenError(string cause, Exception? innerException, object? context)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.RefreshAccessTokenError, cause, innerException);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationInvalideDeviceId(RefreshContext context)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationInvalideDeviceId, nameof(AuthorizationInvalideDeviceId));
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception FoundTooMuch(Guid userId, Guid roleId, string cause)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.FoundTooMuch, nameof(FoundTooMuch));
            exception.Data["UserId"] = userId;
            exception.Data["RoleId"] = roleId;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationInvalideUserId(RefreshContext context)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationInvalideUserId, nameof(AuthorizationInvalideUserId));
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationNoTokenInStore(string cause)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationNoTokenInStore, nameof(AuthorizationNoTokenInStore));
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationRefreshTokenExpired()
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationRefreshTokenExpired, nameof(AuthorizationRefreshTokenExpired));

            return exception;
        }

        internal static Exception AuthorizationUserSecurityStampChanged(string cause)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationUserSecurityStampChanged, nameof(AuthorizationUserSecurityStampChanged));
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationMobileNotConfirmed(Guid userId)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationMobileNotConfirmed, nameof(AuthorizationMobileNotConfirmed));
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AuthorizationEmailNotConfirmed(Guid userId)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationEmailNotConfirmed, nameof(AuthorizationEmailNotConfirmed));
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AuthorizationLockedOut(DateTimeOffset? lockoutEndDate, Guid userId)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationLockedOut, nameof(AuthorizationLockedOut));
            exception.Data["UserId"] = userId;
            exception.Data["LockoutEndDate"] = lockoutEndDate;

            return exception;
        }

        internal static Exception AuthorizationOverMaxFailedCount(Guid userId)
        {
            IdentityException exception = new IdentityException(ApiErrorCodes.AuthorizationOverMaxFailedCount, nameof(AuthorizationOverMaxFailedCount));
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AudienceNotFound(SignInContext context)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.AudienceNotFound, nameof(AudienceNotFound));
            exception.Data["SignInContext"] = SerializeUtil.ToJson(context);

            return exception;
        }

        internal static Exception ServiceRegisterError(string cause)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.ServiceRegisterError, nameof(ServiceRegisterError));
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception NotFound(Guid userId, Guid roleId, string cause)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.NotFound, nameof(NotFound));
            exception.Data["UserId"] = userId;
            exception.Data["RoleId"] = roleId;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception IdentityAlreadyTaken(string? mobile, string? email, string? loginName)
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.IdentityAlreadyTaken, nameof(IdentityAlreadyTaken));
            exception.Data["Mobile"] = mobile;
            exception.Data["Email"] = email;
            exception.Data["LoginName"] = loginName;

            return exception;
        }

        internal static Exception IdentityMobileEmailLoginNameAllNull()
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.IdentityMobileEmailLoginNameAllNull, nameof(IdentityMobileEmailLoginNameAllNull));

            return exception;
        }

        internal static Exception IdentityNothingConfirmed()
        {
            IdentityException exception = new IdentityException(IdentityErrorCodes.IdentityNothingConfirmed, nameof(IdentityNothingConfirmed));

            return exception;
        }

        internal static Exception AlreadyHaveRoles(Guid userId, IEnumerable<Role> roles, string lastUser)
        {
            IdentityException ex = new IdentityException(IdentityErrorCodes.AlreadyHaveRoles, nameof(AlreadyHaveRoles));

            ex.Data["UserId"] = userId.ToString();
            ex.Data["Roles"] = SerializeUtil.ToJson(roles);
            ex.Data["LastUser"] = lastUser;

            return ex;
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