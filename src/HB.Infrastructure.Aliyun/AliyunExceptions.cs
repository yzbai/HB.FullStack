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
            SmsException exception = new SmsException(ErrorCodes.SmsSendError, nameof(SmsSendError));

            exception.Data["Mobile"] = mobile;
            exception.Data["Code"] = code;
            exception.Data["Message"] = message;

            return exception;
        }

        internal static Exception SmsCacheError(string cause, Exception ex)
        {
            SmsException exception = new SmsException(ErrorCodes.SmsCacheError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception SmsServerError(string cause, AliyunException ex)
        {
            SmsException exception = new SmsException(ErrorCodes.SmsServerError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception SmsFormatError(string cause, JsonException ex)
        {
            SmsException exception = new SmsException(ErrorCodes.SmsFormatError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception SmsClientError(string cause, ClientException ex)
        {
            SmsException exception = new SmsException(ErrorCodes.SmsClientError, cause, ex);

            exception.Data["Cause"] = cause;

            return exception;

        }

        internal static Exception StsError(Guid userId, string bucketname, string direcotry, bool readOnly, Exception ex)
        {
            SmsException exception = new SmsException(ErrorCodes.StsError, nameof(StsError), ex);

            exception.Data["UserId"] = userId.ToString();
            exception.Data["BucketName"] = bucketname;
            exception.Data["Directory"] = direcotry;
            exception.Data["ReadOnly"] = readOnly;

            return exception;

        }
    }
}