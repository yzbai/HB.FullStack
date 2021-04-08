using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("HB.Infrastructure.Redis.Cache")]
namespace HB.FullStack.Cache
{
    /// <summary>
    /// from 2000~2999
    /// </summary>
    internal static class CacheErrorCodes
    {
        public static ErrorCode SlidingTimeBiggerThanMaxAlive { get; set; } = new ErrorCode(2000, nameof(SlidingTimeBiggerThanMaxAlive), "");
        public static ErrorCode EntityNotHaveKeyAttribute { get; set; } = new ErrorCode(2001, nameof(EntityNotHaveKeyAttribute), "");
        public static ErrorCode ConvertError { get; set; } = new ErrorCode(2002, nameof(ConvertError), "");
        public static ErrorCode CacheLoadedLuaNotFound { get; set; } = new ErrorCode(2003, nameof(CacheLoadedLuaNotFound), "");
        public static ErrorCode CacheInstanceNotFound { get; set; } = new ErrorCode(2004, nameof(CacheInstanceNotFound), "");
        public static ErrorCode NoSuchDimensionKey { get; set; } = new ErrorCode(2005, nameof(NoSuchDimensionKey), "");
        public static ErrorCode NotEnabledForEntity { get; set; } = new ErrorCode(2006, nameof(NotEnabledForEntity), "");
        public static ErrorCode Unkown { get; set; } = new ErrorCode(2007, nameof(Unkown), "");
        public static ErrorCode NotACacheEntity { get; set; } = new ErrorCode(2008, nameof(NotACacheEntity), "");
        public static ErrorCode UnkownButDeleted { get; set; } = new ErrorCode(2009, nameof(UnkownButDeleted), "");

    }

    internal static class Exceptions
    {
        internal static Exception CacheSlidingTimeBiggerThanMaxAlive(string type)
        {
            CacheException exception = new CacheException(CacheErrorCodes.SlidingTimeBiggerThanMaxAlive);

            exception.Data["Type"] = type;

            return exception;
        }

        internal static Exception CacheEntityNotHaveKeyAttribute(string type)
        {
            CacheException exception = new CacheException(CacheErrorCodes.EntityNotHaveKeyAttribute);

            exception.Data["Type"] = type;

            return exception;
        }

        internal static Exception ConvertError(string key, Exception innerException)
        {
            CacheException exception = new CacheException(CacheErrorCodes.ConvertError, innerException);

            exception.Data["Key"] = key;

            return exception;
        }

        internal static Exception Unkown(object key, object? value, Exception innerException)
        {
            CacheException exception = new CacheException(CacheErrorCodes.Unkown, innerException);

            exception.Data["Key"] = key;
            exception.Data["Value"] = value;

            return exception;
        }

        internal static Exception UnkownButDeleted(string cause, Exception innerException)
        {
            throw new NotImplementedException();
        }

        internal static Exception CacheLoadedLuaNotFound(string instanceName)
        {
            throw new NotImplementedException();
        }

        internal static Exception InstanceNotFound(string instanceName)
        {
            throw new NotImplementedException();
        }

        internal static Exception NoSuchDimensionKey(string type, string dimensionKeyName)
        {
            throw new NotImplementedException();
        }

        internal static Exception NotEnabledForEntity(string type)
        {
            throw new NotImplementedException();
        }
    }
}
