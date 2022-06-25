using HB.FullStack.Common.Api;

using System.Net.Http;

namespace System
{
    public static partial class ApiErrorCodes
    {
        public static ErrorCode NoAuthority { get; } = new ErrorCode(nameof(NoAuthority), "");
        public static ErrorCode AccessTokenExpired { get; } = new ErrorCode(nameof(AccessTokenExpired), "");
        public static ErrorCode ModelValidationError { get; } = new ErrorCode(nameof(ModelValidationError), "");
        public static ErrorCode ApiNotAvailable { get; } = new ErrorCode(nameof(ApiNotAvailable), "");
        public static ErrorCode ApiErrorUnkownFormat { get; } = new ErrorCode(nameof(ApiErrorUnkownFormat), "");
        public static ErrorCode NotApiResourceEntity { get; } = new ErrorCode(nameof(NotApiResourceEntity), "");
        public static ErrorCode ApiSmsCodeInvalid { get; } = new ErrorCode(nameof(ApiSmsCodeInvalid), "短信验证码错误。");
        public static ErrorCode SmsServiceError { get; } = new ErrorCode(nameof(SmsServiceError), "");
        public static ErrorCode CommonResourceTokenNeeded { get; } = new ErrorCode(nameof(CommonResourceTokenNeeded), "");
        public static ErrorCode CommonResourceTokenError { get; } = new ErrorCode(nameof(CommonResourceTokenError), "");
        public static ErrorCode ApiUploadEmptyFile { get; } = new ErrorCode(nameof(ApiUploadEmptyFile), "");
        public static ErrorCode ApiUploadOverSize { get; } = new ErrorCode(nameof(ApiUploadOverSize), "");
        public static ErrorCode ApiUploadWrongType { get; } = new ErrorCode(nameof(ApiUploadWrongType), "");
        public static ErrorCode HttpsRequired { get; } = new ErrorCode(nameof(HttpsRequired), "");
        public static ErrorCode FromExceptionController { get; } = new ErrorCode(nameof(FromExceptionController), "");
        public static ErrorCode ApiCapthaError { get; } = new ErrorCode(nameof(ApiCapthaError), "");
        public static ErrorCode ApiUploadFailed { get; } = new ErrorCode(nameof(ApiUploadFailed), "");
        public static ErrorCode ServerUnKownError { get; } = new ErrorCode(nameof(ServerUnKownError), "");
        public static ErrorCode ClientError { get; } = new ErrorCode(nameof(ClientError), "");
        public static ErrorCode NullReturn { get; } = new ErrorCode(nameof(NullReturn), "");
        public static ErrorCode Timeout { get; } = new ErrorCode(nameof(Timeout), "");
        public static ErrorCode RequestCanceled { get; } = new ErrorCode(nameof(RequestCanceled), "");
        public static ErrorCode AliyunStsTokenReturnNull { get; } = new ErrorCode(nameof(AliyunStsTokenReturnNull), "");
        public static ErrorCode AliyunOssPutObjectError { get; } = new ErrorCode(nameof(AliyunOssPutObjectError), "");
        public static ErrorCode TokenRefreshError { get; } = new ErrorCode(nameof(TokenRefreshError), "");
        public static ErrorCode UserActivityFilterError { get; } = new ErrorCode(nameof(UserActivityFilterError), "");
        public static ErrorCode FileUpdateRequestCountNotEven { get; } = new ErrorCode(nameof(FileUpdateRequestCountNotEven), "");
        public static ErrorCode LackApiResourceAttribute { get; } = new ErrorCode(nameof(LackApiResourceAttribute), "");
        public static ErrorCode RequestTimeout { get; } = new ErrorCode(nameof(RequestTimeout), "");

        /// <summary>
        /// 这个Request已经用过一次了
        /// </summary>
        public static ErrorCode RequestAlreadyUsed { get; } = new ErrorCode(nameof(RequestAlreadyUsed), "");

        /// <summary>
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// </summary>
        public static ErrorCode RequestUnderlyingIssue { get; } = new ErrorCode(nameof(RequestUnderlyingIssue), "");

        public static ErrorCode HttpResponseDeserializeError { get; } = new ErrorCode(nameof(HttpResponseDeserializeError), "");
        public static ErrorCode ApiClientSendUnkownError { get; } = new ErrorCode(nameof(ApiClientSendUnkownError), "");
        public static ErrorCode ApiClientGetStreamUnkownError { get; } = new ErrorCode(nameof(ApiClientGetStreamUnkownError), "");
        public static ErrorCode UploadError { get; } = new ErrorCode(nameof(UploadError), "");
        public static ErrorCode ApiRequestInvalidateError { get; } = new ErrorCode(nameof(ApiRequestInvalidateError), "");
        public static ErrorCode ApiRequestSetJwtError { get; } = new ErrorCode(nameof(ApiRequestSetJwtError), "");
        public static ErrorCode ApiRequestSetApiKeyError { get; } = new ErrorCode(nameof(ApiRequestSetApiKeyError), "");
        public static ErrorCode ApiClientUnkownError { get; } = new ErrorCode(nameof(ApiClientUnkownError), "");
        public static ErrorCode ServerNullReturn { get; } = new ErrorCode(nameof(ServerNullReturn), "");
        public static ErrorCode ArgumentIdsError { get; } = new ErrorCode(nameof(ArgumentIdsError), "");
        public static ErrorCode RequestIntervalFilterError { get; } = new ErrorCode(nameof(RequestIntervalFilterError), "");
        public static ErrorCode CapthcaNotFound { get; } = new ErrorCode(nameof(CapthcaNotFound), "");
        public static ErrorCode CapthcaError { get; } = new ErrorCode(nameof(CapthcaError), "");
        public static ErrorCode NeedOwnerResId { get; } = new ErrorCode(nameof(NeedOwnerResId), "");

        public static ErrorCode LackParent1ResIdAttribute { get; } = new ErrorCode(nameof(LackParent1ResIdAttribute), "因为制定了Parent1ResName，但缺少Parent1ResIdAttribute");

        public static ErrorCode LackParent2ResIdAttribute { get; } = new ErrorCode(nameof(LackParent2ResIdAttribute), "因为制定了Parent2ResName，但缺少Parent2ResIdAttribute");
    }

    public static partial class ApiExceptions
    {
        internal static Exception RequestUnderlyingIssue(ApiRequest request, HttpRequestException innerException)
        {
            ApiException ex = new(ApiErrorCodes.RequestUnderlyingIssue, innerException);

            return ex;
        }

        internal static Exception RequestAlreadyUsed(ApiRequest request, Exception innerException)
        {
            ApiException ex = new(ApiErrorCodes.RequestAlreadyUsed, innerException);

            return ex;
        }

        public static Exception RequestCanceled(ApiRequest request, Exception innerException)
        {
            ApiException ex = new(ApiErrorCodes.RequestCanceled, innerException);

            return ex;
        }

        public static Exception ApiClientGetStreamUnkownError(ApiRequest request, Exception innerException)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiClientGetStreamUnkownError, innerException);

            return ex;
        }

        public static Exception ApiRequestInvalidateError(ApiRequest request, string validationErrorMessage)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiRequestInvalidateError);

            ex.Data["ValidationError"] = validationErrorMessage;

            return ex;
        }

        public static Exception RequestTimeout(ApiRequest request, Exception innerException)
        {
            ApiException ex = new ApiException(ApiErrorCodes.RequestTimeout, innerException);

            return ex;
        }

        public static Exception ApiClientUnkownError(string cause, ApiRequest request, Exception innerException)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiClientUnkownError, innerException);

            ex.Data["Cause"] = cause;

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
            ApiException ex = new ApiException(ApiErrorCodes.UploadError, innerException);

            ex.Data["Cause"] = cause;
            ex.Data["FileName"] = fileName;

            return ex;
        }

        public static Exception ApiRequestSetJwtError(ApiRequest request)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiRequestSetJwtError);

            return ex;
        }

        public static Exception ApiRequestSetApiKeyError(ApiRequest request)
        {
            ApiException ex = new ApiException(ApiErrorCodes.ApiRequestSetApiKeyError);

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

        public static Exception NeedOwnerResId(string resName)
        {
            ApiException ex = new ApiException(ApiErrorCodes.NeedOwnerResId);

            ex.Data["ResName"] = resName;

            return ex;
        }

        internal static Exception LackParent1ResAttribute(Type type)
        {
            ApiException ex = new ApiException(ApiErrorCodes.LackParent1ResIdAttribute);

            ex.Data["ResName"] = type.FullName;

            return ex;
        }

        internal static Exception LackParent2ResAttribute(Type type)
        {
            ApiException ex = new ApiException(ApiErrorCodes.LackParent2ResIdAttribute);

            ex.Data["ResName"] = type.FullName;

            return ex;
        }
    }
}