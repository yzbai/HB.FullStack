using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class CacheErrorCodes
    {
        public static ErrorCode SlidingTimeBiggerThanMaxAlive { get;  } = new ErrorCode(ErrorCodeStartIds.CACHE + 0, nameof(SlidingTimeBiggerThanMaxAlive), "");
        public static ErrorCode EntityNotHaveKeyAttribute { get;  } = new ErrorCode(ErrorCodeStartIds.CACHE + 1, nameof(EntityNotHaveKeyAttribute), "");
        public static ErrorCode ConvertError { get;  } = new ErrorCode(ErrorCodeStartIds.CACHE + 2, nameof(ConvertError), "");
        public static ErrorCode CacheLoadedLuaNotFound { get;  } = new ErrorCode(ErrorCodeStartIds.CACHE + 3, nameof(CacheLoadedLuaNotFound), "");
        public static ErrorCode CacheInstanceNotFound { get;  } = new ErrorCode(ErrorCodeStartIds.CACHE + 4, nameof(CacheInstanceNotFound), "");
        public static ErrorCode NoSuchDimensionKey { get;  } = new ErrorCode(ErrorCodeStartIds.CACHE + 5, nameof(NoSuchDimensionKey), "");
        public static ErrorCode NotEnabledForEntity { get;  } = new ErrorCode(ErrorCodeStartIds.CACHE + 6, nameof(NotEnabledForEntity), "");
        public static ErrorCode Unkown { get;  } = new ErrorCode(ErrorCodeStartIds.CACHE + 7, nameof(Unkown), "");
        public static ErrorCode NotACacheEntity { get;  } = new ErrorCode(ErrorCodeStartIds.CACHE + 8, nameof(NotACacheEntity), "");
        public static ErrorCode UnkownButDeleted { get;  } = new ErrorCode(ErrorCodeStartIds.CACHE + 9, nameof(UnkownButDeleted), "");

    }

    public static class CacheExceptions
    {
        public static CacheException CacheSlidingTimeBiggerThanMaxAlive(string type)
        {
            CacheException exception = new CacheException(CacheErrorCodes.SlidingTimeBiggerThanMaxAlive);

            exception.Data["Type"] = type;

            return exception;
        }

        public static CacheException CacheEntityNotHaveKeyAttribute(string type)
        {
            CacheException exception = new CacheException(CacheErrorCodes.EntityNotHaveKeyAttribute);

            exception.Data["Type"] = type;

            return exception;
        }

        public static CacheException ConvertError(string key, Exception innerException)
        {
            CacheException exception = new CacheException(CacheErrorCodes.ConvertError, innerException);

            exception.Data["Key"] = key;

            return exception;
        }

        public static CacheException Unkown(object key, object? value, Exception innerException)
        {
            CacheException exception = new CacheException(CacheErrorCodes.Unkown, innerException);

            exception.Data["Key"] = key;
            exception.Data["Value"] = value;

            return exception;
        }

        public static CacheException UnkownButDeleted(string cause, Exception innerException)
        {
            throw new NotImplementedException();
        }

        public static CacheException CacheLoadedLuaNotFound(string instanceName)
        {
            throw new NotImplementedException();
        }

        public static CacheException InstanceNotFound(string instanceName)
        {
            throw new NotImplementedException();
        }

        public static CacheException NoSuchDimensionKey(string type, string dimensionKeyName)
        {
            throw new NotImplementedException();
        }

        public static CacheException NotEnabledForEntity(string type)
        {
            throw new NotImplementedException();
        }
    }
}
