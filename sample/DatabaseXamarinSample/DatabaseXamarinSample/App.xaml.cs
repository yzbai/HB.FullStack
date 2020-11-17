using HB.Framework.Database;
using HB.Infrastructure.SQLite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DatabaseXamarinSample
{
    public partial class App : Application
    {
        public static IDatabase Database { get; private set; }

        public App()
        {
            InitializeComponent();

            Database = GetDatabase();

            MainPage = new MainPage();
        }

        private IDatabase GetDatabase()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSQLite(sqliteOptions => {
                string dbFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test.db");

                sqliteOptions.DatabaseSettings.Version = 1;

                sqliteOptions.Schemas.Add(new SchemaInfo
                {
                    SchemaName = "test.db",
                    IsMaster = true,
                    ConnectionString = $"Data Source={dbFile}"
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            IDatabase database = serviceProvider.GetRequiredService<IDatabase>();

            database.Initialize();

            return database;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
