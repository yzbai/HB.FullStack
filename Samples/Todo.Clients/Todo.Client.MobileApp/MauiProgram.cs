/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.MauiLib.Components;
using HB.FullStack.Common.Files;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.LifecycleEvents;

using Todo.Shared;
using Todo.Shared.Resources;

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
        public static MauiApp CreateMauiApp()
        {
            AppSettings appSettings = AppSettings.GetAppSettings(Currents.Environment);
            SharedSettings sharedSettings = SharedSettings.GetSharedSettings(Currents.Environment);

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
                            Name = appSettings.DB_SCHEMA_MAIN,
                            EngineType = DbEngineType.SQLite,
                            Version = 1,
                            ConnectionString = new ConnectionString($"Data Source={Path.Combine(Currents.AppDataDirectory, appSettings.DB_SCHEMA_MAIN_FILE)}")
                        });
                        dbOptions.DbSchemas.Add(new DbSchema
                        {
                            Name = appSettings.DB_SCHEMA_USER,
                            EngineType = DbEngineType.SQLite,
                            Version = 1
                            //不提供ConnectionString，在登录后再提供.每一个用户，一个数据库文件
                        });
                        //dbOptions.InitContexts.Add(new DbInitContext { });
                    },
                    apiClientOptions =>
                    {
                        //if (Currents.IsDebug)
                        //{
                        //    apiClientOptions.HttpClientTimeout = TimeSpan.FromMinutes(15);
                        //}

                        apiClientOptions.HttpClientTimeout = TimeSpan.FromSeconds(10);

                        apiClientOptions.TokenSiteSetting = new SiteSetting
                        {
                            SiteName = appSettings.SITE_TODO_SERVER_MAIN,
                            BaseUrl = new Uri(appSettings.SITE_TODO_SERVER_MAIN_BASE_URL)
                        };
                    },
                    fileManagerOptions =>
                    {
                        fileManagerOptions.AliyunOssEndpoint = appSettings.ALIYUN_OSS_ENDPOINT;
                        fileManagerOptions.AliyunOssBucketName = appSettings.ALIYUN_OSS_BUCKET_NAME;
                        fileManagerOptions.DirectoryDescriptions = sharedSettings.DirectoryDescriptions;
                        fileManagerOptions.DirectoryPermissions = sharedSettings.DirectoryPermissions;
                    },
                    clientOptions =>
                    {
                        clientOptions.AvatarDirectory = sharedSettings.DirectoryDescriptions.First(d => d.DirectoryName == "PUBLIC_AVATAR").ToDirectory(null);
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
                    tCaptchaAppId: appSettings.TECENET_CAPTCHA_APP_ID)
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