using HB.FullStack.Mobile.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.FullStack.Mobile
{
    //public enum ClientState
    //{
    //    NotLogined,
    //    NewVersionAndLogined,
    //    OldVersionAndLogined
    //}

    //TODO: 考虑SecurityStorage不支持时，改用普通的Storage
    public static class UserPreferences
    {
        private const string UserId_Preference_Name = "wjUfoxCi";
        private const string UserCreateTime_Preference_Name = "WMIliRIP";
        private const string Mobile_Preference_Name = "H8YA3d5aj";
        private const string Email_Preference_Name = "B2JG5UN5f";
        private const string LoginName_Preference_Name = "UwsSmhY1";
        private const string AccessToken_Preference_Name = "D3SQAAtrv";
        private const string RefreshToken_Preference_Name = "ZTpMCJQl";

        private static long? _userId;
        private static DateTimeOffset? _userCreateTime;
        private static string? _mobile;
        private static string? _loginName;
        private static string? _email;
        private static string? _accessToken;
        private static string? _refreshToken;

        static UserPreferences()
        {
            //Loading
            string? storedValue = PreferenceHelper.PreferenceGetAsync(UserId_Preference_Name).Result;
            _userId = storedValue == null ? null : Convert.ToInt64(storedValue, CultureInfo.InvariantCulture);

            storedValue = PreferenceHelper.PreferenceGetAsync(UserCreateTime_Preference_Name).Result;
            _userCreateTime = storedValue == null ? null : DateTimeOffset.Parse(storedValue, CultureInfo.InvariantCulture);

            _mobile = PreferenceHelper.PreferenceGetAsync(Mobile_Preference_Name).Result;
            _loginName = PreferenceHelper.PreferenceGetAsync(LoginName_Preference_Name).Result;
            _email = PreferenceHelper.PreferenceGetAsync(Email_Preference_Name).Result;
            _accessToken = PreferenceHelper.PreferenceGetAsync(AccessToken_Preference_Name).Result;
            _refreshToken = PreferenceHelper.PreferenceGetAsync(RefreshToken_Preference_Name).Result;
        }

        public static long? UserId
        {
            get
            {
                return _userId;
            }
            private set
            {
                _userId = value;

                if (_userId.HasValue)
                {
                    PreferenceHelper.PreferenceSetAsync(UserId_Preference_Name, _userId.Value.ToString(CultureInfo.InvariantCulture)).Wait();
                }
            }
        }

        public static DateTimeOffset? UserCreateTime
        {
            get
            {
                return _userCreateTime;
            }
            private set
            {
                _userCreateTime = value;

                if (_userCreateTime.HasValue)
                {
                    PreferenceHelper.PreferenceSetAsync(UserCreateTime_Preference_Name, _userCreateTime.Value.ToString(CultureInfo.InvariantCulture)).Wait();
                }
            }
        }

        public static string? Mobile
        {
            get
            {
                return _mobile;
            }
            private set
            {
                _mobile = value;

                if (_mobile.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(Mobile_Preference_Name, _mobile).Wait();
                }
            }
        }

        public static string? Email
        {
            get => _email;
            private set
            {
                _email = value;

                if(_email.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(Email_Preference_Name, _email).Wait();
                }
            }
        }

        public static string? LoginName
        {
            get => _loginName;
            private set
            {
                _loginName = value;

                if (_loginName.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(LoginName_Preference_Name, _loginName).Wait();
                }
            }
        }

        public static string? AccessToken
        {
            get => _accessToken;
            set
            {
                _accessToken = value;

                if(_accessToken.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(AccessToken_Preference_Name, _accessToken).Wait();
                }
            }
        }

        public static string? RefreshToken
        {
            get => _refreshToken;
            private set
            {
                _refreshToken = value;

                if (_refreshToken.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(RefreshToken_Preference_Name, _refreshToken).Wait();
                }
            }
        }

        public static bool IsLogined => AccessToken.IsNotNullOrEmpty();
        

        public static void Login(long userId, DateTimeOffset userCreateTime, string? mobile, string? email, string? loginName, string? accessToken, string? refreshToken)
        {
            UserId = userId;
            UserCreateTime = userCreateTime;
            Mobile = mobile;
            Email = email;
            LoginName = loginName;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public static void Logout()
        {
            AccessToken = null;
            RefreshToken = null;
        }
    }
}
