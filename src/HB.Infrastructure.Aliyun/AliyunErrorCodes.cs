using System;
using System.Reflection;
using System.Text.Json;

using Aliyun.Acs.Core.Exceptions;

namespace HB.Infrastructure.Aliyun
{
    /// <summary>
    /// from 6000 - 6999
    /// </summary>
    internal static class AliyunErrorCodes
    {
        public static ErrorCode OssError { get; set; } = new ErrorCode(ErrorCodeStartIds.ALIYUN + 0, nameof(OssError), "");
        public static ErrorCode StsError { get; set; } = new ErrorCode(ErrorCodeStartIds.ALIYUN + 1, nameof(StsError), "");
        public static ErrorCode SmsSendError { get; set; } = new ErrorCode(ErrorCodeStartIds.ALIYUN + 2, nameof(SmsSendError), "");
        public static ErrorCode SmsFormatError { get; set; } = new ErrorCode(ErrorCodeStartIds.ALIYUN + 3, nameof(SmsFormatError), "");
        public static ErrorCode SmsClientError { get; set; } = new ErrorCode(ErrorCodeStartIds.ALIYUN + 4, nameof(SmsClientError), "");
        public static ErrorCode SmsServerError { get; set; } = new ErrorCode(ErrorCodeStartIds.ALIYUN + 5, nameof(SmsServerError), "");
        public static ErrorCode SmsCacheError { get; set; } = new ErrorCode(ErrorCodeStartIds.ALIYUN + 6, nameof(SmsCacheError), "");
    }

    internal static class Exceptions
    {
        internal static Exception OssError(string bucket, string cause)
        {
            AliyunException exception = new AliyunException(AliyunErrorCodes.OssError);

            exception.Data["Bucket"] = bucket;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception SmsSendError(string mobile, string? code, string? message)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.SmsSendError);

            exception.Data["Mobile"] = mobile;
            exception.Data["Code"] = code;
            exception.Data["Message"] = message;

            return exception;
        }

        internal static Exception SmsCacheError(string cause, CacheException ex)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.SmsCacheError, ex);

            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception SmsServerError(string cause, AliyunException ex)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.SmsServerError, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception SmsFormatError(string cause, JsonException ex)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.SmsFormatError, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception SmsClientError(string cause, ClientException ex)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.SmsClientError, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception StsError(long userId, string bucketname, string direcotry, bool readOnly, Exception ex)
        {
            SmsException exception = new SmsException(AliyunErrorCodes.StsError, ex);

            exception.Data["UserId"] = userId;
            exception.Data["BucketName"] = bucketname;
            exception.Data["Directory"] = direcotry;
            exception.Data["ReadOnly"] = readOnly;

            return exception;

        }
    }
}