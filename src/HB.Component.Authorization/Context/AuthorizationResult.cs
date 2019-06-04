using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Authorization
{
    public enum AuthorizationResultStatus
    {
        Succeeded,
        NotFound,
        Failed,
        ArgumentError,
        ExceptionThrown
    }

    public class AuthorizationResult
    {
        //public static readonly AuthorizationServerResult Succeeded = new AuthorizationServerResult() { Status = AuthorizationServerResultStatus.Succeeded };
        //public static readonly AuthorizationServerResult NotFound = new AuthorizationServerResult() { Status = AuthorizationServerResultStatus.NotFound };
        //public static readonly AuthorizationServerResult Failed = new AuthorizationServerResult() { Status = AuthorizationServerResultStatus.Failed };

        public AuthorizationResult() { }

        public AuthorizationResult(DatabaseResult dbResult)
        {
            switch (dbResult.Status)
            {
                case DatabaseResultStatus.Failed:
                    Status = AuthorizationResultStatus.Failed;
                    break;
                case DatabaseResultStatus.NotFound:
                    Status = AuthorizationResultStatus.NotFound;
                    break;
                case DatabaseResultStatus.NotWriteable:
                    Status = AuthorizationResultStatus.Failed;
                    break;
                case DatabaseResultStatus.Succeeded:
                    Status = AuthorizationResultStatus.Succeeded;
                    break;
                default:
                    Status = AuthorizationResultStatus.Failed;
                    break;
            }
        }

        public AuthorizationResultStatus Status { get; set; }

        public bool IsSucceeded()
        {
            return Status == AuthorizationResultStatus.Succeeded;
        }

        public static AuthorizationResult ArgumentError()
        {
            return new AuthorizationResult { Status = AuthorizationResultStatus.ArgumentError };
        }

        public static AuthorizationResult Succeeded()
        {
            return new AuthorizationResult { Status = AuthorizationResultStatus.Succeeded };
        }

        public static AuthorizationResult Throwed()
        {
            return new AuthorizationResult { Status = AuthorizationResultStatus.ExceptionThrown };
        }

        //public static bool operator ==(AuthorizationServerResult left, AuthorizationServerResult right)
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

        //public static bool operator !=(AuthorizationServerResult left, AuthorizationServerResult right)
        //{
        //    return !(left == right);
        //}

        //public override bool Equals(object obj)
        //{
        //    AuthorizationServerResult result = obj as AuthorizationServerResult;

        //    return this == result;
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
    }

    public static class DatabaseResultExtensions
    {
        public static AuthorizationResult ToAuthorizationResult(this DatabaseResult dbResult)
        {
            return new AuthorizationResult(dbResult);
        }
    }
}
