using System;
using System.Globalization;
using System.Threading.Tasks;

using HB.FullStack.Client.Maui.Utils;

using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;

namespace HB.FullStack.Client.Maui
{
    public class PreferenceProvider : IPreferenceProvider
    {
        private readonly IStatusManager _statusManager;

        public PreferenceProvider(IStatusManager statusManager)
        {
            _statusManager = statusManager;
        }

        public string? AccessToken { get => UserPreferences.AccessToken; set => UserPreferences.AccessToken = value ?? ""; }

        public string? RefreshToken { get => UserPreferences.RefreshToken; set => UserPreferences.RefreshToken = value ?? ""; }

        public string DeviceId { get => DevicePreferences.DeviceId; }

        public string DeviceVersion { get => DevicePreferences.DeviceVersion; }

        public DeviceInfos DeviceInfos { get => DevicePreferences.DeviceInfos; }

        public void OnTokenRefreshFailed() => UserPreferences.Logout();

        public bool IsLogined() => UserPreferences.IsLogined;

        public bool IsIntroducedYet { get => UserPreferences.IsIntroducedYet; set => UserPreferences.IsIntroducedYet = value; }

        public void OnLogined(Guid userId, DateTimeOffset userCreateTime, string? mobile, string? email, string? loginName, string accessToken, string refreshToken)
        {
            UserPreferences.Login(userId, userCreateTime, mobile, email, loginName, accessToken, refreshToken);

            _statusManager.ReportLogined();
        }

        public void OnLogouted()
        {
            UserPreferences.Logout();

            _statusManager.ReportLogouted();
        }

        public Guid? UserId { get => UserPreferences.UserId; set => UserPreferences.UserId = value; }


        private static class UserPreferences
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

            //TODO: 思考：如果ApiClient里为每一个Endpoint设置不同的JwtEndpoint，那么accesToken和Refresh Token是不是也会有多个？

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

        private static class DevicePreferences
        {
            private const int ADDRESS_REQUEST_INTERVAL_SECONDS = 60;
            public const string PREFERENCE_NAME_DEVICEID = "dbuKErtT";

            private static string? _deviceId;
            private static string? _deviceVersion;
            private static DeviceInfos? _deviceInfos;
            private static string? _deviceAddress;

            private static MemorySimpleLocker RequestLocker { get; } = new MemorySimpleLocker();

            public static string DeviceId
            {
                get
                {
                    if (_deviceId.IsNullOrEmpty())
                    {
                        string? stored = PreferenceHelper.Get(PREFERENCE_NAME_DEVICEID);

                        if (stored.IsNullOrEmpty())
                        {
                            stored = CreateNewDeviceId();
                            PreferenceHelper.Set(PREFERENCE_NAME_DEVICEID, stored);
                        }

                        _deviceId = stored;
                    }

                    return _deviceId;
                }
            }

            public static string CreateNewDeviceId()
            {
                return SecurityUtil.CreateUniqueToken();
            }

            public static DeviceInfos DeviceInfos
            {
                get
                {
                    if (_deviceInfos == null)
                    {
                        _deviceInfos = new DeviceInfos
                        {
                            Name = DeviceInfo.Name ?? "Unkown",
                            Model = DeviceInfo.Model ?? "Unkown",
                            OSVersion = DeviceInfo.VersionString ?? "Unkown",
                            Platform = DeviceInfo.Platform.ToString(),
                            Idiom = DeviceInfo.Idiom.ToString() switch
                            {
                                "Phone" => System.DeviceIdiom.Phone,
                                "Tablet" => System.DeviceIdiom.Tablet,
                                "Desktop" => System.DeviceIdiom.Desktop,
                                "TV" => System.DeviceIdiom.TV,
                                "Watch" => System.DeviceIdiom.Watch,
                                "Web" => System.DeviceIdiom.Web,
                                _ => System.DeviceIdiom.Unknown
                            },
                            Type = DeviceInfo.DeviceType.ToString()
                        };
                    }

                    return _deviceInfos!;
                }
            }

            public static string DeviceVersion
            {
                get
                {
                    if (_deviceVersion.IsNullOrEmpty())
                    {
                        _deviceVersion = AppInfo.VersionString;
                    }

                    return _deviceVersion!;
                }
            }


            public static async Task<string> GetDeviceAddressAsync()
            {
                if (_deviceAddress.IsNullOrEmpty() || RequestLocker.NoWaitLock(nameof(DevicePreferences), nameof(GetDeviceAddressAsync), TimeSpan.FromSeconds(ADDRESS_REQUEST_INTERVAL_SECONDS)))
                {
                    _deviceAddress = await GetLastKnownAddressAsync();
                }

                return _deviceAddress;
            }

            private static async Task<string> GetLastKnownAddressAsync()
            {
                try
                {
                    Location? location = await Geolocation.GetLastKnownLocationAsync();

                    if (location != null)
                    {
                        return $"{location.Latitude}.{location.Longitude}.{location.Altitude}.{location.Accuracy}.{location.Speed}.{location.Timestamp}";
                    }
                }
                catch (FeatureNotSupportedException)
                {
                    // Handle not supported on device exception
                }
                catch (FeatureNotEnabledException)
                {
                    // Handle not enabled on device exception
                }
                catch (PermissionException)
                {
                    // Handle permission exception
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // Unable to get location
                }

                return "unkown";
            }
        }

    }
}
