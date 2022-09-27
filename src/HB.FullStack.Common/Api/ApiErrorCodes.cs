namespace System
{
    public static partial class ApiErrorCodes
    {
        public static readonly ErrorCode ApiClientInnerError = new ErrorCode(nameof(ApiClientInnerError), "ApiClient本身出错");

        public static ErrorCode NoAuthority { get; } = new ErrorCode(nameof(NoAuthority), "");
        public static ErrorCode AccessTokenExpired { get; } = new ErrorCode(nameof(AccessTokenExpired), "");
        public static ErrorCode ModelValidationError { get; } = new ErrorCode(nameof(ModelValidationError), "");
        public static ErrorCode ApiNotAvailable { get; } = new ErrorCode(nameof(ApiNotAvailable), "");
        public static ErrorCode ApiErrorUnkownFormat { get; } = new ErrorCode(nameof(ApiErrorUnkownFormat), "");
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

        #region
        //TODO: 客户端应该针对于这些Authorize类的Error进行相应处理

        public static ErrorCode AuthorizationNotFound { get; } = new ErrorCode(nameof(AuthorizationNotFound), "");
        public static ErrorCode AuthorizationPasswordWrong { get; } = new ErrorCode(nameof(AuthorizationPasswordWrong), "");
        public static ErrorCode AccessTokenRefreshing { get; } = new ErrorCode(nameof(AccessTokenRefreshing), "同一设备正在Refreshing");
        public static ErrorCode RefreshAccessTokenError { get; } = new ErrorCode(nameof(RefreshAccessTokenError), "");
        public static ErrorCode AuthorizationInvalideDeviceId { get; } = new ErrorCode(nameof(AuthorizationInvalideDeviceId), "");
        public static ErrorCode AuthorizationInvalideUserId { get; } = new ErrorCode(nameof(AuthorizationInvalideUserId), "");
        public static ErrorCode AuthorizationUserSecurityStampChanged { get; } = new ErrorCode(nameof(AuthorizationUserSecurityStampChanged), "");
        public static ErrorCode AuthorizationRefreshTokenExpired { get; } = new ErrorCode(nameof(AuthorizationRefreshTokenExpired), "");
        public static ErrorCode AuthorizationNoTokenInStore { get; } = new ErrorCode(nameof(AuthorizationNoTokenInStore), "");
        public static ErrorCode AuthorizationMobileNotConfirmed { get; } = new ErrorCode(nameof(AuthorizationMobileNotConfirmed), "");
        public static ErrorCode AuthorizationEmailNotConfirmed { get; } = new ErrorCode(nameof(AuthorizationEmailNotConfirmed), "");
        public static ErrorCode AuthorizationLockedOut { get; } = new ErrorCode(nameof(AuthorizationLockedOut), "");
        public static ErrorCode AuthorizationOverMaxFailedCount { get; } = new ErrorCode(nameof(AuthorizationOverMaxFailedCount), "");
        public static ErrorCode JwtSigningCertNotFound { get; } = new ErrorCode(nameof(JwtSigningCertNotFound), "");
        public static ErrorCode JwtEncryptionCertNotFound { get; } = new ErrorCode(nameof(JwtEncryptionCertNotFound), "");
        public static ErrorCode ServerReturnError { get; } = new ErrorCode(nameof(ServerReturnError), "Server收到了请求，但返回了错误");
        public static ErrorCode ApiModelError { get; } = new ErrorCode(nameof(ApiModelError), "ApiRequest等Model出错");
        public static ErrorCode ApiAuthenticationError { get; } = new ErrorCode(nameof(ApiAuthenticationError), "ApiClient请求时，授权信息有错或缺少");
        public static ErrorCode ApiResourceError { get; } = new ErrorCode(nameof(ApiResourceError), "");

        #endregion
    }

    public static partial class ApiExceptions
    {

        public static Exception ServerUnkownError(string responseString)
        {
            return new ApiException(ApiErrorCodes.ServerUnKownError, "Server返回了其他格式的错误表示，赶紧处理", null, new { Response = responseString });
        }

        internal static Exception ApiClientInnerError(string cause, Exception? innerEx, object? context)
        {
            return new ApiException(ApiErrorCodes.ApiClientInnerError, cause, innerEx, context);
        }

        internal static Exception ServerReturnError(ErrorCode errorCode)
        {
            return new ApiException(errorCode, "Server认为请求无法返回正确");
        }

        internal static Exception ApiModelError(string cause, Exception? innerEx, object? context)
        {
            return new ApiException(ApiErrorCodes.ApiModelError, cause, innerEx, context);
        }

        internal static Exception ApiAuthenticationError(string cause, Exception? innerEx, object? context)
        {
            return new ApiException(ApiErrorCodes.ApiAuthenticationError, cause, innerEx, context);
        }

        internal static Exception ApiResourceError(string cause, Exception? innerEx, object? context)
        {
            return new ApiException(ApiErrorCodes.ApiResourceError, cause, innerEx, context);
        }

        public static Exception ServerNullReturn(string parameter)
        {
            return new ApiException(ApiErrorCodes.ServerNullReturn, "Server端返回NULL", null, new { Parameter = parameter });
        }
    }
}