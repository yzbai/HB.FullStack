using HB.FullStack.Database;
using HB.FullStack.Database.SQL;
using HB.FullStack.DatabaseTests.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HB.FullStack.DatabaseTests
{
    public class SqliteBasicAsyncTest
    {
        private readonly IDatabase _sqlite;
        private readonly ITransaction _sqlIteTransaction;
        private readonly ITestOutputHelper _output;
        //private readonly IsolationLevel  = IsolationLevel.RepeatableRead;

        private IDatabase? GetDatabase(string databaseType)
        {

            return databaseType switch
            {
                "SQLite" => _sqlite,
                _ => null
            };
        }

        private ITransaction? GetTransaction(string databaseType)
        {

            return databaseType switch
            {
                "SQLite" => _sqlIteTransaction,
                _ => null
            };
        }


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="testOutputHelper"></param>
        /// <param name="serviceFixture"></param>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        public SqliteBasicAsyncTest(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;


            _sqlite = ServiceFixture.SQLite;
            _sqlIteTransaction = ServiceFixture.SQLiteTransaction;
        }

        /// <summary>
        /// Test_1_Batch_Add_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        /// <exception cref="Exception">Ignore.</exception>
        [Theory]

        [InlineData("SQLite")]
        public async Task Test_1_Batch_Add_PublisherEntityAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType)!;
            ITransaction transaction = GetTransaction(databaseType)!;

            IList<PublisherEntity> publishers = Mocker.GetPublishers();

            TransactionContext transactionContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                await database.BatchAddAsync<PublisherEntity>(publishers, "lastUsre", transactionContext);

                await transaction.CommitAsync(transactionContext);
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(transactionContext);
                throw ex;
            }
        }

        /// <summary>
        /// Test_2_Batch_Update_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Theory]

        [InlineData("SQLite")]
        public async Task Test_2_Batch_Update_PublisherEntityAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType)!;
            ITransaction transaction = GetTransaction(databaseType)!;
            TransactionContext transContext = await transaction.BeginTransactionAsync<PublisherEntity>();

            try
            {
                IEnumerable<PublisherEntity> lst = await database.RetrieveAllAsync<PublisherEntity>(transContext);

                for (int i = 0; i < lst.Count(); i += 2)
                {
                    PublisherEntity entity = lst.ElementAt(i);
                    //entity.Guid = Guid.NewGuid().ToString();
                    entity.Type = PublisherType.Online;
                    entity.Name = "中sfasfaf文名字";
                    entity.Books = new List<string>() { "xxx", "tttt" };
                    entity.BookAuthors = new Dictionary<string, Author>()
                    {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                };
                }

                await database.BatchUpdateAsync<PublisherEntity>(lst, "lastUsre", transContext);

                await transaction.CommitAsync(transContext);

            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(transContext);
                throw ex;
            }
        }

        /// <summary>
        /// Test_3_Batch_Delete_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        /// <exception cref="Exception">Ignore.</exception>
        [Theory]

        [InlineData("SQLite")]
        public async Task Test_3_Batch_Delete_PublisherEntityAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType)!;
            ITransaction transaction = GetTransaction(databaseType)!;
            TransactionContext transactionContext = await transaction.BeginTransactionAsync<PublisherEntity>();

            try
            {
                IList<PublisherEntity> lst = (await database.PageAsync<PublisherEntity>(2, 100, transactionContext)).ToList();

                if (lst.Count != 0)
                {
                    await database.BatchDeleteAsync<PublisherEntity>(lst, "lastUsre", transactionContext);

                }

                await transaction.CommitAsync(transactionContext);
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(transactionContext);
                throw ex;
            }
        }

        /// <summary>
        /// Test_4_Add_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        /// <exception cref="Exception">Ignore.</exception>
        [Theory]

        [InlineData("SQLite")]
        public async Task Test_4_Add_PublisherEntityAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType)!;
            ITransaction transaction = GetTransaction(databaseType)!;
            TransactionContext tContext = await transaction.BeginTransactionAsync<PublisherEntity>();

            try
            {
                IList<PublisherEntity> lst = new List<PublisherEntity>();

                for (int i = 0; i < 10; ++i)
                {
                    PublisherEntity entity = Mocker.MockOne();

                    await database.AddAsync(entity, "lastUsre", tContext);

                    lst.Add(entity);
                }

                await transaction.CommitAsync(tContext);

                Assert.True(lst.All(p => p.Id > 0));
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(tContext);
                throw ex;
            }
        }

        /// <summary>
        /// Test_5_Update_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        /// <exception cref="Exception">Ignore.</exception>
        [Theory]

        [InlineData("SQLite")]
        public async Task Test_5_Update_PublisherEntityAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType)!;
            ITransaction transaction = GetTransaction(databaseType)!;
            TransactionContext tContext = await transaction.BeginTransactionAsync<PublisherEntity>();

            try
            {
                IList<PublisherEntity> testEntities = (await database.PageAsync<PublisherEntity>(1, 1, tContext)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await database.UpdateAsync(entity, "lastUsre", tContext);

                PublisherEntity? stored = await database.ScalarAsync<PublisherEntity>(entity.Id, tContext);

                await transaction.CommitAsync(tContext);

                Assert.True(stored?.Books.Contains("New Book2"));
                //Assert.True(stored?.BookAuthors["New Book2"].Mobile == "15190208956");

            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(tContext);
                throw ex;
            }
        }

        /// <summary>
        /// Test_6_Delete_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        /// <exception cref="Exception">Ignore.</exception>
        [Theory]

        [InlineData("SQLite")]
        public async Task Test_6_Delete_PublisherEntityAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType)!;
            ITransaction transaction = GetTransaction(databaseType)!;
            TransactionContext tContext = await transaction.BeginTransactionAsync<PublisherEntity>();

            try
            {
                IList<PublisherEntity> testEntities = (await database.RetrieveAllAsync<PublisherEntity>(tContext)).ToList();

                foreach (var entity in testEntities)
                {
                    await database.DeleteAsync(entity, "lastUsre", tContext);
                }

                long count = await database.CountAsync<PublisherEntity>(tContext);

                await transaction.CommitAsync(tContext);

                Assert.True(count == 0);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(tContext);
                _output.WriteLine(ex.Message);
                throw ex;
            }
        }

        [Theory]

        [InlineData("SQLite")]
        public async Task Test_7_AddOrUpdate_PublisherEntityAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType)!;
            ITransaction transaction = GetTransaction(databaseType)!;
            TransactionContext tContext = await transaction.BeginTransactionAsync<PublisherEntity>();

            try
            {

                var publishers = Mocker.GetPublishers();

                var newIds = await database.BatchAddAsync(publishers, "xx", tContext);

                for (int i = 0; i < publishers.Count; i += 2)
                {
                    publishers[i].Name = "GGGGG" + i.ToString();

                }

                var affectedIds = await database.BatchAddOrUpdateAsync(publishers, "AddOrUpdaterrrr", tContext);


                //publishers[0].Guid = SecurityUtil.CreateUniqueToken();

                await database.AddOrUpdateAsync(publishers[0], "single", tContext);


                await transaction.CommitAsync(tContext);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(tContext);
                _output.WriteLine(ex.Message);
                throw ex;
            }
        }

        [Theory]

        [InlineData("SQLite")]
        public async Task Test_8_LastTimeTestAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType)!;

            PublisherEntity item = Mocker.MockOne();

            await database.AddAsync(item, "xx", null).ConfigureAwait(false);

            var fetched = await database.ScalarAsync<PublisherEntity>(item.Id, null);

            Assert.Equal(item.LastTime, fetched!.LastTime);

            fetched.Name = "ssssss";

            await database.UpdateAsync(fetched, "xxx", null);

            fetched = await database.ScalarAsync<PublisherEntity>(item.Id, null);

            await database.AddOrUpdateAsync(item, "ss", null);

            fetched = await database.ScalarAsync<PublisherEntity>(item.Id, null);



            //Batch

            var items = Mocker.GetPublishers();

            ITransaction transaction = GetTransaction(databaseType)!;

            TransactionContext trans = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                await database.BatchAddAsync<PublisherEntity>(items, "xx", trans).ConfigureAwait(false);


                var results = await database.RetrieveAsync<PublisherEntity>(item => SQLUtil.In(item.Guid, true, items.Select(item => item.Guid).ToArray()), trans).ConfigureAwait(false);

                await database.BatchUpdateAsync<PublisherEntity>(items, "xx", trans);

                var items2 = Mocker.GetPublishers();

                await database.BatchAddOrUpdateAsync<PublisherEntity>(items2, "xx", trans);

                results = await database.RetrieveAsync<PublisherEntity>(item => SQLUtil.In(item.Guid, true, items2.Select(item => item.Guid).ToArray()), trans);

                await database.BatchUpdateAsync<PublisherEntity>(items2, "xx", trans);


                await transaction.CommitAsync(trans);
            }
            catch
            {
                await transaction.RollbackAsync(trans);
                throw;
            }
            finally
            {

            }

        }

        [Theory]

        [InlineData("SQLite")]
        public async Task Test_9_UpdateLastTimeTestAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType)!;

            ITransaction transaction = GetTransaction(databaseType)!;

            TransactionContext transactionContext = await transaction.BeginTransactionAsync<PublisherEntity>();
            //TransactionContext? transactionContext = null;

            try
            {

                PublisherEntity item = Mocker.MockOne();


                await database.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);


                await database.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);


                await database.DeleteAsync(item, "xxx", transactionContext).ConfigureAwait(false);


                IList<PublisherEntity> testEntities = (await database.PageAsync<PublisherEntity>(1, 1, transactionContext)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await database.UpdateAsync(entity, "lastUsre", transactionContext);

                PublisherEntity? stored = await database.ScalarAsync<PublisherEntity>(entity.Id, transactionContext);



                item = Mocker.MockOne();

                await database.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                var fetched = await database.ScalarAsync<PublisherEntity>(item.Id, transactionContext);

                Assert.Equal(item.LastTime, fetched!.LastTime);

                fetched.Name = "ssssss";

                await database.UpdateAsync(fetched, "xxx", transactionContext);

                await transaction.CommitAsync(transactionContext);
            }
            catch
            {
                await transaction.RollbackAsync(transactionContext);
                throw;
            }

        }

        [Theory]

        [InlineData("SQLite")]
        public async Task Test_10_AddOrUpdate_VersionTestAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType)!;

            ITransaction transaction = GetTransaction(databaseType)!;

            TransactionContext transactionContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {

                PublisherEntity item = Mocker.MockOne();

                Assert.True(item.Version == -1);

                await database.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);

                Assert.True(item.Version == 0);

                await database.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);

                Assert.True(item.Version == 1);

                await database.UpdateAsync(item, "sfa", transactionContext).ConfigureAwait(false);

                Assert.True(item.Version == 2);



                IEnumerable<PublisherEntity> items = Mocker.GetPublishers();

                Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == -1));

                await database.BatchAddOrUpdateAsync(items, "xx", transactionContext);

                Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == 0));

                await database.BatchAddOrUpdateAsync(items, "xx", transactionContext);

                Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == 1));

                await database.BatchUpdateAsync(items, "ss", transactionContext).ConfigureAwait(false);

                Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == 2));


                //add
                item = Mocker.MockOne();

                Assert.True(item.Version == -1);

                await database.AddAsync(item, "sfas", transactionContext).ConfigureAwait(false);

                Assert.True(item.Version == 0);


                //batch add
                items = Mocker.GetPublishers();

                Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == -1));

                await database.BatchAddAsync(items, "xx", transactionContext);

                Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == 0));

                await transaction.CommitAsync(transactionContext);
            }
            catch
            {
                await transaction.RollbackAsync(transactionContext);
                throw;
            }

        }
    }
}
