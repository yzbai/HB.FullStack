using HB.FullStack.Common.Api;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace System
{

    public static partial class ApiErrorCodes
    {
        public static ErrorCode NoAuthority { get; } = new ErrorCode(ErrorCodeStartIds.API + 0, nameof(NoAuthority), "");
        public static ErrorCode AccessTokenExpired { get; } = new ErrorCode(ErrorCodeStartIds.API + 1, nameof(AccessTokenExpired), "");
        public static ErrorCode ModelValidationError { get; } = new ErrorCode(ErrorCodeStartIds.API + 2, nameof(ModelValidationError), "");
        public static ErrorCode ApiNotAvailable { get; } = new ErrorCode(ErrorCodeStartIds.API + 3, nameof(ApiNotAvailable), "");
        public static ErrorCode ApiErrorUnkownFormat { get; } = new ErrorCode(ErrorCodeStartIds.API + 4, nameof(ApiErrorUnkownFormat), "");
        public static ErrorCode NotApiResourceEntity { get; } = new ErrorCode(ErrorCodeStartIds.API + 5, nameof(NotApiResourceEntity), "");
        public static ErrorCode ApiSmsCodeInvalid { get; } = new ErrorCode(ErrorCodeStartIds.API + 6, nameof(ApiSmsCodeInvalid), "");
        public static ErrorCode SmsServiceError { get; } = new ErrorCode(ErrorCodeStartIds.API + 7, nameof(SmsServiceError), "");
        public static ErrorCode PublicResourceTokenNeeded { get; } = new ErrorCode(ErrorCodeStartIds.API + 8, nameof(PublicResourceTokenNeeded), "");
        public static ErrorCode PublicResourceTokenError { get; } = new ErrorCode(ErrorCodeStartIds.API + 9, nameof(PublicResourceTokenError), "");
        public static ErrorCode ApiUploadEmptyFile { get; } = new ErrorCode(ErrorCodeStartIds.API + 10, nameof(ApiUploadEmptyFile), "");
        public static ErrorCode ApiUploadOverSize { get; } = new ErrorCode(ErrorCodeStartIds.API + 11, nameof(ApiUploadOverSize), "");
        public static ErrorCode ApiUploadWrongType { get; } = new ErrorCode(ErrorCodeStartIds.API + 12, nameof(ApiUploadWrongType), "");
        public static ErrorCode HttpsRequired { get; } = new ErrorCode(ErrorCodeStartIds.API + 13, nameof(HttpsRequired), "");
        public static ErrorCode FromExceptionController { get; } = new ErrorCode(ErrorCodeStartIds.API + 14, nameof(FromExceptionController), "");
        public static ErrorCode ApiCapthaError { get; } = new ErrorCode(ErrorCodeStartIds.API + 15, nameof(ApiCapthaError), "");
        public static ErrorCode ApiUploadFailed { get; } = new ErrorCode(ErrorCodeStartIds.API + 16, nameof(ApiUploadFailed), "");
        public static ErrorCode ServerUnKownError { get; } = new ErrorCode(ErrorCodeStartIds.API + 17, nameof(ServerUnKownError), "");
        public static ErrorCode ClientError { get; } = new ErrorCode(ErrorCodeStartIds.API + 18, nameof(ClientError), "");
        public static ErrorCode NullReturn { get; } = new ErrorCode(ErrorCodeStartIds.API + 19, nameof(NullReturn), "");
        public static ErrorCode Timeout { get; } = new ErrorCode(ErrorCodeStartIds.API + 20, nameof(Timeout), "");
        public static ErrorCode RequestCanceled { get; } = new ErrorCode(ErrorCodeStartIds.API + 21, nameof(RequestCanceled), "");
        public static ErrorCode AliyunStsTokenReturnNull { get; } = new ErrorCode(ErrorCodeStartIds.API + 22, nameof(AliyunStsTokenReturnNull), "");
        public static ErrorCode AliyunOssPutObjectError { get; } = new ErrorCode(ErrorCodeStartIds.API + 23, nameof(AliyunOssPutObjectError), "");
        public static ErrorCode TokenRefreshError { get; } = new ErrorCode(ErrorCodeStartIds.API + 24, nameof(TokenRefreshError), "");
        public static ErrorCode UserActivityFilterError { get; } = new ErrorCode(ErrorCodeStartIds.API + 25, nameof(UserActivityFilterError), "");
        public static ErrorCode FileUpdateRequestCountNotEven { get; } = new ErrorCode(ErrorCodeStartIds.API + 26, nameof(FileUpdateRequestCountNotEven), "");
        public static ErrorCode LackApiResourceAttribute { get; } = new ErrorCode(ErrorCodeStartIds.API + 27, nameof(LackApiResourceAttribute), "");
        public static ErrorCode RequestTimeout { get; } = new ErrorCode(ErrorCodeStartIds.API + 28, nameof(RequestTimeout), "");

        /// <summary>
        /// 这个Request已经用过一次了
        /// </summary>
        public static ErrorCode RequestAlreadyUsed { get; } = new ErrorCode(ErrorCodeStartIds.API + 30, nameof(RequestAlreadyUsed), "");

        /// <summary>
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// </summary>
        public static ErrorCode RequestUnderlyingIssue { get;  } = new ErrorCode(ErrorCodeStartIds.API + 31, nameof(RequestUnderlyingIssue), "");
        
        
    }

    public static partial class ApiExceptions
    {

        internal static Exception RequestUnderlyingIssue(ApiRequest request, HttpRequestException innerException)
        {
            ApiException ex = new(ApiErrorCodes.RequestUnderlyingIssue, innerException);
            ex.Data["Request"] = request.ToDebugInfo();

            return ex;
        }

        internal static Exception RequestAlreadyUsed(ApiRequest request, Exception innerException)
        {
            ApiException ex = new(ApiErrorCodes.RequestAlreadyUsed, innerException);
            ex.Data["Request"] = request.ToDebugInfo();

            return ex;
        }


        public static Exception RequestCanceled(ApiRequest request, Exception innerException)
        {
            ApiException ex = new(ApiErrorCodes.RequestCanceled, innerException);
            ex.Data["Request"] = request.ToDebugInfo();

            return ex;
        }

        public static Exception RequestTimeout(ApiRequest request, Exception innerException)
        {
            ApiException ex = new ApiException(ApiErrorCodes.RequestTimeout, innerException);

            ex.Data["Request"] = request.ToDebugInfo();

            return ex;
        }

        public static Exception ClientUnkownError(ApiRequest request, Exception innerException)
        {
            throw new NotImplementedException();
        }

        public static Exception ServerUnkownError(string response)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ServerUnKownError);
            ex.Data["Response"] = response;
            return ex;
        }

        public static Exception ClientError(string cause, Exception innerException)
        {
            throw new NotImplementedException();
        }

        public static Exception ServerReturnError(ErrorCode errorCode)
        {
            ApiException ex = new ApiException(errorCode);
            return ex;
        }

        public static Exception AliyunOssPutObjectError()
        {
            throw new NotImplementedException();
        }

        public static Exception ApiUploadEmptyFile()
        {
            throw new NotImplementedException();
        }

        public static Exception ApiUploadOverSize()
        {
            throw new NotImplementedException();
        }

        public static Exception ApiUploadWrongType()
        {
            throw new NotImplementedException();
        }

        public static Exception ServerUnkownError(string fileName, Exception innerException)
        {
            throw new NotImplementedException();
        }

        public static Exception ModelValidationError(string cause)
        {
            throw new NotImplementedException();
        }

        public static Exception NoAuthority()
        {
            throw new NotImplementedException();
        }

        public static Exception TokenRefreshError(string cause)
        {
            throw new NotImplementedException();
        }

        public static Exception NoInternet(string cause)
        {
            throw new NotImplementedException();
        }

        public static Exception ServerNullReturn(string parameter)
        {
            throw new NotImplementedException();
        }

        public static Exception AliyunStsTokenReturnNull()
        {
            throw new NotImplementedException();
        }

        public static Exception ModelObjectTypeError(string cause)
        {
            throw new NotImplementedException();
        }

        internal static Exception FileUpdateRequestCountNotEven()
        {
            ApiException ex = new ApiException(ApiErrorCodes.FileUpdateRequestCountNotEven);
            return ex;
        }

        internal static Exception LackApiResourceAttribute(string type)
        {
            ApiException ex = new ApiException(ApiErrorCodes.LackApiResourceAttribute);
            ex.Data["Type"] = type;
            
            return ex;
        }
    }
}
