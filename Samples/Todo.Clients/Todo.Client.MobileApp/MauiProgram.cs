/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.IO;

using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.MauiLib.Components;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.LifecycleEvents;

using Todo.Shared;

namespace Todo.Client.MobileApp
{
    /// <summary>
    /// 事件综合:
    /// 1. Application Events - PlatformApplication
    /// 2. Window Events - Activity
    /// 3. Page Events
    /// 4. Navigation Events
    /// 5. Network Events
    /// 6. UserEvents
    /// </summary>

    /// Maui Application：就是Window的管理者
    /// Platform Application:
    /// Maui Window: 在Android中就是Activity，提供Page的舞台

    public static class MauiProgram
    {
        public const string SITE_TODO_SERVER_MAIN = "Todo.Server.Main";
        public const string SITE_TODO_SERVER_MAIN_BASE_URL = "https://localhost:7157/api/";

        public const string DB_SCHEMA_MAIN = "TodoMain";
        public const string DB_SCHEMA_MAIN_FILE = "TodoMain.db";
        public const string DB_SCHEMA_USER = "TodoUser_{0}";
        public const string DB_SCHEMA_USER_FILE = "TodoUser_{0}.db";

        private const string ALIYUN_OSS_ENDPOINT = "oss-cn-hangzhou.aliyuncs.com";
        private const string ALIYUN_OSS_BUCKET_NAME = "mycolorfultime-private-dev";

        private const string TECENET_CAPTCHA_APP_ID = "2029147713";

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMarkup()
                .UseMauiCommunityToolkitMediaElement()
                .UseFullStackMaui(
                    dbOptions =>
                    {
                        dbOptions.DbSchemas.Add(new DbSchema
                        {
                            Name = DB_SCHEMA_MAIN,
                            EngineType = DbEngineType.SQLite,
                            Version = 1,
                            ConnectionString = new ConnectionString($"Data Source={Path.Combine(Currents.AppDataDirectory, DB_SCHEMA_MAIN_FILE)}")
                        });
                        dbOptions.DbSchemas.Add(new DbSchema
                        {
                            Name = DB_SCHEMA_USER,
                            EngineType = DbEngineType.SQLite,
                            Version = 1
                            //不提供ConnectionString，在登录后再提供.每一个用户，一个数据库文件
                        });
                        //dbOptions.InitContexts.Add(new DbInitContext { });
                    },
                    apiClientOptions =>
                    {
                        if (Currents.IsDebug)
                        {
                            apiClientOptions.HttpClientTimeout = TimeSpan.FromMinutes(15);
                        }

                        apiClientOptions.TokenSiteSetting = new SiteSetting
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
                    clientOptions =>
                    {
                        clientOptions.AvatarDirectory = DirectorySettings.PUBLIC_AVATAR;
                    },
                    mauiInitOptions =>
                    {
                        mauiInitOptions.DefaultAvatarFileName = "default_avatar.png";
                        mauiInitOptions.IntoduceContents = new IntroduceContent[] 
                        {
                            new IntroduceContent{ ImageSource="introduce_1.png", IsLastPage = false},
                            new IntroduceContent{ ImageSource="introduce_2.png", IsLastPage = false},
                            new IntroduceContent{ ImageSource="introduce_3.png", IsLastPage = false},
                            new IntroduceContent{ ImageSource="introduce_4.png", IsLastPage = true},
                        };
                    },
                    tCaptchaAppId: TECENET_CAPTCHA_APP_ID)
                .UseTodoApp(todoAppOptions => { })
                .ConfigureLifecycleEvents(lifecycleBuilder => { })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            return builder.Build();
        }

        public static MauiAppBuilder UseTodoApp(this MauiAppBuilder builder, Action<TodoAppOptions> configTodoAppOptions)
        {
            builder.Services.Configure(configTodoAppOptions);

            AddServices(builder.Services);

            AddViewModels(builder.Services);

            AddPages(builder.Services);

            return builder;

            static void AddServices(IServiceCollection services)
            {
            }

            static void AddViewModels(IServiceCollection services)
            {
                services.AddSingleton<HomeViewModel>();
            }

            static void AddPages(IServiceCollection services)
            {
                services.AddSingleton<HomePage>();
            }
        }
    }
}