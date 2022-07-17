using System;
using System.Text.Json;

using Aliyun.Acs.Core.Exceptions;

namespace HB.Infrastructure.Aliyun
{
    /// <summary>
    /// from 6000 - 6999
    /// </summary>
    internal static class AliyunErrorCodes
    {
        public static ErrorCode OssError { get; set; } = new ErrorCode(nameof(OssError), "");
        public static ErrorCode StsError { get; set; } = new ErrorCode(nameof(StsError), "");
        public static ErrorCode SmsSendError { get; set; } = new ErrorCode(nameof(SmsSendError), "");
        public static ErrorCode SmsFormatError { get; set; } = new ErrorCode(nameof(SmsFormatError), "");
        public static ErrorCode SmsClientError { get; set; } = new ErrorCode(nameof(SmsClientError), "");
        public static ErrorCode SmsServerError { get; set; } = new ErrorCode(nameof(SmsServerError), "");
        public static ErrorCode SmsCacheError { get; set; } = new ErrorCode(nameof(SmsCacheError), "");
    }

    internal static class AliyunExceptions
    {
        internal static Exception OssError(string bucket, string cause)
        {
            AliyunException exception = new AliyunException(AliyunErrorCodes.OssError, nameof(OssError), null, null);

            exception.Data["Bucket"] = bucket;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception SmsSendError(string mobile, string? code, string? message)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.SmsSendError, nameof(SmsSendError));

            exception.Data["Mobile"] = mobile;
            exception.Data["Code"] = code;
            exception.Data["Message"] = message;

            return exception;
        }

        internal static Exception SmsCacheError(string cause, CacheException ex)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.SmsCacheError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception SmsServerError(string cause, AliyunException ex)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.SmsServerError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception SmsFormatError(string cause, JsonException ex)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.SmsFormatError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception SmsClientError(string cause, ClientException ex)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.SmsClientError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception StsError(Guid userId, string bucketname, string direcotry, bool readOnly, Exception ex)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.StsError, nameof(StsError), ex);

            exception.Data["UserId"] = userId.ToString();
            exception.Data["BucketName"] = bucketname;
            exception.Data["Directory"] = direcotry;
            exception.Data["ReadOnly"] = readOnly;

            return exception;

        }
    }
}