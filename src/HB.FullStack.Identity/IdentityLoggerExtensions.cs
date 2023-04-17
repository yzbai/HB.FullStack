using System;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.Identity
{
    public static class IdentityLoggerExtensions
    {
        private static readonly Action<ILogger, Guid, Guid, string?, Exception?> _logTryRemoveRoleFromUserError = LoggerMessage.Define<Guid, Guid, string?>(
            LogLevel.Error,
            ErrorCodes.TryRemoveRoleFromUserError.ToEventId(),
            "TryRemoveRoleFromUserError. UserId={UserId}, RoleId={RoleId}, LastUser={LastUser}");

        public static void LogTryRemoveRoleFromUserError(this ILogger logger, Guid userId, Guid roleId, string? lastUser, Exception? innerEx)
        {
            _logTryRemoveRoleFromUserError(logger, userId, roleId, lastUser, innerEx);
        }
    }
}