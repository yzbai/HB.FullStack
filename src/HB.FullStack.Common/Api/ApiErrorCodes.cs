using HB.FullStack.Common;
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
        public static ErrorCode RequestUnderlyingIssue { get; } = new ErrorCode(ErrorCodeStartIds.API + 31, nameof(RequestUnderlyingIssue), "");
        public static ErrorCode HttpResponseDeserializeError { get; } = new ErrorCode(ErrorCodeStartIds.API + 32, nameof(HttpResponseDeserializeError), "");
        public static ErrorCode ApiClientSendUnkownError { get; } = new ErrorCode(ErrorCodeStartIds.API + 33, nameof(ApiClientSendUnkownError), "");
        public static ErrorCode ApiClientGetStreamUnkownError { get; } = new ErrorCode(ErrorCodeStartIds.API + 34, nameof(ApiClientGetStreamUnkownError), "");
        public static ErrorCode UploadError { get; } = new ErrorCode(ErrorCodeStartIds.API + 35, nameof(UploadError), "");
        public static ErrorCode ApiRequestInvalidateError { get; } = new ErrorCode(ErrorCodeStartIds.API + 36, nameof(ApiRequestInvalidateError), "");
        public static ErrorCode ApiRequestSetJwtError { get; } = new ErrorCode(ErrorCodeStartIds.API + 37, nameof(ApiRequestSetJwtError), "");
        public static ErrorCode ApiRequestSetApiKeyError { get; } = new ErrorCode(ErrorCodeStartIds.API + 38, nameof(ApiRequestSetApiKeyError), "");
        public static ErrorCode ApiClientUnkownError { get; } = new ErrorCode(ErrorCodeStartIds.API + 39, nameof(ApiClientUnkownError), "");
        public static ErrorCode ServerNullReturn { get; } = new ErrorCode(ErrorCodeStartIds.API + 40, nameof(ServerNullReturn), "");
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

        public static Exception ApiClientGetStreamUnkownError(ApiRequest request, Exception innerException)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiClientGetStreamUnkownError, innerException);

            ex.Data["Request"] = request.ToDebugInfo();

            return ex;
        }

        public static Exception ApiRequestInvalidateError(ApiRequest request, string validationErrorMessage)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiRequestInvalidateError);

            ex.Data["Request"] = request.ToDebugInfo();
            ex.Data["ValidationError"] = validationErrorMessage;

            return ex;
        }

        public static Exception RequestTimeout(ApiRequest request, Exception innerException)
        {
            ApiException ex = new ApiException(ApiErrorCodes.RequestTimeout, innerException);

            ex.Data["Request"] = request.ToDebugInfo();

            return ex;
        }

        public static Exception ApiClientUnkownError(string cause, ApiRequest request, Exception innerException)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiClientUnkownError, innerException);

            ex.Data["Cause"] = cause;
            ex.Data["Request"] = request.ToDebugInfo();

            return ex;
        }

        public static Exception ServerUnkownError(string response)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ServerUnKownError);
            ex.Data["Response"] = response;
            return ex;
        }

        public static Exception ApiClientSendUnkownError(ApiRequest request, Exception innerException)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiClientSendUnkownError, innerException);

            ex.Data["Request"] = request.ToDebugInfo();

            return ex;
        }

        public static Exception ServerReturnError(ErrorCode errorCode)
        {
            ApiException ex = new ApiException(errorCode);
            return ex;
        }


        internal static Exception FileUpdateRequestCountNotEven()
        {
            ApiException ex = new ApiException(ApiErrorCodes.FileUpdateRequestCountNotEven);
            return ex;
        }

        internal static Exception LackApiResourceAttribute(string? type)
        {
            ApiException ex = new ApiException(ApiErrorCodes.LackApiResourceAttribute);
            ex.Data["Type"] = type;

            return ex;
        }

        internal static Exception HttpResponseDeserializeError(ApiRequest request, string? responseString)
        {
            ApiException ex = new ApiException(ApiErrorCodes.HttpResponseDeserializeError);
            ex.Data["Request"] = SerializeUtil.ToJson(request);
            ex.Data["ResponseString"] = responseString;

            return ex;
        }

        public static Exception UploadError(string cause, string? fileName, Exception? innerException)
        {
            ApiException ex = new ApiException(ApiErrorCodes.UploadError);

            ex.Data["Cause"] = cause;
            ex.Data["FileName"] = fileName;

            return ex;
        }

        public static Exception ApiRequestSetJwtError(ApiRequest request)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiRequestSetJwtError);

            ex.Data["Request"] = request.ToDebugInfo();

            return ex;
        }

        public static Exception ApiRequestSetApiKeyError(ApiRequest request)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiRequestSetApiKeyError);

            ex.Data["Request"] = request.ToDebugInfo();

            return ex;
        }

        public static Exception TokenRefreshError(string cause)
        {
            ApiException ex = new ApiException(ApiErrorCodes.TokenRefreshError);

            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception ServerNullReturn(string parameter)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ServerNullReturn);

            ex.Data["Parameter"] = parameter;

            return ex;
        }
    }
}
