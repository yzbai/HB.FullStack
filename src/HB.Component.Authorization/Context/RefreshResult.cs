using HB.Component.Identity.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Authorization.Abstractions
{
    public enum RefreshResultStatus
    {
        Succeeded,
        TooFrequent,
        InvalideAccessToken,
        InvalideUserGuid,
        NoTokenInStore,
        UserSecurityStampChanged,
        UpdateSignInTokenError,
        ArgumentError,
        ExceptionThrown
    }

    public class RefreshResult
    {
        //public static readonly RefreshResult Success = new RefreshResult() { Status = RefreshResultStatus.Success };
        //public static readonly RefreshResult TooFrequent = new RefreshResult() { Status = RefreshResultStatus.TooFrequent };
        //public static readonly RefreshResult InvalideAccessToken = new RefreshResult() { Status = RefreshResultStatus.InvalideAccessToken };
        //public static readonly RefreshResult InvalideUserId = new RefreshResult() { Status = RefreshResultStatus.InvalideUserId };
        //public static readonly RefreshResult NoTokenInStore = new RefreshResult() { Status = RefreshResultStatus.NoTokenInStore };
        //public static readonly RefreshResult UserSecurityStampChanged = new RefreshResult() { Status = RefreshResultStatus.UserSecurityStampChanged };
        //public static readonly RefreshResult UpdateSignInTokenError = new RefreshResult() { Status = RefreshResultStatus.UpdateSignInTokenError };

        public RefreshResultStatus Status { get; set; }

        public string AccessToken { get; set; }

        public bool IsSucceeded()
        {
            return Status == RefreshResultStatus.Succeeded;
        }

        public static RefreshResult TooFrequent()
        {
            return new RefreshResult { Status = RefreshResultStatus.TooFrequent };
        }

        public static RefreshResult InvalideAccessToken()
        {
            return new RefreshResult { Status = RefreshResultStatus.InvalideAccessToken };
        }

        public static RefreshResult InvalideUserGuid()
        {
            return new RefreshResult { Status = RefreshResultStatus.InvalideUserGuid };
        }

        public static RefreshResult NoTokenInStore()
        {
            return new RefreshResult { Status = RefreshResultStatus.NoTokenInStore };
        }

        public static RefreshResult UserSecurityStampChanged()
        {
            return new RefreshResult { Status = RefreshResultStatus.UserSecurityStampChanged };
        }

        public static RefreshResult UpdateSignInTokenError()
        {
            return new RefreshResult { Status = RefreshResultStatus.UpdateSignInTokenError };
        }

        public static RefreshResult ArgumentError()
        {
            return new RefreshResult { Status = RefreshResultStatus.ArgumentError };
        }

        public static RefreshResult Throwed()
        {
            return new RefreshResult { Status = RefreshResultStatus.ExceptionThrown };
        }

        //public static bool operator ==(RefreshResult left, RefreshResult right)
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

        //public static bool operator !=(RefreshResult left, RefreshResult right)
        //{
        //    return !(left == right);
        //}

        //public override bool Equals(object obj)
        //{
        //    RefreshResult result = obj as RefreshResult;

        //    return this == result;
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
    }
}
