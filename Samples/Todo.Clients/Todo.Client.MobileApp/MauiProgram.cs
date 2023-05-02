/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.IO;
using System.Reflection;

using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.MauiLib.Components;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.LifecycleEvents;

using Todo.Shared;

namespace Todo.Client.MobileApp
{
    public class ConstantSettings
    {
        public string SITE_TODO_SERVER_MAIN { get; set; } = null!;
        public string SITE_TODO_SERVER_MAIN_BASE_URL { get; set; } = null!;
        public string DB_SCHEMA_MAIN { get; set; } = null!;
        public string DB_SCHEMA_MAIN_FILE { get; set; } = null!;
        public string DB_SCHEMA_USER { get; set; } = null!;
        public string DB_SCHEMA_USER_FILE { get; set; } = null!;
        public string ALIYUN_OSS_ENDPOINT { get; set; } = null!;
        public string ALIYUN_OSS_BUCKET_NAME { get; set; } = null!;
        public string TECENET_CAPTCHA_APP_ID { get; set; } = null!;
    }

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
            ConstantSettings constantSettings = GetConstantSettings();

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
                            Name = constantSettings.DB_SCHEMA_MAIN,
                            EngineType = DbEngineType.SQLite,
                            Version = 1,
                            ConnectionString = new ConnectionString($"Data Source={Path.Combine(Currents.AppDataDirectory, constantSettings.DB_SCHEMA_MAIN_FILE)}")
                        });
                        dbOptions.DbSchemas.Add(new DbSchema
                        {
                            Name = constantSettings.DB_SCHEMA_USER,
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
                            SiteName = constantSettings.SITE_TODO_SERVER_MAIN,
                            BaseUrl = new Uri(constantSettings.SITE_TODO_SERVER_MAIN_BASE_URL)
                        };
                    },
                    fileManagerOptions =>
                    {
                        fileManagerOptions.AliyunOssEndpoint = constantSettings.ALIYUN_OSS_ENDPOINT;
                        fileManagerOptions.AliyunOssBucketName = constantSettings.ALIYUN_OSS_BUCKET_NAME;
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
                    tCaptchaAppId: constantSettings.TECENET_CAPTCHA_APP_ID)
                .UseTodoApp(todoAppOptions => { })
                .ConfigureLifecycleEvents(lifecycleBuilder => { })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            return builder.Build();

            static ConstantSettings GetConstantSettings()
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                using Stream appsettingsStream = assembly.GetManifestResourceStream("Todo.Client.MobileApp.appsettings.json").ThrowIfNull("no appsettings.json");
                using Stream? appsettingsDevelopmentStream = Currents.IsDebug ?
                    assembly.GetManifestResourceStream("Todo.Client.MobileApp.appsettings.Debug.json") :
                    assembly.GetManifestResourceStream("Todo.Client.MobileApp.appsettings.Release.json");

                var configBuilder = new ConfigurationBuilder().AddJsonStream(appsettingsStream);

                if (appsettingsDevelopmentStream != null)
                {
                    configBuilder.AddJsonStream(appsettingsDevelopmentStream);
                }

                ConstantSettings configSettings = new ConstantSettings();

                configBuilder.Build().Bind(configSettings);

                return configSettings;
            }
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