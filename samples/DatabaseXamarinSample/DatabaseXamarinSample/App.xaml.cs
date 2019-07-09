using HB.Framework.Database;
using HB.Infrastructure.SQLite;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DatabaseXamarinSample
{
    public partial class App : Application
    {
        public static IDatabase Database;

        public App()
        {
            InitializeComponent();

            Database = GetDatabase();

            MainPage = new MainPage();
        }

        private IDatabase GetDatabase()
        {
            string dbFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test.db");

            SQLiteOptions sqliteOptions = new SQLiteOptions();

            sqliteOptions.DatabaseSettings.Version = 1;

            sqliteOptions.Schemas.Add(new SchemaInfo {
                SchemaName = "test.db",
                IsMaster = true,
                ConnectionString = $"Data Source={dbFile}"
            });

            IDatabase database = new DatabaseBuilder(new SQLiteBuilder(sqliteOptions).Build()).Build();

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
