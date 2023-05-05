/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using Microsoft.Extensions.Configuration;

using System.IO;
using System.Reflection;

namespace Todo.Client.MobileApp
{
    public class AppSettings
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


        public static AppSettings GetAppSettings(string envrionment)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using Stream appsettingsStream = assembly.GetManifestResourceStream("Todo.Client.MobileApp.appsettings.json").ThrowIfNull("no appsettings.json");
            using Stream appsettingsEnvironmentStream = assembly.GetManifestResourceStream($"Todo.Client.MobileApp.appsettings.{envrionment}.json").ThrowIfNull($"no {envrionment} appsettings.json");

            var configBuilder = new ConfigurationBuilder()
                .AddJsonStream(appsettingsStream)
                .AddJsonStream(appsettingsEnvironmentStream);

            AppSettings appSettings = new AppSettings();

            configBuilder.Build().Bind(appSettings);

            return appSettings;
        }
    }
}