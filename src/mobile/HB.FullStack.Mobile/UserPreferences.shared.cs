using HB.FullStack.XamarinForms.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms
{
    //TODO: 考虑SecurityStorage不支持时，改用普通的Storage
    public static class UserPreferences
    {
        private static long? _userId;
        private static bool _userIdFirstRead = true;
        private static DateTimeOffset? _userCreateTime;
        private static bool _userCreateTimeFirstRead = true;
        private static string? _mobile;
        private static bool _mobileFirstRead = true;
        private static string? _loginName;
        private static bool _loginNameFirstRead = true;
        private static string? _email;
        private static bool _emailFirstRead = true;
        private static string _accessToken = null!;
        private static bool _accessTokenFirstRead = true;
        private static string _refreshToken = null!;
        private static bool _refreshTokenFirstRead = true;

        public static long? UserId
        {
            get
            {
                if (_userIdFirstRead && !_userId.HasValue)
                {
                    _userIdFirstRead = false;

                    string? storedValue = PreferenceHelper.PreferenceGetAsync(Conventions.UserId_Preference_Name).Result;
                    _userId = storedValue == null ? null : Convert.ToInt64(storedValue, CultureInfo.InvariantCulture);
                }

                return _userId;
            }
            private set
            {
                _userId = value;

                if (_userId.HasValue)
                {
                    PreferenceHelper.PreferenceSetAsync(Conventions.UserId_Preference_Name, _userId.Value.ToString(CultureInfo.InvariantCulture)).Wait();
                }
            }
        }

        public static DateTimeOffset? UserCreateTime
        {
            get
            {
                if (_userCreateTimeFirstRead && !_userCreateTime.HasValue)
                {
                    _userCreateTimeFirstRead = false;
                    string? storedValue = PreferenceHelper.PreferenceGetAsync(Conventions.UserCreateTime_Preference_Name).Result;
                    _userCreateTime = storedValue == null ? null : DateTimeOffset.Parse(storedValue, CultureInfo.InvariantCulture);
                }

                return _userCreateTime;
            }
            private set
            {
                _userCreateTime = value;

                if (_userCreateTime.HasValue)
                {
                    PreferenceHelper.PreferenceSetAsync(Conventions.UserCreateTime_Preference_Name, _userCreateTime.Value.ToString(CultureInfo.InvariantCulture)).Wait();
                }
            }
        }

        public static string? Mobile
        {
            get
            {
                if (_mobileFirstRead && _mobile.IsNullOrEmpty())
                {
                    _mobileFirstRead = false;
                    _mobile = PreferenceHelper.PreferenceGetAsync(Conventions.Mobile_Preference_Name).Result;
                }
                return _mobile;
            }
            private set
            {
                _mobile = value;

                if (_mobile.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(Conventions.Mobile_Preference_Name, _mobile).Wait();
                }
            }
        }

        public static string? Email
        {
            get
            {
                if (_emailFirstRead && _email.IsNullOrEmpty())
                {
                    _emailFirstRead = false;
                    _email = PreferenceHelper.PreferenceGetAsync(Conventions.Email_Preference_Name).Result;
                }

                return _email;
            }
            private set
            {
                _email = value;

                if (_email.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(Conventions.Email_Preference_Name, _email).Wait();
                }
            }
        }

        public static string? LoginName
        {
            get
            {
                if (_loginNameFirstRead && _loginName.IsNullOrEmpty())
                {
                    _loginNameFirstRead = false;
                    _loginName = PreferenceHelper.PreferenceGetAsync(Conventions.LoginName_Preference_Name).Result;
                }

                return _loginName;
            }
            private set
            {
                _loginName = value;

                if (_loginName.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(Conventions.LoginName_Preference_Name, _loginName).Wait();
                }
            }
        }

        public static string AccessToken
        {
            get
            {
                if (_accessTokenFirstRead && _accessToken.IsNullOrEmpty())
                {
                    _accessTokenFirstRead = false;

                    string? stored = PreferenceHelper.PreferenceGetAsync(Conventions.AccessToken_Preference_Name).Result;

                    _accessToken = stored ?? "";
                }

                return _accessToken;
            }
            set
            {
                _accessToken = value;

                PreferenceHelper.PreferenceSetAsync(Conventions.AccessToken_Preference_Name, _accessToken).Wait();
            }
        }

        public static string RefreshToken
        {
            get
            {
                if (_refreshTokenFirstRead && _refreshToken.IsNullOrEmpty())
                {
                    _refreshTokenFirstRead = false;
                    string? stored = PreferenceHelper.PreferenceGetAsync(Conventions.RefreshToken_Preference_Name).Result;

                    _refreshToken = stored ?? "";
                }

                return _refreshToken;
            }
            private set
            {
                _refreshToken = value;

                PreferenceHelper.PreferenceSetAsync(Conventions.RefreshToken_Preference_Name, _refreshToken).Wait();
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
            AccessToken = accessToken ?? "";
            RefreshToken = refreshToken ?? "";
        }

        public static void Logout()
        {
            AccessToken = "";
            RefreshToken = "";
        }
    }
}
