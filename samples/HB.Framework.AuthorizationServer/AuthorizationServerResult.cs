using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.AuthorizationServer
{
    public enum AuthorizationServerResultStatus
    {
        Succeeded,
        NotFound,
        Failed
    }

    public class AuthorizationServerResult
    {
        public static readonly AuthorizationServerResult Succeeded = new AuthorizationServerResult() { Status = AuthorizationServerResultStatus.Succeeded };
        public static readonly AuthorizationServerResult NotFound = new AuthorizationServerResult() { Status = AuthorizationServerResultStatus.NotFound };
        public static readonly AuthorizationServerResult Failed = new AuthorizationServerResult() { Status = AuthorizationServerResultStatus.Failed };

        public AuthorizationServerResult() { }

        public AuthorizationServerResult(DatabaseResult dbResult)
        {
            switch (dbResult.Status)
            {
                case DatabaseResultStatus.Failed:
                    Status = AuthorizationServerResultStatus.Failed;
                    break;
                case DatabaseResultStatus.NotFound:
                    Status = AuthorizationServerResultStatus.NotFound;
                    break;
                case DatabaseResultStatus.NotWriteable:
                    Status = AuthorizationServerResultStatus.Failed;
                    break;
                case DatabaseResultStatus.Succeeded:
                    Status = AuthorizationServerResultStatus.Succeeded;
                    break;
                default:
                    Status = AuthorizationServerResultStatus.Failed;
                    break;
            }
        }

        public AuthorizationServerResultStatus Status { get; private set; }

        public static bool operator ==(AuthorizationServerResult left, AuthorizationServerResult right)
        {
            if (System.Object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }

            return left.Status == right.Status;
        }

        public static bool operator !=(AuthorizationServerResult left, AuthorizationServerResult right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            AuthorizationServerResult result = obj as AuthorizationServerResult;

            return this == result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public static class DatabaseResultExtensions
    {
        public static AuthorizationServerResult ToAuthorizationResult(this DatabaseResult dbResult)
        {
            return new AuthorizationServerResult(dbResult);
        }
    }
}
