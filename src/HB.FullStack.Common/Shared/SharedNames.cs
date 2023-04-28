/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

namespace HB.FullStack.Common.Shared
{
    public static class SharedNames
    {
        public static class ApiHeaders
        {
            public const string Authorization = "Authorization";
            public const string XApiKey = "X-Api-Key";
            public const string Captcha = "Captcha";
            public const string CLIENT_ID = Client.CLIENT_ID;
            public const string CLIENT_VERSION = Client.CLIENT_VERSION;
            public const string CommonResourceToken = "CRT";
        }

        public static class Conditions
        {
            public const string ById = nameof(ById);
            public const string ByIds = nameof(ByIds);

            public const string ByMobile = nameof(ByMobile);
            public const string BySms = nameof(BySms);
            public const string ByLoginName = nameof(ByLoginName);
            public const string ByRefresh = nameof(ByRefresh);

            public const string ByUserId = nameof(ByUserId);
            public const string ByDirectoryPermissionName = nameof(ByDirectoryPermissionName);
        }

        /// <summary>
        /// Client与Server沟通时，约定好的默认名称
        /// </summary>
        public static class Client
        {
            //public const string ACCESS_TOKEN = "AccessToken";
            //public const string REFRESH_TOKEN = "RefreshToken";

            public const string CLIENT_ID = "ClientId";
            public const string CLIENT_VERSION = "ClientVersion";
            //public const string DEVICE_INFOS = "DeviceInfos";
            //public const string DEVICE_ADDRESS = "DeviceAddress";

            public const string RANDOM_STR = "RandomStr";

            public const string TIMESTAMP = "Timestamp";

            public const string Page = "Page";

            public const string PerPage = "PerPage";

            public const string OrderBy = "OrderBy";

            //public const string PUBLIC_RESOURCE_TOKEN = "PublicResourceToken";

            //public const string MOBILE = "Mobile";

            //public const string SMS_CODE = "SmsCode";

            //public const string EMAIL = "Email";
        }

        public static class Length
        {
            public const int MAX_URL_LENGTH = 2000;
            public const int MAX_ARGUMENTS_LENGTH = 2000;
            public const int MAX_RESULT_ERROR_LENGTH = 2000;

            public const int MAX_USER_CLAIM_VALUE_LENGTH = 5000;
            public const int MAX_ROLE_COMMENT_LENGTH = 1024;

            public const int MAX_USER_LOGIN_NAME_LENGTH = 100;
            public const int MAX_USER_MOBILE_LENGTH = 14;
            public const int MAX_USER_EMAIL_LENGTH = 256;

            public const int MAX_FILE_NAME_LENGTH = 128;
        }
    }
}