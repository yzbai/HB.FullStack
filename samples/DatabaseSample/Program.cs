using HB.Framework.Database;
using HB.Framework.Database.Transaction;
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
            Tuple<IDatabase, ITransaction> dbTuple = GetDatabaseAndTransaction();
            IDatabase database = dbTuple.Item1;
            ITransaction transaction = dbTuple.Item2;

            IList<BookEntity> books = MokeData.GetBooks();

            TransactionContext tContext = await transaction.BeginTransactionAsync<BookEntity>();

            DatabaseResult databaseResult = await database.BatchAddAsync(books, "", tContext);

            if (!databaseResult.IsSucceeded())
            {
                await transaction.RollbackAsync(tContext);
            }

            await transaction.CommitAsync(tContext);

            IEnumerable<BookEntity> retrieveResult = await database.RetrieveAllAsync<BookEntity>(null);

            foreach (BookEntity bookEntity in retrieveResult)
            {
                Console.WriteLine($"{bookEntity.Name}");
            }
        }

        private static Tuple<IDatabase, ITransaction> GetDatabaseAndTransaction()
        {
            MySQLOptions mySQLOptions = new MySQLOptions();

            mySQLOptions.DatabaseSettings.Version = 1;
            mySQLOptions.Schemas.Add(new SchemaInfo {
                SchemaName = "test_db",
                IsMaster = true,
                ConnectionString = "server=127.0.0.1;port=3306;user=admin;password=_admin;database=test_db;SslMode=None"
            });

            MySQLBuilder mySQLBuilder = new MySQLBuilder().SetMySqlOptions(mySQLOptions).Build();

            DatabaseBuilder databaseBuilder = new DatabaseBuilder()
                .SetDatabaseEngine(mySQLBuilder.DatabaseEngine)
                .SetDatabaseSettings(mySQLBuilder.DatabaseSettings)
                .Build();

            IDatabase database = databaseBuilder.Database;

            database.Initialize();

            return new Tuple<IDatabase, ITransaction>(database, databaseBuilder.Transaction);
        }
    }
}
