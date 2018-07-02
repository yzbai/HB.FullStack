using HB.Framework.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.AuthorizationServer.Abstractions
{
    public enum SignInResultStatus
    {
        Success,
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
        ArgumentError
    }

    public class SignInResult
    {
        public static readonly SignInResult Success = new SignInResult() { Status = SignInResultStatus.Success };
        public static readonly SignInResult NewUserCreateFailed = new SignInResult() { Status = SignInResultStatus.NewUserCreateFailed };
        public static readonly SignInResult NewUserCreateFailedMobileAlreadyTaken = new SignInResult() { Status = SignInResultStatus.NewUserCreateFailedMobileAlreadyTaken };
        public static readonly SignInResult NewUserCreateFailedEmailAlreadyTaken = new SignInResult() { Status = SignInResultStatus.NewUserCreateFailedEmailAlreadyTaken };
        public static readonly SignInResult NewUserCreateFailedUserNameAlreadyTaken = new SignInResult() { Status = SignInResultStatus.NewUserCreateFailedUserNameAlreadyTaken };
        public static readonly SignInResult LockedOut = new SignInResult() { Status = SignInResultStatus.LockedOut };
        public static readonly SignInResult TwoFactorRequired = new SignInResult() { Status = SignInResultStatus.TwoFactorRequired };
        public static readonly SignInResult MobileNotConfirmed = new SignInResult() { Status = SignInResultStatus.MobileNotConfirmed };
        public static readonly SignInResult EmailNotConfirmed = new SignInResult() { Status = SignInResultStatus.EmailNotConfirmed };
        public static readonly SignInResult OverMaxFailedCount = new SignInResult() { Status = SignInResultStatus.OverMaxFailedCount };
        public static readonly SignInResult NoSuchUser = new SignInResult() { Status = SignInResultStatus.NoSuchUser };
        public static readonly SignInResult PasswordWrong = new SignInResult() { Status = SignInResultStatus.PasswordWrong };
        public static readonly SignInResult AuthtokenCreatedFailed = new SignInResult() { Status = SignInResultStatus.AuthtokenCreatedFailed };
        public static readonly SignInResult ArgumentError = new SignInResult() { Status = SignInResultStatus.ArgumentError };

        public SignInResultStatus Status { get; private set; }
        public User CurrentUser { get; set; }
        public bool NewUserCreated { get; set; }
        
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }

        public static bool operator ==(SignInResult left, SignInResult right)
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

        public static bool operator !=(SignInResult left, SignInResult right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            SignInResult result = obj as SignInResult;

            return this == result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}