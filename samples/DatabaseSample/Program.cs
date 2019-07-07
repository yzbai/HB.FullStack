using HB.Framework.Database;
using HB.Infrastructure.MySQL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabaseSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
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
            MySQLOptions mySQLOptions = new MySQLOptions();

            mySQLOptions.DatabaseSettings.Version = 1;
            mySQLOptions.Schemas.Add(new SchemaInfo {
                SchemaName = "test_db",
                IsMaster = true,
                ConnectionString = "server=127.0.0.1;port=3306;user=admin;password=_admin;database=test_db;SslMode=None"
            });

            MySQLBuilder mySQLBuilder = new MySQLBuilder().SetMySqlOptions(mySQLOptions).Build();

            IDatabase database = new DatabaseBuilder()
                .SetDatabaseEngine(mySQLBuilder.DatabaseEngine)
                .SetDatabaseSettings(mySQLBuilder.DatabaseSettings)
                .Build();

            database.Initialize();

            return database;
        }
    }
}
