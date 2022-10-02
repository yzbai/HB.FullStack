using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

namespace System
{
    public static partial class ErrorCodes
    {
        public static readonly ErrorCode CertNotInPackage = new ErrorCode(nameof(CertNotInPackage), "证书没有打包在程序里，将试图在服务器中寻找");
        public static readonly ErrorCode CertNotFound = new ErrorCode(nameof(CertNotFound), "没有找到证书");
        public static readonly ErrorCode IsFileSignatureMatchedError = new ErrorCode(nameof(IsFileSignatureMatchedError), "");
        public static readonly ErrorCode SaveFileError = new ErrorCode(nameof(SaveFileError), "");
        public static readonly ErrorCode ReadFileError = new ErrorCode(nameof(ReadFileError), "");

        public static readonly ErrorCode HttpResponseDeSerializeJsonError = new ErrorCode(nameof(HttpResponseDeSerializeJsonError), "");

        public static readonly ErrorCode SerializeLogError = new ErrorCode(nameof(SerializeLogError), "序列化出错");

        public static readonly ErrorCode UnSerializeLogError = new ErrorCode(nameof(UnSerializeLogError), "反序列化出错");

        public static readonly ErrorCode PerformValidateError = new ErrorCode(nameof(PerformValidateError), "");

        public static readonly ErrorCode TryFromJsonWithCollectionCheckError = new ErrorCode(nameof(TryFromJsonWithCollectionCheckError), "");

        public static readonly ErrorCode EnvironmentVariableError = new ErrorCode(nameof(EnvironmentVariableError), "");

        public static readonly ErrorCode ChangedPackError = new ErrorCode(nameof(ChangedPackError), "");

        public static readonly ErrorCode EventError = new ErrorCode(nameof(EventError), "");

        public static ErrorCode CacheGetError { get; } = new ErrorCode(nameof(CacheGetError), "");
        public static ErrorCode CacheMissed { get; } = new ErrorCode(nameof(CacheMissed), "");
        public static ErrorCode CacheGetEmpty { get; } = new ErrorCode(nameof(CacheGetEmpty), "");
        public static ErrorCode CacheLockAcquireFailed { get; } = new ErrorCode(nameof(CacheLockAcquireFailed), "");

        #region 
        public static ErrorCode OssError { get; set; } = new ErrorCode(nameof(OssError), "");
        public static ErrorCode StsError { get; set; } = new ErrorCode(nameof(StsError), "");
        public static ErrorCode SmsSendError { get; set; } = new ErrorCode(nameof(SmsSendError), "");
        public static ErrorCode SmsFormatError { get; set; } = new ErrorCode(nameof(SmsFormatError), "");
        public static ErrorCode SmsClientError { get; set; } = new ErrorCode(nameof(SmsClientError), "");
        public static ErrorCode SmsServerError { get; set; } = new ErrorCode(nameof(SmsServerError), "");
        public static ErrorCode SmsCacheError { get; set; } = new ErrorCode(nameof(SmsCacheError), "");
        #endregion

        #region
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
        public static ErrorCode ServerReturnError { get; } = new ErrorCode(nameof(ServerReturnError), "Server收到了请求，但返回了错误");
        public static ErrorCode ApiModelError { get; } = new ErrorCode(nameof(ApiModelError), "ApiRequest等Model出错");
        public static ErrorCode ApiAuthenticationError { get; } = new ErrorCode(nameof(ApiAuthenticationError), "ApiClient请求时，授权信息有错或缺少");
        public static ErrorCode ApiResourceError { get; } = new ErrorCode(nameof(ApiResourceError), "");

        #endregion
        #endregion
        #region
        public static readonly ErrorCode SlidingTimeBiggerThanMaxAlive = new ErrorCode(nameof(SlidingTimeBiggerThanMaxAlive), "");
        public static readonly ErrorCode ModelNotHaveKeyAttribute = new ErrorCode(nameof(ModelNotHaveKeyAttribute), "");
        public static readonly ErrorCode ConvertError = new ErrorCode(nameof(ConvertError), "");
        public static readonly ErrorCode CacheLoadedLuaNotFound = new ErrorCode(nameof(CacheLoadedLuaNotFound), "");
        public static readonly ErrorCode CacheInstanceNotFound = new ErrorCode(nameof(CacheInstanceNotFound), "");
        public static readonly ErrorCode NoSuchDimensionKey = new ErrorCode(nameof(NoSuchDimensionKey), "");
        public static readonly ErrorCode NotEnabledForModel = new ErrorCode(nameof(NotEnabledForModel), "");
        public static readonly ErrorCode NotACacheModel = new ErrorCode(nameof(NotACacheModel), "");
        public static readonly ErrorCode UnkownButDeleted = new ErrorCode(nameof(UnkownButDeleted), "");
        public static readonly ErrorCode GetModelsErrorButDeleted = new ErrorCode(nameof(GetModelsErrorButDeleted), "");
        public static readonly ErrorCode SetModelsError = new ErrorCode(nameof(SetModelsError), "");

        public static readonly ErrorCode RemoveModelsError = new ErrorCode(nameof(RemoveModelsError), "");

        public static readonly ErrorCode ForcedRemoveModelsError = new ErrorCode(nameof(ForcedRemoveModelsError), "");

        public static readonly ErrorCode GetModelsError = new ErrorCode(nameof(GetModelsError), "");

        public static readonly ErrorCode CacheInvalidationConcurrencyWithModels = new ErrorCode(nameof(CacheInvalidationConcurrencyWithModels), "");
        public static readonly ErrorCode CacheInvalidationConcurrencyWithTimestamp = new ErrorCode(nameof(CacheInvalidationConcurrencyWithTimestamp), "");

        public static readonly ErrorCode CacheUpdateVersionConcurrency = new ErrorCode(nameof(CacheUpdateVersionConcurrency), "");

        public static readonly ErrorCode SetError = new ErrorCode(nameof(SetError), "");

        public static readonly ErrorCode RemoveError = new ErrorCode(nameof(RemoveError), "");
        public static readonly ErrorCode GetError = new ErrorCode(nameof(GetError), "");

        public static readonly ErrorCode CacheUpdateTimestampConcurrency = new ErrorCode(nameof(CacheUpdateTimestampConcurrency), "");

        public static readonly ErrorCode RemoveMultipleError = new ErrorCode(nameof(RemoveMultipleError), "");

        public static ErrorCode CacheCollectionKeyNotSame { get; } = new ErrorCode(nameof(CacheCollectionKeyNotSame), "");
        public static ErrorCode CacheKeyNotSet { get; } = new ErrorCode(nameof(CacheKeyNotSet), "");
        public static ErrorCode CacheValueNotSet { get; } = new ErrorCode(nameof(CacheValueNotSet), "");
        public static ErrorCode CachedItemTimestampNotSet { get; } = new ErrorCode(nameof(CachedItemTimestampNotSet), "");
        #endregion
        #region
        public static ErrorCode ExecuterError { get; } = new ErrorCode(nameof(ExecuterError), "");
        public static ErrorCode UseDateTimeOffsetOnly { get; } = new ErrorCode(nameof(UseDateTimeOffsetOnly), "");
        public static ErrorCode ModelError { get; } = new ErrorCode(nameof(ModelError), "");
        public static ErrorCode MapperError { get; } = new ErrorCode(nameof(MapperError), "");
        public static ErrorCode SqlError { get; } = new ErrorCode(nameof(SqlError), "");
        public static ErrorCode DatabaseTableCreateError { get; } = new ErrorCode(nameof(DatabaseTableCreateError), "");
        public static ErrorCode MigrateError { get; } = new ErrorCode(nameof(MigrateError), "");
        public static ErrorCode FoundTooMuch { get; } = new ErrorCode(nameof(FoundTooMuch), "");
        public static ErrorCode DatabaseNotWriteable { get; } = new ErrorCode(nameof(DatabaseNotWriteable), "");
        public static ErrorCode ConcurrencyConflict { get; } = new ErrorCode(nameof(ConcurrencyConflict), "");
        public static ErrorCode TransactionError { get; } = new ErrorCode(nameof(TransactionError), "");
        public static ErrorCode SystemInfoError { get; } = new ErrorCode(nameof(SystemInfoError), "");
        public static ErrorCode NotSupported { get; } = new ErrorCode(nameof(NotSupported), "");
        public static ErrorCode BatchError { get; } = new ErrorCode(nameof(BatchError), "");
        public static ErrorCode TypeConverterError { get; } = new ErrorCode(nameof(TypeConverterError), "");
        public static ErrorCode EmptyGuid { get; } = new ErrorCode(nameof(EmptyGuid), "");
        public static ErrorCode UpdatePropertiesCountShouldBePositive { get; } = new ErrorCode(nameof(UpdatePropertiesCountShouldBePositive), "");
        public static ErrorCode LongIdShouldBePositive { get; } = new ErrorCode(nameof(LongIdShouldBePositive), "");
        public static ErrorCode PropertyNotFound { get; } = new ErrorCode(nameof(PropertyNotFound), "");
        public static ErrorCode NoSuchForeignKey { get; } = new ErrorCode(nameof(NoSuchForeignKey), "");

        public static ErrorCode NoSuchProperty { get; } = new ErrorCode(nameof(NoSuchProperty), "");
        public static ErrorCode KeyValueNotLongOrGuid { get; } = new ErrorCode(nameof(KeyValueNotLongOrGuid), "");

        public static ErrorCode ModelHasNotSupportedPropertyType { get; } = new ErrorCode(nameof(ModelHasNotSupportedPropertyType), "");

        public static ErrorCode ModelTimestampError { get; } = new ErrorCode(nameof(ModelTimestampError), "");
        public static ErrorCode NotInitializedYet { get; } = new ErrorCode(nameof(NotInitializedYet), "");
        public static ErrorCode UpdateVersionError { get; } = new ErrorCode(nameof(UpdateVersionError), "");
        #endregion
        #region
        public static ErrorCode NoHandler { get; } = new ErrorCode(nameof(NoHandler), "");
        public static ErrorCode HandlerAlreadyExisted { get; } = new ErrorCode(nameof(HandlerAlreadyExisted), "");
        public static ErrorCode SettingsError { get; } = new ErrorCode(nameof(SettingsError), "");
        #endregion
        #region
        public static ErrorCode NotFound { get; } = new ErrorCode(nameof(NotFound), "");
        public static ErrorCode IdentityNothingConfirmed { get; } = new ErrorCode(nameof(IdentityNothingConfirmed), "");
        public static ErrorCode IdentityMobileEmailLoginNameAllNull { get; } = new ErrorCode(nameof(IdentityMobileEmailLoginNameAllNull), "");
        public static ErrorCode IdentityAlreadyTaken { get; } = new ErrorCode(nameof(IdentityAlreadyTaken), "");
        public static ErrorCode ServiceRegisterError { get; } = new ErrorCode(nameof(ServiceRegisterError), "");
        public static ErrorCode TryRemoveRoleFromUserError { get; } = new ErrorCode(nameof(TryRemoveRoleFromUserError), "");
        public static ErrorCode AudienceNotFound { get; } = new ErrorCode(nameof(AudienceNotFound), "");
        public static ErrorCode AlreadyHaveRoles { get; } = new ErrorCode(nameof(AlreadyHaveRoles), "用户已经有了一些你要添加的Role");
        public static ErrorCode InnerError { get; } = new ErrorCode(nameof(InnerError), "");
        #endregion
        #region
        public static ErrorCode LoadedLuaNotFound { get; set; } = new ErrorCode(nameof(LoadedLuaNotFound), "");
        public static ErrorCode RedisConnectionFailed { get; set; } = new ErrorCode(nameof(RedisConnectionFailed), "");
        public static ErrorCode KVStoreRedisTimeout { get; set; } = new ErrorCode(nameof(KVStoreRedisTimeout), "");
        public static ErrorCode KVStoreError { get; set; } = new ErrorCode(nameof(KVStoreError), "");
        public static ErrorCode NoSuchInstance { get; set; } = new ErrorCode(nameof(NoSuchInstance), "");
        public static ErrorCode KVStoreExistAlready { get; set; } = new ErrorCode(nameof(KVStoreExistAlready), "");
        public static ErrorCode KVStoreTimestampNotMatched { get; set; } = new ErrorCode(nameof(KVStoreTimestampNotMatched), "");
        public static ErrorCode NoModelSchemaFound { get; set; } = new ErrorCode(nameof(NoModelSchemaFound), "");
        public static ErrorCode LackKVStoreKeyAttributeError { get; set; } = new ErrorCode(nameof(LackKVStoreKeyAttributeError), "");
        public static ErrorCode VersionsKeysNotEqualError { get; set; } = new ErrorCode(nameof(VersionsKeysNotEqualError), "");
        public static ErrorCode UnKown { get; set; } = new ErrorCode(nameof(UnKown), "");
        #endregion
        #region
        public static ErrorCode DistributedLockUnLockFailed { get; set; } = new ErrorCode(nameof(DistributedLockUnLockFailed), "");
        public static ErrorCode MemoryLockError { get; set; } = new ErrorCode(nameof(MemoryLockError), "");
        #endregion
        #region
        public static ErrorCode CapthaError { get; set; } = new ErrorCode(nameof(CapthaError), "");
        #endregion
        #region
        public static ErrorCode DataProtectionCertNotFound { get; set; } = new ErrorCode(nameof(DataProtectionCertNotFound), "");
        public static ErrorCode JwtEncryptionCertNotFound { get; set; } = new ErrorCode(nameof(JwtEncryptionCertNotFound), "");
        public static ErrorCode StartupError { get; set; } = new ErrorCode(nameof(StartupError), "");
        public static ErrorCode DatabaseInitLockError { get; set; } = new ErrorCode(nameof(DatabaseInitLockError), "");

        //public static ErrorCode ExceptionHandlerPathFeatureNull { get; } = new ErrorCode(nameof(ExceptionHandlerPathFeatureNull), "");

        //public static ErrorCode ServerUnKownNonErrorCodeError { get; } = new ErrorCode(nameof(ServerUnKownNonErrorCodeError), "");

        public static ErrorCode ServerInternalError { get; } = new ErrorCode(nameof(ServerInternalError), "服务器内部运行错误");

        public static ErrorCode GlobalExceptionError { get; } = new ErrorCode(nameof(GlobalExceptionError), "");
        public static ErrorCode UploadError { get; } = new ErrorCode(nameof(UploadError), "");
        #endregion
        #region
        public static ErrorCode NoInternet { get; } = new ErrorCode(nameof(NoInternet), "");

        public static ErrorCode CaptchaErrorReturn { get; } = new ErrorCode(nameof(CaptchaErrorReturn), "Tecent的Captha服务返回不对，查看");
        public static ErrorCode UnSupportedResToModel { get; } = new ErrorCode(nameof(UnSupportedResToModel), "");
        #endregion
        #region

        #endregion
    }
}
