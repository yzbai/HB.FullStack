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
        private static string? _accessToken;
        private static bool _accessTokenFirstRead = true;
        private static string? _refreshToken;
        private static bool _refreshTokenFirstRead = true;

        public static long? UserId
        {
            get
            {
                if (_userIdFirstRead && !_userId.HasValue)
                {
                    _userIdFirstRead = false;

                    string? storedValue = PreferenceHelper.PreferenceGetAsync(Consts.UserId_Preference_Name).Result;
                    _userId = storedValue == null ? null : Convert.ToInt64(storedValue, CultureInfo.InvariantCulture);
                }

                return _userId;
            }
            private set
            {
                _userId = value;

                if (_userId.HasValue)
                {
                    PreferenceHelper.PreferenceSetAsync(Consts.UserId_Preference_Name, _userId.Value.ToString(CultureInfo.InvariantCulture)).Wait();
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
                    string? storedValue = PreferenceHelper.PreferenceGetAsync(Consts.UserCreateTime_Preference_Name).Result;
                    _userCreateTime = storedValue == null ? null : DateTimeOffset.Parse(storedValue, CultureInfo.InvariantCulture);
                }

                return _userCreateTime;
            }
            private set
            {
                _userCreateTime = value;

                if (_userCreateTime.HasValue)
                {
                    PreferenceHelper.PreferenceSetAsync(Consts.UserCreateTime_Preference_Name, _userCreateTime.Value.ToString(CultureInfo.InvariantCulture)).Wait();
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
                    _mobile = PreferenceHelper.PreferenceGetAsync(Consts.Mobile_Preference_Name).Result;
                }
                return _mobile;
            }
            private set
            {
                _mobile = value;

                if (_mobile.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(Consts.Mobile_Preference_Name, _mobile).Wait();
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
                    _email = PreferenceHelper.PreferenceGetAsync(Consts.Email_Preference_Name).Result;
                }

                return _email;
            }
            private set
            {
                _email = value;

                if (_email.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(Consts.Email_Preference_Name, _email).Wait();
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
                    _loginName = PreferenceHelper.PreferenceGetAsync(Consts.LoginName_Preference_Name).Result;
                }

                return _loginName;
            }
            private set
            {
                _loginName = value;

                if (_loginName.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(Consts.LoginName_Preference_Name, _loginName).Wait();
                }
            }
        }

        public static string? AccessToken
        {
            get 
            {
                if(_accessTokenFirstRead && _accessToken.IsNullOrEmpty())
                {
                    _accessTokenFirstRead = false;
                    _accessToken = PreferenceHelper.PreferenceGetAsync(Consts.AccessToken_Preference_Name).Result;
                }

                return _accessToken;
            }
            set
            {
                _accessToken = value;

                if (_accessToken.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(Consts.AccessToken_Preference_Name, _accessToken).Wait();
                }
            }
        }

        public static string? RefreshToken
        {
            get
            {
                if (_refreshTokenFirstRead && _refreshToken.IsNullOrEmpty())
                {
                    _refreshTokenFirstRead = false;
                    _refreshToken = PreferenceHelper.PreferenceGetAsync(Consts.RefreshToken_Preference_Name).Result;
                }

                return _refreshToken;
            }
            private set
            {
                _refreshToken = value;

                if (_refreshToken.IsNotNullOrEmpty())
                {
                    PreferenceHelper.PreferenceSetAsync(Consts.RefreshToken_Preference_Name, _refreshToken).Wait();
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
