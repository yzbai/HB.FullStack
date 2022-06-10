﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui
{
    public static class ErrorCodes
    {
        //TODO: fix the error code
        public static ErrorCode AliyunStsTokenReturnNull { get; } = new ErrorCode(3, nameof(AliyunStsTokenReturnNull), "");
        public static ErrorCode NoInternet { get; } = new ErrorCode(3, nameof(NoInternet), "");

        public static ErrorCode AliyunOssPutObjectError { get; } = new ErrorCode(ErrorCodeStartIds.CLIENT + 7, nameof(AliyunOssPutObjectError), "");

    }

    public static class Exceptions
    {
        public static Exception AliyunStsTokenReturnNull()
        {
            MauiException exception = new MauiException(ErrorCodes.AliyunStsTokenReturnNull);

            return exception;
        }

        internal static Exception NoInternet()
        {
            MauiException exception = new MauiException(ErrorCodes.NoInternet);
            return exception;
        }


        public static Exception AliyunOssPutObjectError(string bucketName, string key)
        {
            MauiException exception = new MauiException(ErrorCodes.AliyunOssPutObjectError);

            exception.Data["BucketName"] = bucketName;
            exception.Data["Key"] = key;

            return exception;
        }

        public static Exception AliyunOssPutObjectError(string cause, Exception? innerException)
        {
            MauiException ex = new MauiException(ErrorCodes.AliyunOssPutObjectError, innerException);

            ex.Data["Cause"] = cause;

            return ex;
        }
    }
}
