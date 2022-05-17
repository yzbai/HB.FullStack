using System;
using System.Globalization;

namespace HB.FullStack.Client.UI.Maui
{
    public static class UserPreferences
    {
        public const string PREFERENCE_NAME_USERID = "wjUfoxCi";
        public const string PREFERENCE_NAME_USERCREATETIME = "WMIliRIP";
        public const string PREFERENCE_NAME_MOBILE = "H8YA3d5aj";
        public const string PREFERENCE_NAME_EMAIL = "B2JG5UN5f";
        public const string PREFERENCE_NAME_LOGINNAME = "UwsSmhY1";
        public const string PREFERENCE_NAME_ACCESSTOKEN = "D3SQAAtrv";
        public const string PREFERENCE_NAME_REFRESHTOKEN = "ZTpMCJQl";
        public const string PREFERENCE_NAME_INTRODUCEDYET = "BuOMCJ7l";


        private static Guid? _userId;
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

        public static bool IsIntroducedYet
        {
            get
            {
                string? storedValue = PreferenceHelper.Get(PREFERENCE_NAME_INTRODUCEDYET);
                return storedValue != null && Convert.ToBoolean(storedValue, CultureInfo.InvariantCulture);
            }
            set
            {
                PreferenceHelper.Set(PREFERENCE_NAME_INTRODUCEDYET, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static Guid? UserId
        {
            get
            {
                if (_userIdFirstRead && !_userId.HasValue)
                {
                    _userIdFirstRead = false;

                    string? storedValue = PreferenceHelper.Get(PREFERENCE_NAME_USERID);
                    _userId = storedValue == null ? null : Guid.Parse(storedValue);
                }

                return _userId;
            }
            set
            {
                _userId = value;

                if (_userId.HasValue)
                {
                    PreferenceHelper.Set(PREFERENCE_NAME_USERID, _userId.Value.ToString());
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
                    string? storedValue = PreferenceHelper.Get(PREFERENCE_NAME_USERCREATETIME);
                    _userCreateTime = storedValue == null ? null : DateTimeOffset.Parse(storedValue, CultureInfo.InvariantCulture);
                }

                return _userCreateTime;
            }
            private set
            {
                _userCreateTime = value;

                if (_userCreateTime.HasValue)
                {
                    PreferenceHelper.Set(PREFERENCE_NAME_USERCREATETIME, _userCreateTime.Value.ToString(CultureInfo.InvariantCulture));
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
                    _mobile = PreferenceHelper.Get(PREFERENCE_NAME_MOBILE);
                }
                return _mobile;
            }
            private set
            {
                _mobile = value;

                if (_mobile.IsNotNullOrEmpty())
                {
                    PreferenceHelper.Set(PREFERENCE_NAME_MOBILE, _mobile);
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
                    _email = PreferenceHelper.Get(PREFERENCE_NAME_EMAIL);
                }

                return _email;
            }
            private set
            {
                _email = value;

                if (_email.IsNotNullOrEmpty())
                {
                    PreferenceHelper.Set(PREFERENCE_NAME_EMAIL, _email);
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
                    _loginName = PreferenceHelper.Get(PREFERENCE_NAME_LOGINNAME);
                }

                return _loginName;
            }
            private set
            {
                _loginName = value;

                if (_loginName.IsNotNullOrEmpty())
                {
                    PreferenceHelper.Set(PREFERENCE_NAME_LOGINNAME, _loginName);
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

                    string? stored = PreferenceHelper.Get(PREFERENCE_NAME_ACCESSTOKEN);

                    _accessToken = stored ?? "";
                }

                return _accessToken;
            }
            set
            {
                _accessToken = value;

                PreferenceHelper.Set(PREFERENCE_NAME_ACCESSTOKEN, _accessToken);
            }
        }

        public static string RefreshToken
        {
            get
            {
                if (_refreshTokenFirstRead && _refreshToken.IsNullOrEmpty())
                {
                    _refreshTokenFirstRead = false;
                    string? stored = PreferenceHelper.Get(PREFERENCE_NAME_REFRESHTOKEN);

                    _refreshToken = stored ?? "";
                }

                return _refreshToken;
            }
            internal set
            {
                _refreshToken = value;

                PreferenceHelper.Set(PREFERENCE_NAME_REFRESHTOKEN, _refreshToken);
            }
        }

        public static bool IsLogined => AccessToken.IsNotNullOrEmpty();

        public static void Login(Guid userId, DateTimeOffset userCreateTime, string? mobile, string? email, string? loginName, string? accessToken, string? refreshToken)
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
