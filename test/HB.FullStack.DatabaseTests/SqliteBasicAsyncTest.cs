﻿using ClassLibrary1;

using HB.FullStack.Database;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.SQL;
using HB.FullStack.DatabaseTests.Data;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace HB.FullStack.DatabaseTests
{
    public class SqliteBasicAsyncTest : IClassFixture<ServiceFixture>
    {
        private readonly IDatabase _sqlite;
        private readonly ITransaction _sqlIteTransaction;
        private readonly ITestOutputHelper _output;
        //private readonly IsolationLevel  = IsolationLevel.RepeatableRead;
        private readonly IDatabaseEntityDefFactory _defFactory;

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
        public SqliteBasicAsyncTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            _output = testOutputHelper;


            _sqlite = serviceFixture.ServiceProvider2.GetRequiredService<IDatabase>();
            _sqlIteTransaction = serviceFixture.ServiceProvider2.GetRequiredService<ITransaction>();

            _defFactory = serviceFixture.ServiceProvider2.GetRequiredService<IDatabaseEntityDefFactory>();

            _sqlite.InitializeAsync().Wait();
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

        //[Theory]

        //[InlineData("SQLite")]
        //public async Task Test_7_AddOrUpdate_PublisherEntityAsync(string databaseType)
        //{
        //    IDatabase database = GetDatabase(databaseType)!;
        //    ITransaction transaction = GetTransaction(databaseType)!;
        //    TransactionContext tContext = await transaction.BeginTransactionAsync<PublisherEntity>();

        //    try
        //    {

        //        var publishers = Mocker.GetPublishers();

        //        var newIds = await database.BatchAddAsync(publishers, "xx", tContext);

        //        for (int i = 0; i < publishers.Count; i += 2)
        //        {
        //            publishers[i].Name = "GGGGG" + i.ToString();

        //        }

        //        var affectedIds = await database.BatchAddOrUpdateAsync(publishers, "AddOrUpdaterrrr", tContext);


        //        //publishers[0].Guid = SecurityUtil.CreateUniqueToken();

        //        await database.AddOrUpdateAsync(publishers[0], "single", tContext);


        //        await transaction.CommitAsync(tContext);
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync(tContext);
        //        _output.WriteLine(ex.Message);
        //        throw ex;
        //    }
        //}

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

            //await database.AddOrUpdateAsync(item, "ss", null);

            fetched = await database.ScalarAsync<PublisherEntity>(item.Id, null);



            //Batch

            var items = Mocker.GetPublishers();

            ITransaction transaction = GetTransaction(databaseType)!;

            TransactionContext trans = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                await database.BatchAddAsync<PublisherEntity>(items, "xx", trans).ConfigureAwait(false);


                var results = await database.RetrieveAsync<PublisherEntity>(item => SqlStatement.In(item.Guid, true, items.Select(item => item.Guid).ToArray()), trans).ConfigureAwait(false);

                await database.BatchUpdateAsync<PublisherEntity>(items, "xx", trans);

                var items2 = Mocker.GetPublishers();

                await database.BatchAddAsync<PublisherEntity>(items2, "xx", trans);

                results = await database.RetrieveAsync<PublisherEntity>(item => SqlStatement.In(item.Guid, true, items2.Select(item => item.Guid).ToArray()), trans);

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


                //await database.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);


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

        [Fact]
        public async Task Test_EntityMapperAsync()
        {
            IDatabase database = _sqlite;

            #region

            var publisher3 = new PublisherEntity3();

            await database.AddAsync(publisher3, "sss", null);

            var stored3 = await database.ScalarAsync<PublisherEntity3>(publisher3.Id, null);

            Assert.Equal(SerializeUtil.ToJson(publisher3), SerializeUtil.ToJson(stored3));

            #endregion

            #region 

            var publishers2 = Mocker.GetPublishers2();


            foreach (PublisherEntity2 publisher in publishers2)
            {
                await database.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }


            PublisherEntity2? publisher2 = await database.ScalarAsync<PublisherEntity2>(publishers2[0].Id, null).ConfigureAwait(false);

            Assert.Equal(SerializeUtil.ToJson(publisher2), SerializeUtil.ToJson(publishers2[0]));

            #endregion

            #region 

            var publishers = Mocker.GetPublishers();


            foreach (PublisherEntity publisher in publishers)
            {
                await database.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }


            PublisherEntity? publisher1 = await database.ScalarAsync<PublisherEntity>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.Equal(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));
            #endregion
        }

        [Theory]
        [InlineData(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "<Pending>")]
        public async Task Test_EntityMapperPerformanceAsync(int index)
        {
            index++;



            IDatabase database = _sqlite;

            var books = Mocker.GetBooks(500);

            var trans = await _sqlIteTransaction.BeginTransactionAsync<BookEntity>().ConfigureAwait(false);

            IEnumerable<BookEntity> re = await database.RetrieveAsync<BookEntity>(b => b.Deleted, trans);

            await database.AddAsync<BookEntity>(Mocker.GetBooks(1)[0], "", trans);

            try
            {

                //await database.AddAsync<BookEntity>(books[0], "", trans);

                await database.BatchAddAsync(books, "x", trans).ConfigureAwait(false);
                await _sqlIteTransaction.CommitAsync(trans).ConfigureAwait(false);
            }
            catch
            {
                await _sqlIteTransaction.RollbackAsync(trans).ConfigureAwait(false);
            }


            Stopwatch stopwatch = new Stopwatch();

            SqliteConnection mySqlConnection = new SqliteConnection("Data Source=sqlite_test2.db");



            //time = 0;
            int loop = 10;

            TimeSpan time0 = TimeSpan.Zero, time1 = TimeSpan.Zero, time2 = TimeSpan.Zero, time3 = TimeSpan.Zero;
            for (int cur = 0; cur < loop; ++cur)
            {

                await mySqlConnection.OpenAsync();


                SqliteCommand command0 = new SqliteCommand("select * from tb_book limit 1000", mySqlConnection);

                var reader0 = await command0.ExecuteReaderAsync().ConfigureAwait(false);

                List<BookEntity> list1 = new List<BookEntity>();
                List<BookEntity> list2 = new List<BookEntity>();
                List<BookEntity> list3 = new List<BookEntity>();

                int len = reader0.FieldCount;
                DatabaseEntityPropertyDef[] propertyDefs = new DatabaseEntityPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                DatabaseEntityDef definition = _defFactory.GetDef<BookEntity>();

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetPropertyDef(reader0.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }


                Func<IDataReader, DatabaseEntityDef, object> mapper1 = EntityMapperCreator.CreateEntityMapper(definition, reader0, 0, definition.FieldCount, false);


                //Warning: 如果用Dapper，小心DateTimeOffset的存储，会丢失offset，然后转回来时候，会加上当地时间的offset
                TypeHandlerHelper.AddTypeHandlerImpl(typeof(DateTimeOffset), new DateTimeOffsetTypeHandler(), false);
                Func<IDataReader, object> mapper2 = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(BookEntity), reader0);



                Stopwatch stopwatch1 = new Stopwatch();
                Stopwatch stopwatch2 = new Stopwatch();
                Stopwatch stopwatch3 = new Stopwatch();




                while (reader0.Read())
                {
                    stopwatch1.Start();

                    object obj1 = mapper1(reader0, definition);

                    list1.Add((BookEntity)obj1);
                    stopwatch1.Stop();



                    stopwatch2.Start();
                    object obj2 = mapper2(reader0);

                    list2.Add((BookEntity)obj2);
                    stopwatch2.Stop();


                    stopwatch3.Start();

                    BookEntity item = new BookEntity();

                    for (int i = 0; i < len; ++i)
                    {
                        DatabaseEntityPropertyDef property = propertyDefs[i];

                        object? value = TypeConvert.DbValueToTypeValue(reader0[i], property);


                        setMethods[i].Invoke(item, new object?[] { value });

                    }

                    list3.Add(item);

                    stopwatch3.Stop();


                }

                time1 += stopwatch1.Elapsed;
                time2 += stopwatch2.Elapsed;
                time3 += stopwatch3.Elapsed;

                await reader0.DisposeAsync().ConfigureAwait(false);
                command0.Dispose();

                await mySqlConnection.CloseAsync();

            }

            _output.WriteLine("Emit Coding : " + (time1.TotalMilliseconds / (loop * 1.0)).ToString());
            _output.WriteLine("Dapper : " + (time2.TotalMilliseconds / (loop * 1.0)).ToString());
            _output.WriteLine("Reflection : " + (time3.TotalMilliseconds / (loop * 1.0)).ToString());

        }

        //[Theory]

        //[InlineData("SQLite")]
        //public async Task Test_10_AddOrUpdate_VersionTestAsync(string databaseType)
        //{
        //    IDatabase database = GetDatabase(databaseType)!;

        //    ITransaction transaction = GetTransaction(databaseType)!;

        //    TransactionContext transactionContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

        //    try
        //    {

        //        PublisherEntity item = Mocker.MockOne();

        //        Assert.True(item.Version == -1);

        //        await database.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);

        //        Assert.True(item.Version == 0);

        //        await database.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);

        //        Assert.True(item.Version == 1);

        //        await database.UpdateAsync(item, "sfa", transactionContext).ConfigureAwait(false);

        //        Assert.True(item.Version == 2);



        //        IEnumerable<PublisherEntity> items = Mocker.GetPublishers();

        //        Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == -1));

        //        await database.BatchAddOrUpdateAsync(items, "xx", transactionContext);

        //        Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == 0));

        //        await database.BatchAddOrUpdateAsync(items, "xx", transactionContext);

        //        Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == 1));

        //        await database.BatchUpdateAsync(items, "ss", transactionContext).ConfigureAwait(false);

        //        Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == 2));


        //        //add
        //        item = Mocker.MockOne();

        //        Assert.True(item.Version == -1);

        //        await database.AddAsync(item, "sfas", transactionContext).ConfigureAwait(false);

        //        Assert.True(item.Version == 0);


        //        //batch add
        //        items = Mocker.GetPublishers();

        //        Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == -1));

        //        await database.BatchAddAsync(items, "xx", transactionContext);

        //        Assert.All<PublisherEntity>(items, item => Assert.True(item.Version == 0));

        //        await transaction.CommitAsync(transactionContext);
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync(transactionContext);
        //        throw;
        //    }

        //}
    }
}