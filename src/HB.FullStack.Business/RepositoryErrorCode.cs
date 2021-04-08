namespace System
{
    /// <summary>
    /// from 7000 - 7999
    /// </summary>
    internal static class RepositoryErrorCodes
    {
        public static ErrorCode CacheKeyNotSet { get; set; } = new ErrorCode(7000, nameof(CacheKeyNotSet), "");
        public static ErrorCode CacheValueNotSet { get; set; } = new ErrorCode(7001, nameof(CacheValueNotSet), "");
        public static ErrorCode UtcTicksNotSet { get; set; } = new ErrorCode(7002, nameof(UtcTicksNotSet), "");
    }

    internal static class Exceptions
    {
        internal static Exception UtcTicksNotSet(string resourceType, string cacheKey, object? cacheValue)
        {
            RepositoryException exception = new RepositoryException(RepositoryErrorCodes.UtcTicksNotSet);

            exception.Data["ResourceType"] = resourceType;
            exception.Data["CacheKey"] = cacheKey;
            exception.Data["CacheValue"] = cacheValue;

            return exception;
        }

        internal static Exception CacheValueNotSet(string resourceType, string cacheKey)
        {
            RepositoryException exception = new RepositoryException(RepositoryErrorCodes.CacheValueNotSet);

            exception.Data["ResourceType"] = resourceType;
            exception.Data["CacheKey"] = cacheKey;

            return exception;
        }

        internal static Exception CacheKeyNotSet(string resourceType)
        {
            RepositoryException exception = new RepositoryException(RepositoryErrorCodes.CacheKeyNotSet);

            exception.Data["ResourceType"] = resourceType;

            return exception;

        }
    }
}