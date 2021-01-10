namespace HB.FullStack.Identity
{
    public enum IdentityErrorCode
    {
        AuthorizationNotFound,
        AuthorizationPasswordWrong,
        AuthorizationTooFrequent,
        AuthorizationInvalideAccessToken,
        AuthorizationInvalideDeviceId,
        AuthorizationInvalideUserId,
        AuthorizationUserSecurityStampChanged,
        AuthorizationRefreshTokenExpired,
        AuthorizationNoTokenInStore,
        AuthorizationMobileNotConfirmed,
        AuthorizationEmailNotConfirmed,
        AuthorizationLockedOut,
        AuthorizationOverMaxFailedCount,
        JwtSigningCertNotFound,
        JwtEncryptionCertNotFound,
        FoundTooMuch,
        NotFound,
        IdentityNothingConfirmed,
        IdentityMobileEmailLoginNameAllNull,
        IdentityAlreadyTaken,
        ServiceRegisterError
    }
}