using HB.FullStack.Client;

using Microsoft.Extensions.Logging;

namespace System
{
    public static class ClientErrorCodes
    {
        public static ErrorCode NoSuchDirectory   = new ErrorCode(nameof(NoSuchDirectory), "");

        public static ErrorCode ImageOptionsOutOfRange  = new ErrorCode(nameof(ImageOptionsOutOfRange), "");
        public static ErrorCode IdBarrierError  = new ErrorCode(nameof(IdBarrierError), "");
        public static ErrorCode ResourceNotFound  = new ErrorCode(nameof(ResourceNotFound), "");
        public static ErrorCode BizError  = new ErrorCode(nameof(BizError), "");
        public static ErrorCode NotLogined  = new ErrorCode(nameof(NotLogined), "");
        public static ErrorCode AliyunStsTokenError  = new ErrorCode(nameof(AliyunStsTokenError), "");
        public static ErrorCode FileServiceError  = new ErrorCode(nameof(FileServiceError), "");
        public static ErrorCode AliyunOssPutObjectError  = new ErrorCode(nameof(AliyunOssPutObjectError), "");
        public static ErrorCode LocalFileCopyError  = new ErrorCode(nameof(LocalFileCopyError), "");
        public static ErrorCode LocalFileSaveError  = new ErrorCode(nameof(LocalFileSaveError), "");
        public static ErrorCode AliyunStsTokenOverTime  = new ErrorCode(nameof(AliyunStsTokenOverTime), "");
        public static ErrorCode SmsCodeValidateError  = new ErrorCode(nameof(SmsCodeValidateError), "");
        public static ErrorCode NoInternet  = new ErrorCode(nameof(NoInternet), "");
        public static ErrorCode UploadError  = new ErrorCode(nameof(UploadError), "");

        public static ErrorCode DbSimpleLockerNoWaitLockFailed  = new ErrorCode(nameof(DbSimpleLockerNoWaitLockFailed), "");
        public static ErrorCode UnSupportedModelType  = new ErrorCode(nameof(UnSupportedModelType), "");
        public static ErrorCode OperationInvalidCauseofSyncingAfterReconnected  = new ErrorCode(nameof(OperationInvalidCauseofSyncingAfterReconnected), "");
        public static ErrorCode SyncError  = new ErrorCode(nameof(SyncError), "");
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
            ClientException ex = new ClientException(ClientErrorCodes.LocalFileSaveError, nameof(LocalFileSaveError));

            ex.Data["FullPath"] = fullPath;

            return ex;
        }

        public static Exception NoInternet()
        {
            ClientException ex = new ClientException(ClientErrorCodes.NoInternet, nameof(NoInternet));


            return ex;
        }

        public static Exception AliyunOssPutObjectError(string cause, Exception? innerException)
        {
            ClientException ex = new ClientException(ClientErrorCodes.AliyunOssPutObjectError, nameof(AliyunOssPutObjectError), innerException);

            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception FileServiceError(string fileName, string directoryName, string cause, Exception innerException)
        {
            ClientException ex = new ClientException(ClientErrorCodes.FileServiceError, nameof(FileServiceError), innerException);

            ex.Data["FileName"] = fileName;
            ex.Data["DirectoryName"] = directoryName;
            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception ImageOptionsOutOfRange(int selectedIndex, string cause)
        {
            ClientException ex = new ClientException(ClientErrorCodes.ImageOptionsOutOfRange, nameof(ImageOptionsOutOfRange));

            ex.Data["SelectedIndex"] = selectedIndex;
            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception IdBarrierError(string cause)
        {
            ClientException ex = new ClientException(ClientErrorCodes.IdBarrierError, nameof(IdBarrierError));

            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception ResourceNotFound(string resourceId)
        {
            ClientException ex = new ClientException(ClientErrorCodes.ResourceNotFound, nameof(ResourceNotFound));

            ex.Data["ResourceId"] = resourceId;

            return ex;
        }

        public static Exception AliyunStsTokenOverTime(string casuse, string directoryPermissionName, bool needWrite)
        {
            ClientException ex = new ClientException(ClientErrorCodes.AliyunStsTokenOverTime, nameof(AliyunStsTokenOverTime));

            ex.Data["Cause"] = casuse;
            ex.Data["DirectoryPermissionName"] = directoryPermissionName;
            ex.Data["NeedWrite"] = needWrite.ToString();

            return ex;
        }

        public static Exception NotLogined()
        {
            ClientException ex = new ClientException(ClientErrorCodes.NotLogined, nameof(NotLogined));

            return ex;
        }

        public static Exception SmsCodeValidateError(string mobile)
        {
            ClientException ex = new ClientException(ClientErrorCodes.SmsCodeValidateError, nameof(SmsCodeValidateError));

            ex.Data["Mobile"] = mobile;

            return ex;
        }

        public static Exception UploadError(string cause)
        {
            ClientException ex = new ClientException(ClientErrorCodes.UploadError, nameof(UploadError));

            ex.Data["Cause"] = cause;

            return ex;
        }

        internal static Exception UnSupportedModelType(string? modelFullName)
        {
            ClientException ex = new ClientException(ClientErrorCodes.UnSupportedModelType, nameof(UnSupportedModelType));

            ex.Data["FullName"] = modelFullName;

            return ex;
        }

        internal static Exception OperationInvalidCauseofSyncingAfterReconnected()
        {
            ClientException ex = new ClientException(ClientErrorCodes.OperationInvalidCauseofSyncingAfterReconnected, nameof(OperationInvalidCauseofSyncingAfterReconnected));

            return ex;
        }

        public static Exception NoSuchDirectory(string directoryName)
        {
            ClientException ex = new ClientException(ClientErrorCodes.NoSuchDirectory, nameof(NoSuchDirectory));
            ex.Data["DirectoryName"] = directoryName;
            return ex;
        }

        public static Exception SyncError(SyncStatus syncStatus)
        {
            ClientException ex = new ClientException(ClientErrorCodes.SyncError, nameof(SyncError), null, null);

            return ex;
        }
    }
}