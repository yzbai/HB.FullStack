using HB.Component.Identity;
using HB.Component.Identity.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Component.Authorization.Abstractions
{
    public enum SignInResultStatus
    {
        Success,
        LogoffOtherClientFailed,
        NewUserCreateFailed,
        NewUserCreateFailedMobileAlreadyTaken,
        NewUserCreateFailedEmailAlreadyTaken,
        NewUserCreateFailedUserNameAlreadyTaken,
        LockedOut,
        TwoFactorRequired,
        MobileNotConfirmed,
        EmailNotConfirmed,
        OverMaxFailedCount,
        NoSuchUser,
        PasswordWrong,
        AuthtokenCreatedFailed,
        ArgumentError,
        ExceptionThrown
    }

    public class SignInResult
    {
        //public static readonly SignInResult Success = new SignInResult() { Status = SignInResultStatus.Succeed };
        //public static readonly SignInResult NewUserCreateFailed = new SignInResult() { Status = SignInResultStatus.NewUserCreateFailed };
        //public static readonly SignInResult NewUserCreateFailedMobileAlreadyTaken = new SignInResult() { Status = SignInResultStatus.NewUserCreateFailedMobileAlreadyTaken };
        //public static readonly SignInResult NewUserCreateFailedEmailAlreadyTaken = new SignInResult() { Status = SignInResultStatus.NewUserCreateFailedEmailAlreadyTaken };
        //public static readonly SignInResult NewUserCreateFailedUserNameAlreadyTaken = new SignInResult() { Status = SignInResultStatus.NewUserCreateFailedUserNameAlreadyTaken };
        //public static readonly SignInResult LockedOut = new SignInResult() { Status = SignInResultStatus.LockedOut };
        //public static readonly SignInResult TwoFactorRequired = new SignInResult() { Status = SignInResultStatus.TwoFactorRequired };
        //public static readonly SignInResult MobileNotConfirmed = new SignInResult() { Status = SignInResultStatus.MobileNotConfirmed };
        //public static readonly SignInResult EmailNotConfirmed = new SignInResult() { Status = SignInResultStatus.EmailNotConfirmed };
        //public static readonly SignInResult OverMaxFailedCount = new SignInResult() { Status = SignInResultStatus.OverMaxFailedCount };
        //public static readonly SignInResult NoSuchUser = new SignInResult() { Status = SignInResultStatus.NoSuchUser };
        //public static readonly SignInResult PasswordWrong = new SignInResult() { Status = SignInResultStatus.PasswordWrong };
        //public static readonly SignInResult AuthtokenCreatedFailed = new SignInResult() { Status = SignInResultStatus.AuthtokenCreatedFailed };
        //public static readonly SignInResult ArgumentError = new SignInResult() { Status = SignInResultStatus.ArgumentError };

        public SignInResultStatus Status { get; set; }
        public User CurrentUser { get; set; }
        public bool NewUserCreated { get; set; }
        
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }

        public bool IsSucceeded()
        {
            return Status == SignInResultStatus.Success;
        }
        public static SignInResult Throwed()
        {
            return new SignInResult { Status = SignInResultStatus.ExceptionThrown };
        }

        public static SignInResult ArgumentError()
        {
            return new SignInResult { Status = SignInResultStatus.ArgumentError };
        }

        public static SignInResult NewUserCreateFailed()
        {
            return new SignInResult { Status = SignInResultStatus.NewUserCreateFailed };
        }

        public static SignInResult NewUserCreateFailedEmailAlreadyTaken()
        {
            return new SignInResult { Status = SignInResultStatus.NewUserCreateFailedEmailAlreadyTaken };
        }

        public static SignInResult NewUserCreateFailedMobileAlreadyTaken()
        {
            return new SignInResult { Status = SignInResultStatus.NewUserCreateFailedMobileAlreadyTaken };
        }

        public static SignInResult NewUserCreateFailedUserNameAlreadyTaken()
        {
            return new SignInResult { Status = SignInResultStatus.NewUserCreateFailedUserNameAlreadyTaken };
        }

        public static SignInResult NoSuchUser()
        {
            return new SignInResult { Status = SignInResultStatus.NoSuchUser };
        }

        public static SignInResult PasswordWrong()
        {
            return new SignInResult { Status = SignInResultStatus.PasswordWrong };
        }

        public static SignInResult AuthtokenCreatedFailed()
        {
            return new SignInResult { Status = SignInResultStatus.AuthtokenCreatedFailed };
        }

        public static SignInResult MobileNotConfirmed()
        {
            return new SignInResult { Status = SignInResultStatus.MobileNotConfirmed };
        }

        public static SignInResult EmailNotConfirmed()
        {
            return new SignInResult { Status = SignInResultStatus.EmailNotConfirmed };
        }

        public static SignInResult LockedOut()
        {
            return new SignInResult { Status = SignInResultStatus.LockedOut };
        }

        public static SignInResult OverMaxFailedCount()
        {
            return new SignInResult { Status = SignInResultStatus.OverMaxFailedCount };
        }

        public static SignInResult Succeeded()
        {
            return new SignInResult { Status = SignInResultStatus.Success };
        }

        public static SignInResult LogoffOtherClientFailed()
        {
            return new SignInResult { Status = SignInResultStatus.LogoffOtherClientFailed };
        }

        //public static bool operator ==(SignInResult left, SignInResult right)
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

        //public static bool operator !=(SignInResult left, SignInResult right)
        //{
        //    return !(left == right);
        //}

        //public override bool Equals(object obj)
        //{
        //    SignInResult result = obj as SignInResult;

        //    return this == result;
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
    }
}