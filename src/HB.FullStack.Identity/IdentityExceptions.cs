using System;
using System.Collections.Generic;

using HB.FullStack.Server.Identity.Models;

namespace HB.FullStack.Server.Identity
{
    internal static class IdentityExceptions
    {
        internal static Exception IdentityUserNotExists(SignInContext signInContext)
        {
            IdentityException exception = new IdentityException(ErrorCodes.IdentityUserNotExists, nameof(IdentityUserNotExists));
            exception.Data["Context"] = signInContext;

            return exception;
        }

        public static Exception IdentityInvalidSmsCode(IBySmsCode context)
        {
            IdentityException exception = new IdentityException(ErrorCodes.InvalidSmsCode, nameof(IdentityInvalidSmsCode));
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationPasswordWrong(SignInContext signInContext)
        {
            IdentityException exception = new IdentityException(ErrorCodes.AuthorizationPasswordWrong, nameof(AuthorizationPasswordWrong));
            exception.Data["Context"] = signInContext;

            return exception;
        }

        internal static Exception SignInReceiptRefreshConcurrentError(RefreshContext context)
        {
            IdentityException exception = new IdentityException(ErrorCodes.SignInReceiptRefreshConcurrentError, nameof(SignInReceiptRefreshConcurrentError));
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception RefreshSignInReceiptError(string cause, Exception? innerException, object? context)
        {
            IdentityException exception = new IdentityException(ErrorCodes.RefreshAccessTokenError, cause, innerException);
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationInvalideClientId(RefreshContext context)
        {
            IdentityException exception = new IdentityException(ErrorCodes.AuthorizationInvalideClientId, nameof(AuthorizationInvalideClientId));
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception FoundTooMuch(Guid userId, Guid roleId, string cause)
        {
            IdentityException exception = new IdentityException(ErrorCodes.FoundTooMuch, nameof(FoundTooMuch));
            exception.Data["UserId"] = userId;
            exception.Data["RoleId"] = roleId;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationInvalideUserId(RefreshContext context)
        {
            IdentityException exception = new IdentityException(ErrorCodes.AuthorizationInvalideUserId, nameof(AuthorizationInvalideUserId));
            exception.Data["Context"] = context;

            return exception;
        }

        internal static Exception AuthorizationNoTokenInStore(string cause)
        {
            IdentityException exception = new IdentityException(ErrorCodes.AuthorizationNoTokenInStore, nameof(AuthorizationNoTokenInStore));
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationRefreshTokenExpired()
        {
            IdentityException exception = new IdentityException(ErrorCodes.AuthorizationRefreshTokenExpired, nameof(AuthorizationRefreshTokenExpired));

            return exception;
        }

        internal static Exception AuthorizationUserSecurityStampChanged(string cause)
        {
            IdentityException exception = new IdentityException(ErrorCodes.AuthorizationUserSecurityStampChanged, nameof(AuthorizationUserSecurityStampChanged));
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception AuthorizationMobileNotConfirmed(Guid userId)
        {
            IdentityException exception = new IdentityException(ErrorCodes.AuthorizationMobileNotConfirmed, nameof(AuthorizationMobileNotConfirmed));
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AuthorizationEmailNotConfirmed(Guid userId)
        {
            IdentityException exception = new IdentityException(ErrorCodes.AuthorizationEmailNotConfirmed, nameof(AuthorizationEmailNotConfirmed));
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AuthorizationLockedOut(DateTimeOffset? lockoutEndDate, Guid userId)
        {
            IdentityException exception = new IdentityException(ErrorCodes.AuthorizationLockedOut, nameof(AuthorizationLockedOut));
            exception.Data["UserId"] = userId;
            exception.Data["LockoutEndDate"] = lockoutEndDate;

            return exception;
        }

        internal static Exception AuthorizationOverMaxFailedCount(Guid userId)
        {
            IdentityException exception = new IdentityException(ErrorCodes.AuthorizationOverMaxFailedCount, nameof(AuthorizationOverMaxFailedCount));
            exception.Data["UserId"] = userId;

            return exception;
        }

        internal static Exception AudienceNotFound(object context)
        {
            IdentityException exception = new IdentityException(ErrorCodes.AudienceNotFound, nameof(AudienceNotFound));
            exception.Data["Context"] = SerializeUtil.ToJson(context);

            return exception;
        }

        internal static Exception ServiceRegisterError(string cause)
        {
            IdentityException exception = new IdentityException(ErrorCodes.ServiceRegisterError, nameof(ServiceRegisterError));
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception NotFound(Guid userId, Guid roleId, string cause)
        {
            IdentityException exception = new IdentityException(ErrorCodes.NotFound, nameof(NotFound));
            exception.Data["UserId"] = userId;
            exception.Data["RoleId"] = roleId;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception IdentityAlreadyTaken(string? mobile, string? email, string? loginName)
        {
            IdentityException exception = new IdentityException(ErrorCodes.IdentityAlreadyTaken, nameof(IdentityAlreadyTaken));
            exception.Data["Mobile"] = mobile;
            exception.Data["Email"] = email;
            exception.Data["LoginName"] = loginName;

            return exception;
        }

        internal static Exception IdentityMobileEmailLoginNameAllNull()
        {
            IdentityException exception = new IdentityException(ErrorCodes.IdentityMobileEmailLoginNameAllNull, nameof(IdentityMobileEmailLoginNameAllNull));

            return exception;
        }

        internal static Exception IdentityNothingConfirmed()
        {
            IdentityException exception = new IdentityException(ErrorCodes.IdentityNothingConfirmed, nameof(IdentityNothingConfirmed));

            return exception;
        }

        internal static Exception AlreadyHaveRoles(Guid userId, IEnumerable<Role> roles, string lastUser)
        {
            IdentityException ex = new IdentityException(ErrorCodes.AlreadyHaveRoles, nameof(AlreadyHaveRoles));

            ex.Data["UserId"] = userId.ToString();
            ex.Data["Roles"] = SerializeUtil.ToJson(roles);
            ex.Data["LastUser"] = lastUser;

            return ex;
        }

        internal static Exception DisallowRegisterByLoginName()
        {
            IdentityException ex = new IdentityException(ErrorCodes.IdentityDisallowRegisterByLoginName, nameof(DisallowRegisterByLoginName));

            return ex;
        }
    }
}