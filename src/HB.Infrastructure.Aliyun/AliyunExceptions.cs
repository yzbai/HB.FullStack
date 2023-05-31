using System;
using System.Text.Json;

using Aliyun.Acs.Core.Exceptions;

namespace HB.Infrastructure.Aliyun
{
    internal static class AliyunExceptions
    {
        internal static Exception OssError(string bucket, string cause)
        {
            AliyunException exception = new AliyunException(ErrorCodes.OssError, nameof(OssError), null, null);

            exception.Data["Bucket"] = bucket;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception SmsSendError(string mobile, string? code, string? message)
        {
            AliyunException exception = new AliyunException(ErrorCodes.SmsSendError, nameof(SmsSendError));

            exception.Data["Mobile"] = mobile;
            exception.Data["Code"] = code;
            exception.Data["Message"] = message;

            return exception;
        }

        internal static Exception SmsCacheError(string cause, Exception ex)
        {
            AliyunException exception = new AliyunException(ErrorCodes.SmsCacheError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception SmsServerError(string cause, AliyunException ex)
        {
            AliyunException exception = new AliyunException(ErrorCodes.SmsServerError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception SmsFormatError(string cause, JsonException ex)
        {
            AliyunException exception = new AliyunException(ErrorCodes.SmsFormatError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception SmsClientError(string cause, ClientException ex)
        {
            AliyunException exception = new AliyunException(ErrorCodes.SmsClientError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception StsError(string userIdString, string bucketname, string direcotry, bool readOnly, Exception ex)
        {
            AliyunException exception = new AliyunException(ErrorCodes.StsError, nameof(StsError), ex);

            exception.Data["UserId"] = userIdString;
            exception.Data["BucketName"] = bucketname;
            exception.Data["Directory"] = direcotry;
            exception.Data["ReadOnly"] = readOnly;

            return exception;

        }
    }
}