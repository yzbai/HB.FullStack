using Microsoft.Extensions.Logging;

namespace System
{
    public static class ClientErrorCodes
    {
        public static ErrorCode ImageOptionsOutOfRange { get; } = new ErrorCode(nameof(ImageOptionsOutOfRange), "");
        public static ErrorCode IdBarrierError { get; } = new ErrorCode(nameof(IdBarrierError), "");
        public static ErrorCode ResourceNotFound { get; } = new ErrorCode(nameof(ResourceNotFound), "");
        public static ErrorCode BizError { get; } = new ErrorCode(nameof(BizError), "");
        public static ErrorCode NotLogined { get; } = new ErrorCode(nameof(NotLogined), "");
        public static ErrorCode AliyunStsTokenError { get; } = new ErrorCode(nameof(AliyunStsTokenError), "");
        public static ErrorCode FileServiceError { get; } = new ErrorCode(nameof(FileServiceError), "");
        public static ErrorCode AliyunOssPutObjectError { get; } = new ErrorCode(nameof(AliyunOssPutObjectError), "");
        public static ErrorCode LocalFileCopyError { get; } = new ErrorCode(nameof(LocalFileCopyError), "");
        public static ErrorCode LocalFileSaveError { get; } = new ErrorCode(nameof(LocalFileSaveError), "");
        public static ErrorCode AliyunStsTokenOverTime { get; } = new ErrorCode(nameof(AliyunStsTokenOverTime), "");
        public static ErrorCode SmsCodeValidateError { get; } = new ErrorCode(nameof(SmsCodeValidateError), "");
        public static ErrorCode NoInternet { get; } = new ErrorCode(nameof(NoInternet), "");
        public static ErrorCode UploadError { get; } = new ErrorCode(nameof(UploadError), "");

        public static ErrorCode DbSimpleLockerNoWaitLockFailed { get; } = new ErrorCode(nameof(DbSimpleLockerNoWaitLockFailed), "");
        public static ErrorCode UnSupportedEntityType { get; } = new ErrorCode(nameof(UnSupportedEntityType), "");
        public static ErrorCode OperationInvalidCauseofSyncingAfterReconnected { get; } = new ErrorCode(nameof(OperationInvalidCauseofSyncingAfterReconnected), "");
    }

    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, string?, string?, TimeSpan?, Exception?> _logDbSimpleLockerNoWaitLockFailed =
            LoggerMessage.Define<string?, string?, TimeSpan?>(
                LogLevel.Error,
                ClientErrorCodes.DbSimpleLockerNoWaitLockFailed.ToEventId(),
                "客户端的DbSimpleLocker不等待Lock失败.ResourceType = {ResourceType}, Resource={Resource}, AvailableTime={AvailableTime}");

        public static void LogDbSimpleLockerNoWaitLockFailed(this ILogger logger, string? resourceType, string? resource, TimeSpan? availableTime, Exception? exception)
        {
            _logDbSimpleLockerNoWaitLockFailed(logger, resourceType, resource, availableTime, exception);
        }


        private static readonly Action<ILogger, string?, string?, Exception?> _logDbSimpleLockerUnLockFailed =
            LoggerMessage.Define<string?, string?>(
                LogLevel.Error,
                ClientErrorCodes.DbSimpleLockerNoWaitLockFailed.ToEventId(),
                "客户端的DbSimpleLocker解锁失败.ResourceType = {ResourceType}, Resource={Resource}");

        public static void LogDbSimpleLockerUnLockFailed(this ILogger logger, string? resourceType, string? resource, Exception? exception)
        {
            _logDbSimpleLockerUnLockFailed(logger, resourceType, resource, exception);
        }
    }

    public static class ClientExceptions
    {
        public static Exception LocalFileSaveError(string fullPath)
        {
            ClientException ex = new ClientException(ClientErrorCodes.LocalFileSaveError);

            ex.Data["FullPath"] = fullPath;

            return ex;
        }

        public static Exception NoInternet(string cause)
        {
            ClientException ex = new ClientException(ClientErrorCodes.NoInternet);

            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception AliyunOssPutObjectError(string cause, Exception? innerException)
        {
            ClientException ex = new ClientException(ClientErrorCodes.AliyunOssPutObjectError, innerException);

            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception FileServiceError(string fileName, string directory, string cause, Exception innerException)
        {
            ClientException ex = new ClientException(ClientErrorCodes.FileServiceError, innerException);

            ex.Data["FileName"] = fileName;
            ex.Data["Directory"] = directory;
            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception ImageOptionsOutOfRange(int selectedIndex, string cause)
        {
            ClientException ex = new ClientException(ClientErrorCodes.ImageOptionsOutOfRange);

            ex.Data["SelectedIndex"] = selectedIndex;
            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception IdBarrierError(string cause)
        {
            ClientException ex = new ClientException(ClientErrorCodes.IdBarrierError);

            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception ResourceNotFound(string resourceId)
        {
            ClientException ex = new ClientException(ClientErrorCodes.ResourceNotFound);

            ex.Data["ResourceId"] = resourceId;

            return ex;
        }

        public static Exception AliyunStsTokenOverTime(string casuse, string requestDirectory, bool needWrite)
        {
            ClientException ex = new ClientException(ClientErrorCodes.AliyunStsTokenOverTime);

            ex.Data["Cause"] = casuse;
            ex.Data["RequestDirectory"] = requestDirectory;
            ex.Data["NeedWrite"] = needWrite.ToString();

            return ex;
        }

        public static Exception NotLogined()
        {
            ClientException ex = new ClientException(ClientErrorCodes.NotLogined);

            return ex;
        }

        public static Exception SmsCodeValidateError(string mobile)
        {
            ClientException ex = new ClientException(ClientErrorCodes.SmsCodeValidateError);

            ex.Data["Mobile"] = mobile;

            return ex;
        }

        public static Exception UploadError(string cause)
        {
            ClientException ex = new ClientException(ClientErrorCodes.UploadError);

            ex.Data["Cause"] = cause;

            return ex;
        }

        internal static Exception UnSupportedEntityType(string? entityFullName)
        {
            ClientException ex = new ClientException(ClientErrorCodes.UnSupportedEntityType);

            ex.Data["FullName"] = entityFullName;

            return ex;
        }

        internal static Exception OperationInvalidCauseofSyncingAfterReconnected()
        {
            ClientException ex = new ClientException(ClientErrorCodes.OperationInvalidCauseofSyncingAfterReconnected);

            return ex;
        }
    }
}