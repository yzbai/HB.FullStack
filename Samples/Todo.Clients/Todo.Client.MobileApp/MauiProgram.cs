using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Database;
using HB.FullStack.Database.Config;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

using Todo.Shared;

namespace Todo.Client.MobileApp
{
    public static class MauiProgram
    {
        public const string SITE_TODO_SERVER_MAIN = "Todo.Server.Main";
        public const string SITE_TODO_SERVER_MAIN_BASE_URL = "https://localhost:7157/api/";

        private const string DEBUG_HOST = "192.168.0.109";
        private const int DEBUG_HTTP_PORT = 5021;
        private const int DEBUG_HTTPS_PORT = 7021;

        private const string RELEASE_HOST = "https://time.brlite.com";
        private const int RELEASE_HTTP_PORT = 80;
        private const int RELEASE_HTTPS_PORT = 443;

        private const string SITE_MAIN_API = "MyColorfulTime.Server.MainApi";
        private const string SITE_MAIN_API_VERSION = "V1";
        private const string STS_TOKEN_URL = "api/V1/StsToken/ByDirectoryPermissionName";

        private const string ALIYUN_OSS_ENDPOINT = "oss-cn-hangzhou.aliyuncs.com";
        private const string ALIYUN_OSS_BUCKET_NAME = "mycolorfultime-private-dev";

        private const string TECENET_CAPTCHA_APP_ID = "2029147713";

        private static readonly bool _useSSL = true;
        private static readonly string _httpProtocal = _useSSL ? "https" : "http";
        private static readonly int _httpPort = _useSSL ? (Currents.IsDebug ? DEBUG_HTTPS_PORT : RELEASE_HTTPS_PORT) : (Currents.IsDebug ? DEBUG_HTTP_PORT : RELEASE_HTTP_PORT);
        private static readonly string _httpHost = Currents.IsDebug ? DeviceInfo.Platform.ToString() switch
        {
            "macOS" or "iOS" => "",
            "Android" => DEBUG_HOST,
            "WinUI" or "UWP" => DEBUG_HOST,
            _ => throw new NotImplementedException(),
        } : RELEASE_HOST;
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMarkup()
                .UseMauiCommunityToolkitMediaElement()
                .UseFullStackClient(
                    dbOptions =>
                    {
                        dbOptions.DbSchemas.Add(new DbSchema
                        {

                        });
                        dbOptions.DbSchemas.Add(new DbSchema
                        {

                        });
                        dbOptions.InitContexts.Add(new DbInitContext { });
                    },
                    apiClientOptions =>
                    {
                        if (Currents.IsDebug)
                        {
                            apiClientOptions.HttpClientTimeout = TimeSpan.FromMinutes(15);
                        }

                        apiClientOptions.SignInReceiptSiteSetting = new SiteSetting
                        {
                            SiteName = SITE_TODO_SERVER_MAIN,
                            BaseUrl = new Uri(SITE_TODO_SERVER_MAIN_BASE_URL)
                        };
                    },
                    fileManagerOptions =>
                    {
                        fileManagerOptions.AliyunOssEndpoint = ALIYUN_OSS_ENDPOINT;
                        fileManagerOptions.AliyunOssBucketName = ALIYUN_OSS_BUCKET_NAME;
                        fileManagerOptions.DirectoryDescriptions = DirectorySettings.Descriptions.All;
                        fileManagerOptions.DirectoryPermissions = DirectorySettings.DirectoryPermissions.All;
                    },
                    initOptions => { },
                    tCaptchaAppId: "xxx")
                .UseTodo()
                .ConfigureLifecycleEvents(lifecycleBuilder => { })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        public static MauiAppBuilder UseTodo(this MauiAppBuilder builder)
        {
            return builder;
        }
    }
}