using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.KVStore
{
    public enum KVStoreResultStatus
    {
        Succeeded,
        NotFound,
        Failed,
        ExistAlready,
        VersionNotMatched
    }

    public class KVStoreResult
    {
        public Exception Exception { get; private set; }
        public KVStoreResultStatus Status { get; private set; }
        
        public static KVStoreResult Fail(Exception exception)
        {
            KVStoreResult result = new KVStoreResult
            {
                Status = KVStoreResultStatus.Failed
            };

            if (exception != null)
            {
                result.Exception = exception;
            }

            return result;
        }

        public static KVStoreResult Fail(string message)
        {
            return Fail(new Exception(message));
        }

        public bool IsSucceeded()
        {
            return Status == KVStoreResultStatus.Succeeded;
        }

        public static KVStoreResult ExistAlready()
        {
            return new KVStoreResult { Status = KVStoreResultStatus.ExistAlready };
        }

        public static KVStoreResult VersionNotMatched()
        {
            return new KVStoreResult { Status = KVStoreResultStatus.VersionNotMatched };
        }

        public static KVStoreResult Failed()
        {
            return new KVStoreResult { Status = KVStoreResultStatus.Failed };
        }

        public static KVStoreResult Succeeded()
        {
            return new KVStoreResult { Status = KVStoreResultStatus.Succeeded };
        }

    }
}
