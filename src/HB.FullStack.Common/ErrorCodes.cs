namespace System
{
    //TODO: 做成继承，分散在各个库中
    public static partial class ErrorCodes
    {

        public static readonly ErrorCode SecurityCheck = new ErrorCode(nameof(SecurityCheck), "");
        
        public static readonly ErrorCode ModelDefError = new ErrorCode(nameof(ModelDefError), "");

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

        public static ErrorCode CacheGetError = new ErrorCode(nameof(CacheGetError), "");
        public static ErrorCode CacheMissed = new ErrorCode(nameof(CacheMissed), "");
        public static ErrorCode CacheGetEmpty = new ErrorCode(nameof(CacheGetEmpty), "");
        public static ErrorCode CacheLockAcquireFailed = new ErrorCode(nameof(CacheLockAcquireFailed), "");

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

        public static ErrorCode NoAuthority = new ErrorCode(nameof(NoAuthority), "");
        public static ErrorCode AccessTokenExpired = new ErrorCode(nameof(AccessTokenExpired), "");
        public static ErrorCode ModelValidationError = new ErrorCode(nameof(ModelValidationError), "");
        public static ErrorCode ApiNotAvailable = new ErrorCode(nameof(ApiNotAvailable), "");
        public static ErrorCode ApiErrorUnkownFormat = new ErrorCode(nameof(ApiErrorUnkownFormat), "");
        public static ErrorCode InvalidSmsCode = new ErrorCode(nameof(InvalidSmsCode), "短信验证码错误。");
        public static ErrorCode SmsServiceError = new ErrorCode(nameof(SmsServiceError), "");
        public static ErrorCode CommonResourceTokenNeeded = new ErrorCode(nameof(CommonResourceTokenNeeded), "");
        public static ErrorCode CommonResourceTokenError = new ErrorCode(nameof(CommonResourceTokenError), "");
        public static ErrorCode ApiUploadEmptyFile = new ErrorCode(nameof(ApiUploadEmptyFile), "");
        public static ErrorCode ApiUploadOverSize = new ErrorCode(nameof(ApiUploadOverSize), "");
        public static ErrorCode ApiUploadWrongType = new ErrorCode(nameof(ApiUploadWrongType), "");
        public static ErrorCode HttpsRequired = new ErrorCode(nameof(HttpsRequired), "");
        public static ErrorCode FromExceptionController = new ErrorCode(nameof(FromExceptionController), "");
        public static ErrorCode ApiCapthaError = new ErrorCode(nameof(ApiCapthaError), "");
        public static ErrorCode ApiUploadFailed = new ErrorCode(nameof(ApiUploadFailed), "");
        public static ErrorCode ServerUnKownError = new ErrorCode(nameof(ServerUnKownError), "");
        public static ErrorCode ClientError = new ErrorCode(nameof(ClientError), "");
        public static ErrorCode Timeout = new ErrorCode(nameof(Timeout), "");
        public static ErrorCode RequestCanceled = new ErrorCode(nameof(RequestCanceled), "");
        public static ErrorCode DirectoryTokenNotFound = new ErrorCode(nameof(DirectoryTokenNotFound), "");
        public static ErrorCode AliyunOssPutObjectError = new ErrorCode(nameof(AliyunOssPutObjectError), "");
        public static ErrorCode TokenRefreshError = new ErrorCode(nameof(TokenRefreshError), "");
        public static ErrorCode UserActivityFilterError = new ErrorCode(nameof(UserActivityFilterError), "");
        public static ErrorCode FileUpdateRequestCountNotEven = new ErrorCode(nameof(FileUpdateRequestCountNotEven), "");
        public static ErrorCode LackApiResourceAttribute = new ErrorCode(nameof(LackApiResourceAttribute), "");
        public static ErrorCode RequestTimeout = new ErrorCode(nameof(RequestTimeout), "");

        /// <summary>
        /// 这个Request已经用过一次了
        /// </summary>
        public static ErrorCode RequestAlreadyUsed = new ErrorCode(nameof(RequestAlreadyUsed), "");

        /// <summary>
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// </summary>
        public static ErrorCode RequestUnderlyingIssue = new ErrorCode(nameof(RequestUnderlyingIssue), "");

        public static ErrorCode HttpResponseDeserializeError = new ErrorCode(nameof(HttpResponseDeserializeError), "");
        public static ErrorCode ApiClientSendUnkownError = new ErrorCode(nameof(ApiClientSendUnkownError), "");
        public static ErrorCode ApiClientGetStreamUnkownError = new ErrorCode(nameof(ApiClientGetStreamUnkownError), "");
        public static ErrorCode ApiRequestInvalidateError = new ErrorCode(nameof(ApiRequestInvalidateError), "");
        public static ErrorCode ApiRequestSetJwtError = new ErrorCode(nameof(ApiRequestSetJwtError), "");
        public static ErrorCode ApiRequestSetApiKeyError = new ErrorCode(nameof(ApiRequestSetApiKeyError), "");
        public static ErrorCode ApiClientUnkownError = new ErrorCode(nameof(ApiClientUnkownError), "");
        public static ErrorCode ServerNullReturn = new ErrorCode(nameof(ServerNullReturn), "");
        public static ErrorCode ArgumentIdsError = new ErrorCode(nameof(ArgumentIdsError), "");
        public static ErrorCode RequestIntervalFilterError = new ErrorCode(nameof(RequestIntervalFilterError), "");
        public static ErrorCode CapthcaNotFound = new ErrorCode(nameof(CapthcaNotFound), "");
        public static ErrorCode CapthcaError = new ErrorCode(nameof(CapthcaError), "");
        public static ErrorCode NeedOwnerResId = new ErrorCode(nameof(NeedOwnerResId), "");

        public static ErrorCode LackParent1ResIdAttribute = new ErrorCode(nameof(LackParent1ResIdAttribute), "因为制定了Parent1ResName，但缺少Parent1ResIdAttribute");

        public static ErrorCode LackParent2ResIdAttribute = new ErrorCode(nameof(LackParent2ResIdAttribute), "因为制定了Parent2ResName，但缺少Parent2ResIdAttribute");

        #region Identity
        //TODO: 客户端应该针对于这些Authorize类的Error进行相应处理

        public static ErrorCode IdentityDisallowRegisterByLoginName { get; set; } = new ErrorCode(nameof(IdentityDisallowRegisterByLoginName), "");
        public static ErrorCode IdentityUserNotExists = new ErrorCode(nameof(IdentityUserNotExists), "");
        public static ErrorCode AuthorizationPasswordWrong = new ErrorCode(nameof(AuthorizationPasswordWrong), "");
        public static ErrorCode TokenRefreshConcurrentError = new ErrorCode(nameof(TokenRefreshConcurrentError), "同一设备正在Refreshing");
        public static ErrorCode RefreshAccessTokenError = new ErrorCode(nameof(RefreshAccessTokenError), "");
        public static ErrorCode AuthorizationInvalideClientId = new ErrorCode(nameof(AuthorizationInvalideClientId), "");
        public static ErrorCode AuthorizationInvalideUserId = new ErrorCode(nameof(AuthorizationInvalideUserId), "");
        public static ErrorCode AuthorizationUserSecurityStampChanged = new ErrorCode(nameof(AuthorizationUserSecurityStampChanged), "");
        public static ErrorCode AuthorizationRefreshTokenExpired = new ErrorCode(nameof(AuthorizationRefreshTokenExpired), "");
        public static ErrorCode AuthorizationNoTokenInStore = new ErrorCode(nameof(AuthorizationNoTokenInStore), "");
        public static ErrorCode AuthorizationMobileNotConfirmed = new ErrorCode(nameof(AuthorizationMobileNotConfirmed), "");
        public static ErrorCode AuthorizationEmailNotConfirmed = new ErrorCode(nameof(AuthorizationEmailNotConfirmed), "");
        public static ErrorCode AuthorizationLockedOut = new ErrorCode(nameof(AuthorizationLockedOut), "");
        public static ErrorCode AuthorizationOverMaxFailedCount = new ErrorCode(nameof(AuthorizationOverMaxFailedCount), "");
        public static ErrorCode JwtSigningCertNotFound = new ErrorCode(nameof(JwtSigningCertNotFound), "");
        public static ErrorCode ServerReturnError = new ErrorCode(nameof(ServerReturnError), "Server收到了请求，但返回了错误");
        public static ErrorCode ApiModelError = new ErrorCode(nameof(ApiModelError), "ApiRequest等Model出错");
        public static ErrorCode ApiAuthenticationError = new ErrorCode(nameof(ApiAuthenticationError), "ApiClient请求时，授权信息有错或缺少");
        public static ErrorCode ApiResourceError = new ErrorCode(nameof(ApiResourceError), "");

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

        public static ErrorCode CacheCollectionKeyNotSame = new ErrorCode(nameof(CacheCollectionKeyNotSame), "");
        public static ErrorCode CacheKeyNotSet = new ErrorCode(nameof(CacheKeyNotSet), "");
        public static ErrorCode CacheValueNotSet = new ErrorCode(nameof(CacheValueNotSet), "");
        public static ErrorCode CachedItemTimestampNotSet = new ErrorCode(nameof(CachedItemTimestampNotSet), "");
        #endregion
        #region Db

        public static ErrorCode DbAddError { get; set; } = new ErrorCode(nameof(DbAddError), "");
        public static ErrorCode DbConflictMethodError { get; set; } = new ErrorCode(nameof(DbConflictMethodError), "");
        public static ErrorCode DbUpdatePropertiesError { get; set; } = new ErrorCode(nameof(DbUpdatePropertiesError), "");

        public static ErrorCode DbUpdateUsingTimestampError = new ErrorCode(nameof(DbUpdateUsingTimestampError), "");
        public static ErrorCode DbDataTooLong = new ErrorCode(nameof(DbDataTooLong), "");

        public static ErrorCode DuplicateKeyEntry = new ErrorCode(nameof(DuplicateKeyEntry), "");

        public static ErrorCode DbEngineExecuterError = new ErrorCode(nameof(DbEngineExecuterError), "");
        public static ErrorCode DbEngineUnKownExecuterError = new ErrorCode(nameof(DbEngineUnKownExecuterError), "");

        public static ErrorCode UseDateTimeOffsetOnly = new ErrorCode(nameof(UseDateTimeOffsetOnly), "");
        public static ErrorCode ModelError = new ErrorCode(nameof(ModelError), "");
        public static ErrorCode MapperError = new ErrorCode(nameof(MapperError), "");
        public static ErrorCode SqlError = new ErrorCode(nameof(SqlError), "");
        public static ErrorCode DatabaseTableCreateError = new ErrorCode(nameof(DatabaseTableCreateError), "");
        
        //Important
        public static ErrorCode OptionsError = new ErrorCode(nameof(OptionsError), "");

        public static ErrorCode MigrateError = new ErrorCode(nameof(MigrateError), "");
        public static ErrorCode FoundTooMuch = new ErrorCode(nameof(FoundTooMuch), "");
        public static ErrorCode DatabaseNotWriteable = new ErrorCode(nameof(DatabaseNotWriteable), "");
        
        //Important
        public static ErrorCode ConcurrencyConflict = new ErrorCode(nameof(ConcurrencyConflict), "");
        public static ErrorCode TransactionError = new ErrorCode(nameof(TransactionError), "");
        public static ErrorCode SystemInfoError = new ErrorCode(nameof(SystemInfoError), "");
        public static ErrorCode NotSupported = new ErrorCode(nameof(NotSupported), "");
        public static ErrorCode BatchError = new ErrorCode(nameof(BatchError), "");
        public static ErrorCode TypeConverterError = new ErrorCode(nameof(TypeConverterError), "");
        public static ErrorCode EmptyGuid = new ErrorCode(nameof(EmptyGuid), "");
        public static ErrorCode UpdatePropertiesCountShouldBePositive = new ErrorCode(nameof(UpdatePropertiesCountShouldBePositive), "");
        public static ErrorCode LongIdShouldBePositive = new ErrorCode(nameof(LongIdShouldBePositive), "");
        public static ErrorCode PropertyNotFound = new ErrorCode(nameof(PropertyNotFound), "");
        public static ErrorCode NoSuchForeignKey = new ErrorCode(nameof(NoSuchForeignKey), "");

        public static ErrorCode NoSuchProperty = new ErrorCode(nameof(NoSuchProperty), "");
        public static ErrorCode KeyValueNotLongOrGuid = new ErrorCode(nameof(KeyValueNotLongOrGuid), "");

        public static ErrorCode ModelHasNotSupportedPropertyType = new ErrorCode(nameof(ModelHasNotSupportedPropertyType), "");

        public static ErrorCode TimestampError = new ErrorCode(nameof(TimestampError), "");
        public static ErrorCode NotInitializedYet = new ErrorCode(nameof(NotInitializedYet), "");
        public static ErrorCode UpdateVersionError = new ErrorCode(nameof(UpdateVersionError), "");
        #endregion
        #region
        public static ErrorCode NoHandler = new ErrorCode(nameof(NoHandler), "");
        public static ErrorCode HandlerAlreadyExisted = new ErrorCode(nameof(HandlerAlreadyExisted), "");
        public static ErrorCode SettingsError = new ErrorCode(nameof(SettingsError), "");
        #endregion
        #region
        public static ErrorCode NotFound = new ErrorCode(nameof(NotFound), "");
        public static ErrorCode IdentityNothingConfirmed = new ErrorCode(nameof(IdentityNothingConfirmed), "");
        public static ErrorCode IdentityMobileEmailLoginNameAllNull = new ErrorCode(nameof(IdentityMobileEmailLoginNameAllNull), "");
        public static ErrorCode IdentityAlreadyTaken = new ErrorCode(nameof(IdentityAlreadyTaken), "");
        public static ErrorCode ServiceRegisterError = new ErrorCode(nameof(ServiceRegisterError), "");
        public static ErrorCode TryRemoveRoleFromUserError = new ErrorCode(nameof(TryRemoveRoleFromUserError), "");
        public static ErrorCode AudienceNotFound = new ErrorCode(nameof(AudienceNotFound), "");
        public static ErrorCode AlreadyHaveRoles = new ErrorCode(nameof(AlreadyHaveRoles), "用户已经有了一些你要添加的Role");
        public static ErrorCode InnerError = new ErrorCode(nameof(InnerError), "");
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
        

        //public static ErrorCode ExceptionHandlerPathFeatureNull = new ErrorCode(nameof(ExceptionHandlerPathFeatureNull), "");

        //public static ErrorCode ServerUnKownNonErrorCodeError = new ErrorCode(nameof(ServerUnKownNonErrorCodeError), "");

        public static ErrorCode ServerInternalError = new ErrorCode(nameof(ServerInternalError), "服务器内部运行错误");

        public static ErrorCode GlobalExceptionError = new ErrorCode(nameof(GlobalExceptionError), "");
        public static ErrorCode UploadError = new ErrorCode(nameof(UploadError), "");
        #endregion
        #region
        public static ErrorCode NoInternet = new ErrorCode(nameof(NoInternet), "");

        public static ErrorCode CaptchaErrorReturn = new ErrorCode(nameof(CaptchaErrorReturn), "Tecent的Captha服务返回不对，查看");
        public static ErrorCode UnSupportedResToModel = new ErrorCode(nameof(UnSupportedResToModel), "");

        #endregion
        #region
        public static ErrorCode UpdateWithEmptyResource = new ErrorCode(nameof(UpdateWithEmptyResource), "");
        public static ErrorCode OnlyUpdateYours = new ErrorCode(nameof(OnlyUpdateYours), "");
        public static ErrorCode AlreadyHasUserProfile = new ErrorCode(nameof(AlreadyHasUserProfile), "");
        public static ErrorCode ApiRequestNameConditionMatchError = new ErrorCode(nameof(ApiRequestNameConditionMatchError), "");

        public static ErrorCode CppShouldContainAddtionalProperty = new ErrorCode(nameof(CppShouldContainAddtionalProperty), "");

        #endregion
    }
}
