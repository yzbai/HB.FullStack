namespace HB.FullStack.Server
{
    public enum ServerErrorCode
    {
        DataProtectionCertNotFound,
        JwtEncryptionCertNotFound,
        StartupError,
        DatabaseInitLockError
    }
}