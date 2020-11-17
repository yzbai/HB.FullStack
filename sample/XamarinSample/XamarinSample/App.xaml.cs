using HB.Framework.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinSample.Services;
using XamarinSample.Views;

namespace XamarinSample
{
    public partial class App : Application
    {
        public static IDatabase Database { get; private set; }

        public static ITransaction Transaction { get; private set; }

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();

            (IDatabase, ITransaction) result  = GetDatabase();

            Database = result.Item1;
            Transaction = result.Item2;

            MainPage = new AppShell();
        }

        private (IDatabase, ITransaction) GetDatabase()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddOptions();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddDebug();
            });

            services.AddSQLite(sqliteOptions => {
                string dbFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test.db");

                sqliteOptions.CommonSettings.Version = 1;

                sqliteOptions.Connections.Add(new DatabaseConnectionSettings
                {
                    DatabaseName = "test.db",
                    IsMaster = true,
                    ConnectionString = $"Data Source={dbFile}"
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            IDatabase database = serviceProvider.GetRequiredService<IDatabase>();

            database.InitializeAsync();

            ITransaction transaction = serviceProvider.GetRequiredService<ITransaction>();

            return (database, transaction);
        }
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
