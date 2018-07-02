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

        //public static readonly KVStoreResult Succeeded = new KVStoreResult() { Status = KVStoreResultStatus.Succeeded };
        //public static readonly KVStoreResult NotFound = new KVStoreResult() { Status = KVStoreResultStatus.NotFound };
        //public static readonly KVStoreResult Failed = new KVStoreResult() { Status = KVStoreResultStatus.Failed };
        //public static readonly KVStoreResult ExistAlready = new KVStoreResult() { Status = KVStoreResultStatus.ExistAlready };
        //public static readonly KVStoreResult VersionNotMatched = new KVStoreResult() { Status = KVStoreResultStatus.VersionNotMatched };


        //public static bool operator ==(KVStoreResult left, KVStoreResult right)
        //{
        //    if (System.Object.ReferenceEquals(left, right))
        //    {
        //        return true;
        //    }

        //    if (((object)left == null) || ((object)right == null))
        //    {
        //        return false;
        //    }

        //    return left.Status == right.Status;
        //}

        //public static bool operator !=(KVStoreResult left, KVStoreResult right)
        //{
        //    return !(left == right);
        //}

        //public override bool Equals(object obj)
        //{
        //    if (obj == null)
        //    {

        //    }

        //    KVStoreResult result = obj as KVStoreResult;

        //    return this == result;
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
    }
}
