using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Client.Maui
{
    public static class Exceptions
    {
        public static Exception AliyunStsTokenReturnNull()
        {
            MauiException exception = new MauiException(ErrorCodes.AliyunStsTokenReturnNull, "AliyunStsTokenReturnNull");

            return exception;
        }

        internal static Exception NoInternet()
        {
            MauiException exception = new MauiException(ErrorCodes.NoInternet, "NoInternet");
            return exception;
        }

        public static Exception AliyunOssPutObjectError(string bucketName, string key)
        {
            MauiException exception = new MauiException(ErrorCodes.AliyunOssPutObjectError, "AliyunOssPutObjectError");

            exception.Data["BucketName"] = bucketName;
            exception.Data["Key"] = key;

            return exception;
        }

        public static Exception AliyunOssPutObjectError(string cause, Exception? innerException)
        {
            MauiException ex = new MauiException(ErrorCodes.AliyunOssPutObjectError, "AliyunOssPutObjectError", innerException);

            ex.Data["Cause"] = cause;

            return ex;
        }

        internal static Exception CaptchaErrorReturn(string? captcha, ApiRequest request)
        {
            MauiException ex = new MauiException(ErrorCodes.CaptchaErrorReturn, "CaptchaErrorReturn");
            ex.Data["Captcha"] = captcha;
            ex.Data["RequestUrl"] = request;

            return ex;
        }

        internal static Exception UnSupportedResToModel(string resName, string modelName)
        {
            MauiException ex = new MauiException(ErrorCodes.UnSupportedResToModel, "UnSupportedResToModel");
            ex.Data["ResName"] = resName;
            ex.Data["ModelName"] = modelName;

            return ex;
        }
    }
}
