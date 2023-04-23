/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Globalization;
using System.Threading.Tasks;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Resources;

using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;

namespace HB.FullStack.Client.MauiLib
{
    internal class MauiTokenPreferences : ITokenPreferences
    {
        #region Client

        public string ClientId => ClientPreferences.ClientId;

        public string ClientVersion => ClientPreferences.ClientVersion;

        public DeviceInfos DeviceInfos => ClientPreferences.DeviceInfos;

        public bool IsIntroducedYet { get => ClientPreferences.IsIntroducedYet; set => ClientPreferences.IsIntroducedYet = value; }

        #endregion

        #region User

        public Guid? UserId { get => TokenPreferences.UserId; }

        public string? Mobile { get => TokenPreferences.Mobile; }

        public string? LoginName { get => TokenPreferences.LoginName; }

        public string? Email { get => TokenPreferences.Email; }

        public bool EmailConfirmed { get => TokenPreferences.EmailConfirmed; }

        public bool MobileConfirmed { get => TokenPreferences.MobileConfirmed; }

        public bool TwoFactorEnabled { get => TokenPreferences.TwoFactorEnabled; }

        public DateTimeOffset? TokenCreatedTime { get => TokenPreferences.TokenCreateTime; }

        public string? AccessToken { get => TokenPreferences.AccessToken; }

        public string? RefreshToken { get => TokenPreferences.RefreshToken; }

        public void OnTokenRefreshFailed() => TokenPreferences.DeleteToken();

        public void OnTokenFetched(TokenRes signInReceipt) => TokenPreferences.SetToken(signInReceipt);

        public void OnTokenDeleted() => TokenPreferences.DeleteToken();

        #endregion
    }

    internal static class TokenPreferences
    {
        public const string PREFERENCE_NAME_USERID = "wjUfoxCi";
        public const string PREFERENCE_NAME_TOKEN_CREATETIME = "WMIliRIP";
        public const string PREFERENCE_NAME_MOBILE = "H8YA3d5aj";
        public const string PREFERENCE_NAME_MOBILE_CONFIRMED = "H8xAsedxaj";
        public const string PREFERENCE_NAME_EMAIL = "B2JG5UN5f";
        public const string PREFERENCE_NAME_EMAIL_CONFIRMED = "ebJG5UN5f";
        public const string PREFERENCE_NAME_TWOFACTOR_ENABLED = "ejsg94ks";
        public const string PREFERENCE_NAME_LOGINNAME = "UwsSmhY1";
        public const string PREFERENCE_NAME_ACCESSTOKEN = "D3SQAAtrv";
        public const string PREFERENCE_NAME_REFRESHTOKEN = "ZTpMCJQl";

        private static Guid? _userId;
        private static bool _userIdFirstRead = true;
        private static DateTimeOffset? _tokenCreateTime;
        private static bool _tokenCreateTimeFirstRead = true;
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
        private static bool? _emailConfirmed;
        private static bool _emailConfirmed_FirstRead = true;
        private static bool? _mobileConfirmed;
        private static bool _mobileConfirmed_FirstRead = true;
        private static bool? _twoFactoryEnabled;
        private static bool _twoFactoryEnabled_FirstRead = true;

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

        public static DateTimeOffset? TokenCreateTime
        {
            get
            {
                if (_tokenCreateTimeFirstRead && !_tokenCreateTime.HasValue)
                {
                    _tokenCreateTimeFirstRead = false;
                    string? storedValue = PreferenceHelper.Get(PREFERENCE_NAME_TOKEN_CREATETIME);
                    _tokenCreateTime = storedValue == null ? null : DateTimeOffset.Parse(storedValue, CultureInfo.InvariantCulture);
                }

                return _tokenCreateTime;
            }
            private set
            {
                _tokenCreateTime = value;

                if (_tokenCreateTime.HasValue)
                {
                    PreferenceHelper.Set(PREFERENCE_NAME_TOKEN_CREATETIME, _tokenCreateTime.Value.ToString(CultureInfo.InvariantCulture));
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

        public static bool EmailConfirmed
        {
            get
            {
                if (_emailConfirmed_FirstRead && _emailConfirmed == null)
                {
                    _emailConfirmed_FirstRead = false;
                    string? stored = PreferenceHelper.Get(PREFERENCE_NAME_EMAIL_CONFIRMED);

                    _emailConfirmed = stored == null ? null : bool.Parse(stored);
                }

                return _emailConfirmed ?? false;
            }
            internal set
            {
                _emailConfirmed = value;

                PreferenceHelper.Set(PREFERENCE_NAME_EMAIL_CONFIRMED, value.ToString());
            }
        }

        public static bool MobileConfirmed
        {
            get
            {
                if (_mobileConfirmed_FirstRead && _mobileConfirmed == null)
                {
                    _mobileConfirmed_FirstRead = false;
                    string? stored = PreferenceHelper.Get(PREFERENCE_NAME_MOBILE_CONFIRMED);

                    _mobileConfirmed = stored == null ? null : bool.Parse(stored);
                }

                return _mobileConfirmed ?? false;
            }
            internal set
            {
                _mobileConfirmed = value;

                PreferenceHelper.Set(PREFERENCE_NAME_MOBILE_CONFIRMED, value.ToString());
            }
        }

        public static bool TwoFactorEnabled
        {
            get
            {
                if (_twoFactoryEnabled_FirstRead && _twoFactoryEnabled == null)
                {
                    _twoFactoryEnabled_FirstRead = false;
                    string? stored = PreferenceHelper.Get(PREFERENCE_NAME_TWOFACTOR_ENABLED);

                    _twoFactoryEnabled = stored == null ? null : bool.Parse(stored);
                }

                return _twoFactoryEnabled ?? false;
            }
            internal set
            {
                _twoFactoryEnabled = value;

                PreferenceHelper.Set(PREFERENCE_NAME_TWOFACTOR_ENABLED, value.ToString());
            }
        }

        public static void SetToken(TokenRes tokenRes)
        {
            UserId = tokenRes.UserId;
            Mobile = tokenRes.Mobile;
            LoginName = tokenRes.LoginName;
            Email = tokenRes.Email;
            AccessToken = tokenRes.AccessToken ?? "";
            RefreshToken = tokenRes.RefreshToken ?? "";
            TokenCreateTime = tokenRes.TokenCreatedTime;
        }

        public static void DeleteToken()
        {
            AccessToken = "";
            RefreshToken = "";
        }
    }

    internal static class ClientPreferences
    {
        private const int ADDRESS_REQUEST_INTERVAL_SECONDS = 60;
        public const string PREFERENCE_NAME_CLIENTID = "dbuKErtT";
        public const string PREFERENCE_NAME_INTRODUCEDYET = "BuOMCJ7l";

        private static string? _clientId;
        private static string? _clientVersion;
        private static DeviceInfos? _deviceInfos;
        private static string? _deviceAddress;

        private static MemorySimpleLocker RequestLocker { get; } = new MemorySimpleLocker();

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

        public static string ClientId
        {
            get
            {
                if (_clientId.IsNullOrEmpty())
                {
                    string? stored = PreferenceHelper.Get(PREFERENCE_NAME_CLIENTID);

                    if (stored.IsNullOrEmpty())
                    {
                        stored = CreateNewClientId();
                        PreferenceHelper.Set(PREFERENCE_NAME_CLIENTID, stored);
                    }

                    _clientId = stored;
                }

                return _clientId;
            }
        }

        public static string CreateNewClientId()
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
                            "Phone" => Common.Shared.DeviceIdiom.Phone,
                            "Tablet" => Common.Shared.DeviceIdiom.Tablet,
                            "Desktop" => Common.Shared.DeviceIdiom.Desktop,
                            "TV" => Common.Shared.DeviceIdiom.TV,
                            "Watch" => Common.Shared.DeviceIdiom.Watch,
                            "Web" => Common.Shared.DeviceIdiom.Web,
                            _ => Common.Shared.DeviceIdiom.Unknown
                        },
                        Type = DeviceInfo.DeviceType.ToString()
                    };
                }

                return _deviceInfos!;
            }
        }

        public static string ClientVersion
        {
            get
            {
                if (_clientVersion.IsNullOrEmpty())
                {
                    _clientVersion = AppInfo.VersionString;
                }

                return _clientVersion!;
            }
        }

        public static async Task<string> GetDeviceAddressAsync()
        {
            if (_deviceAddress.IsNullOrEmpty() || RequestLocker.NoWaitLock(nameof(ClientPreferences), nameof(GetDeviceAddressAsync), TimeSpan.FromSeconds(ADDRESS_REQUEST_INTERVAL_SECONDS)))
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