using HB.Framework.Database;
using HB.Infrastructure.MySQL;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace DatabaseSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AssemblyLoadContext
            IDatabase database = GetDatabase();

            IList<BookEntity> books = MokeData.GetBooks();

            TransactionContext tContext = await database.BeginTransactionAsync<BookEntity>();

            DatabaseResult databaseResult = await database.BatchAddAsync(books, "", tContext);

            if (!databaseResult.IsSucceeded())
            {
                await database.RollbackAsync(tContext);
            }

            await database.CommitAsync(tContext);

            IEnumerable<BookEntity> retrieveResult = await database.RetrieveAllAsync<BookEntity>(null);

            foreach (BookEntity bookEntity in retrieveResult)
            {
                Console.WriteLine($"{bookEntity.Name}");
            }
        }

        private static IDatabase GetDatabase()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddMySQL(mySQLOptions => {
                mySQLOptions.DatabaseSettings.Version = 1;
                mySQLOptions.Schemas.Add(new SchemaInfo
                {
                    SchemaName = "test_db",
                    IsMaster = true,
                    ConnectionString = "server=127.0.0.1;port=3306;user=admin;password=_admin;database=test_db;SslMode=None"
                });
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            IDatabase database = serviceProvider.GetRequiredService<IDatabase>();

            database.Initialize();

            return database;
        }
    }
}
