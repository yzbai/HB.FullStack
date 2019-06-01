using HB.Component.Identity.Entity;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HB.Component.Identity
{
    public enum IdentityResultStatus
    {
        NotFound,
        AlreadyExists,
        Succeeded,
        ArgumentError,
        Failed,
        MobileAlreadyTaken,
        UserNameAlreadyTaken,
        EmailAlreadyTaken,
        ExceptionThrown
    }

    public class IdentityResult
    {
        //public static readonly IdentityResult NotFound = new IdentityResult() { Status = IdentityResultStatus.NotFound };
        //public static readonly IdentityResult AlreadyExists = new IdentityResult() { Status = IdentityResultStatus.AlreadyExists };
        //public static readonly IdentityResult Succeeded = new IdentityResult() { Status = IdentityResultStatus.Succeeded };
        //public static readonly IdentityResult ArgumentError = new IdentityResult() { Status = IdentityResultStatus.ArgumentError };
        //public static readonly IdentityResult Failed = new IdentityResult() { Status = IdentityResultStatus.Failed };
        //public static readonly IdentityResult MobileAlreadyTaken = new IdentityResult() { Status = IdentityResultStatus.MobileAlreadyTaken };
        //public static readonly IdentityResult UserNameAlreadyTaken = new IdentityResult() { Status = IdentityResultStatus.UserNameAlreadyTaken };
        //public static readonly IdentityResult EmailAlreadyTaken = new IdentityResult() { Status = IdentityResultStatus.EmailAlreadyTaken };

        public IdentityResult() { }

        public IdentityResult(DatabaseResult dbResult)
        {
            switch (dbResult.Status)
            {
                case DatabaseResultStatus.Succeeded:
                    Status = IdentityResultStatus.Succeeded;
                    break;
                case DatabaseResultStatus.NotFound:
                    Status = IdentityResultStatus.NotFound;
                    break;
                case DatabaseResultStatus.Failed:
                    Status = IdentityResultStatus.Failed;
                    break;
                case DatabaseResultStatus.NotWriteable:
                    Status = IdentityResultStatus.Failed;
                    break;
                default:
                    Status = IdentityResultStatus.Failed;
                    break;
            }
        }

        

        public IdentityResultStatus Status { get; private set; }
        
        public User User { get; set; }

        public bool IsSucceeded()
        {
            return Status == IdentityResultStatus.Succeeded;
        }
        public static IdentityResult Succeeded()
        {
            return new IdentityResult { Status = IdentityResultStatus.Succeeded };
        }

        public static IdentityResult UserNameAlreadyTaken()
        {
            return new IdentityResult { Status = IdentityResultStatus.UserNameAlreadyTaken };
        }

        public static IdentityResult ArgumentError()
        {
            return new IdentityResult { Status = IdentityResultStatus.ArgumentError };
        }

        public static IdentityResult Failed()
        {
            return new IdentityResult { Status = IdentityResultStatus.Failed };
        }

        public static IdentityResult MobileAlreadyTaken()
        {
            return new IdentityResult { Status = IdentityResultStatus.MobileAlreadyTaken };
        }

        public static IdentityResult NotFound()
        {
            return new IdentityResult { Status = IdentityResultStatus.NotFound };
        }

        public static IdentityResult Throwed()
        {
            return new IdentityResult { Status = IdentityResultStatus.ExceptionThrown };
        }

        public static IdentityResult AlreadyExists()
        {
            return new IdentityResult { Status = IdentityResultStatus.AlreadyExists };
        }

        //public static bool operator ==(IdentityResult left, IdentityResult right)
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

        //public static bool operator !=(IdentityResult left, IdentityResult right)
        //{
        //    return !(left == right);
        //}

        //public override bool Equals(object obj)
        //{
        //    IdentityResult result = obj as IdentityResult;

        //    return this == result;
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}

    }

    public static class DatabaseResultExtensions
    {
        public static IdentityResult ToIdentityResult(this DatabaseResult dbResult)
        {
            return new IdentityResult(dbResult);
        }
    }
}