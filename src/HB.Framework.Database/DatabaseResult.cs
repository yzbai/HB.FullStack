using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HB.Framework.Database
{
    public enum DatabaseResultStatus
    {
        Succeeded,
        NotFound,
        Failed,
        NotWriteable
    }

    public class DatabaseResult
    {
        public Exception Exception { get; private set; }

        public DatabaseResultStatus Status { get; private set; }

        public IList<long> Ids { get; private set; } = new List<long>();

        public void AddId(long id)
        {
            Ids.Add(id);
        }

        public bool IsSucceeded()
        {
            return Status == DatabaseResultStatus.Succeeded;
        }

        public static DatabaseResult Fail(Exception exception)
        {
            DatabaseResult result = new DatabaseResult
            {
                Status = DatabaseResultStatus.Failed
            };

            if (exception != null)
            {
                result.Exception = exception;
            }

            return result;
        }

        public static DatabaseResult Fail(string message)
        {
            return Fail(new Exception(message));
        }

        public static DatabaseResult NotWriteable()
        {
            return new DatabaseResult { Status = DatabaseResultStatus.NotWriteable };
        }

        public static DatabaseResult Succeeded()
        {
            return new DatabaseResult { Status = DatabaseResultStatus.Succeeded };
        }

        public static DatabaseResult NotFound()
        {
            return new DatabaseResult { Status = DatabaseResultStatus.NotFound };
        }

        public static DatabaseResult Failed()
        {
            return new DatabaseResult { Status = DatabaseResultStatus.Failed };
        }

        //public static readonly DatabaseResult Succeeded = new DatabaseResult() { Status = DatabaseResultStatus.Succeeded };

        //public static readonly DatabaseResult Failed = new DatabaseResult() { Status = DatabaseResultStatus.Failed };

        //public static readonly DatabaseResult NotFound = new DatabaseResult() { Status = DatabaseResultStatus.NotFound };

        //public static readonly DatabaseResult NotWriteable = new DatabaseResult() { Status = DatabaseResultStatus.NotWriteable };

        //public static bool operator ==(DatabaseResult left, DatabaseResult right)
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

        //public static bool operator !=(DatabaseResult left, DatabaseResult right)
        //{
        //    return !(left == right);
        //}

        //public override bool Equals(object obj)
        //{
        //    DatabaseResult result = obj as DatabaseResult;

        //    return this == result;
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
    }
}
