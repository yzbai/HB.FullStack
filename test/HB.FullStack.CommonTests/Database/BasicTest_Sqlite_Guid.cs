using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ClassLibrary1;
using HB.FullStack.Database;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;
using HB.FullStack.DatabaseTests.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

using Xunit;
using Xunit.Abstractions;

namespace HB.FullStack.DatabaseTests
{
    public class BasicTest_Sqlite_Guid : IClassFixture<ServiceFixture_Sqlite>
    {
        private readonly IDatabase _sqlite;
        private readonly ITransaction _sqlIteTransaction;
        private readonly ITestOutputHelper _output;
        //private readonly IsolationLevel  = IsolationLevel.RepeatableRead;




        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="testOutputHelper"></param>
        /// <param name="serviceFixture"></param>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        public BasicTest_Sqlite_Guid(ITestOutputHelper testOutputHelper, ServiceFixture_Sqlite serviceFixture)
        {
            _output = testOutputHelper;


            _sqlite = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();
            _sqlIteTransaction = serviceFixture.ServiceProvider.GetRequiredService<ITransaction>();


            _sqlite.InitializeAsync().Wait();
        }

        /// <summary>
        /// Test_1_Batch_Add_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]
        public async Task Test_1_Batch_Add_PublisherEntityAsync()
        {
            IDatabase database = _sqlite;
            ITransaction transaction = _sqlIteTransaction;

            IList<Guid_PublisherEntity_Client> publishers = Mocker.Guid_GetPublishers_Client();

            TransactionContext transactionContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                await database.BatchAddAsync(publishers, "lastUsre", transactionContext).ConfigureAwait(false);

                await transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Test_2_Batch_Update_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]
        public async Task Test_2_Batch_Update_PublisherEntityAsync()
        {
            IDatabase database = _sqlite;
            ITransaction transaction = _sqlIteTransaction;
            TransactionContext transContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                IEnumerable<Guid_PublisherEntity_Client> lst = await database.RetrieveAllAsync<Guid_PublisherEntity_Client>(transContext).ConfigureAwait(false);

                for (int i = 0; i < lst.Count(); i += 2)
                {
                    Guid_PublisherEntity_Client entity = lst.ElementAt(i);
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

                await database.BatchUpdateAsync<Guid_PublisherEntity_Client>(lst, "lastUsre", transContext).ConfigureAwait(false);

                await transaction.CommitAsync(transContext).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(transContext).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Test_3_Batch_Delete_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]
        public async Task Test_3_Batch_Delete_PublisherEntityAsync()
        {
            IDatabase database = _sqlite;
            ITransaction transaction = _sqlIteTransaction;
            TransactionContext transactionContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity_Client> lst = (await database.PageAsync<Guid_PublisherEntity_Client>(2, 100, transactionContext).ConfigureAwait(false)).ToList();

                if (lst.Count != 0)
                {
                    await database.BatchDeleteAsync<Guid_PublisherEntity_Client>(lst, "lastUsre", transactionContext).ConfigureAwait(false);

                }

                await transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Test_4_Add_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]


        public async Task Test_4_Add_PublisherEntityAsync()
        {
            IDatabase database = _sqlite;
            ITransaction transaction = _sqlIteTransaction;
            TransactionContext tContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity_Client> lst = new List<Guid_PublisherEntity_Client>();

                for (int i = 0; i < 10; ++i)
                {
                    Guid_PublisherEntity_Client entity = Mocker.Guid_MockOnePublisherEntity_Client();

                    await database.AddAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                    lst.Add(entity);
                }

                await transaction.CommitAsync(tContext).ConfigureAwait(false);

                Assert.True(lst.All(p => p.Version == 0));
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(tContext).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Test_5_Update_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]


        public async Task Test_5_Update_PublisherEntityAsync()
        {
            IDatabase database = _sqlite;
            ITransaction transaction = _sqlIteTransaction;
            TransactionContext tContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity_Client> testEntities = (await database.PageAsync<Guid_PublisherEntity_Client>(1, 1, tContext).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception( "No Entity to update");
                }

                Guid_PublisherEntity_Client entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await database.UpdateAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                Guid_PublisherEntity_Client? stored = await database.ScalarAsync<Guid_PublisherEntity_Client>(entity.Id, tContext).ConfigureAwait(false);

                await transaction.CommitAsync(tContext).ConfigureAwait(false);

                Assert.True(stored?.Books.Contains("New Book2"));
                //Assert.True(stored?.BookAuthors["New Book2"].Mobile == "15190208956");

            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(tContext).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Test_6_Delete_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]
        public async Task Test_6_Delete_PublisherEntityAsync()
        {
            IDatabase database = _sqlite;
            ITransaction transaction = _sqlIteTransaction;
            TransactionContext tContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity_Client> testEntities = (await database.RetrieveAllAsync<Guid_PublisherEntity_Client>(tContext).ConfigureAwait(false)).ToList();

                foreach (Guid_PublisherEntity_Client? entity in testEntities)
                {
                    await database.DeleteAsync(entity, "lastUsre", tContext).ConfigureAwait(false);
                }

                long count = await database.CountAsync<Guid_PublisherEntity_Client>(tContext).ConfigureAwait(false);

                await transaction.CommitAsync(tContext).ConfigureAwait(false);

                Assert.True(count == 0);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(tContext).ConfigureAwait(false);
                _output.WriteLine(ex.Message);
                throw;
            }
        }

        //[Fact]

        //
        //public async Task Test_7_AddOrUpdate_PublisherEntityAsync()
        //{
        //    IDatabase database = GetDatabase(databaseType)!;
        //    ITransaction transaction = GetTransaction(databaseType)!;
        //    TransactionContext tContext = await transaction.BeginTransactionAsync<PublisherEntity_Client>();

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

        /// <summary>
        /// Test_8_LastTimeTestAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [Fact]


        public async Task Test_8_LastTimeTestAsync()
        {
            IDatabase database = _sqlite;
            ITransaction transaction = _sqlIteTransaction;

            Guid_PublisherEntity_Client item = Mocker.Guid_MockOnePublisherEntity_Client();

            await database.AddAsync(item, "xx", null).ConfigureAwait(false);

            Guid_PublisherEntity_Client? fetched = await database.ScalarAsync<Guid_PublisherEntity_Client>(item.Id, null).ConfigureAwait(false);

            Assert.Equal(item.LastTime, fetched!.LastTime);

            fetched.Name = "ssssss";

            await database.UpdateAsync(fetched, "xxx", null).ConfigureAwait(false);

            fetched = await database.ScalarAsync<Guid_PublisherEntity_Client>(item.Id, null).ConfigureAwait(false);

            //await database.AddOrUpdateAsync(item, "ss", null);

            fetched = await database.ScalarAsync<Guid_PublisherEntity_Client>(item.Id, null).ConfigureAwait(false);



            //Batch

            List<Guid_PublisherEntity_Client>? items = Mocker.Guid_GetPublishers_Client();



            TransactionContext trans = await transaction.BeginTransactionAsync<Guid_PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                await database.BatchAddAsync<Guid_PublisherEntity_Client>(items, "xx", trans).ConfigureAwait(false);


                IEnumerable<Guid_PublisherEntity_Client>? results = await database.RetrieveAsync<Guid_PublisherEntity_Client>(item => SqlStatement.In(item.Id, true, items.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await database.BatchUpdateAsync<Guid_PublisherEntity_Client>(items, "xx", trans).ConfigureAwait(false);

                List<Guid_PublisherEntity_Client>? items2 = Mocker.Guid_GetPublishers_Client();

                await database.BatchAddAsync<Guid_PublisherEntity_Client>(items2, "xx", trans).ConfigureAwait(false);

                results = await database.RetrieveAsync<Guid_PublisherEntity_Client>(item => SqlStatement.In(item.Id, true, items2.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await database.BatchUpdateAsync<Guid_PublisherEntity_Client>(items2, "xx", trans).ConfigureAwait(false);


                await transaction.CommitAsync(trans).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
            finally
            {

            }

        }

        /// <summary>
        /// Test_9_UpdateLastTimeTestAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]
        public async Task Test_9_UpdateLastTimeTestAsync()
        {
            IDatabase database = _sqlite;
            ITransaction transaction = _sqlIteTransaction;

            TransactionContext transactionContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity_Client>().ConfigureAwait(false);
            //TransactionContext? transactionContext = null;

            try
            {

                Guid_PublisherEntity_Client item = Mocker.Guid_MockOnePublisherEntity_Client();


                await database.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);


                //await database.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);


                await database.DeleteAsync(item, "xxx", transactionContext).ConfigureAwait(false);


                IList<Guid_PublisherEntity_Client> testEntities = (await database.PageAsync<Guid_PublisherEntity_Client>(1, 1, transactionContext).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new  Exception("No Entity to update");
                }

                Guid_PublisherEntity_Client entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await database.UpdateAsync(entity, "lastUsre", transactionContext).ConfigureAwait(false);

                Guid_PublisherEntity_Client? stored = await database.ScalarAsync<Guid_PublisherEntity_Client>(entity.Id, transactionContext).ConfigureAwait(false);



                item = Mocker.Guid_MockOnePublisherEntity_Client();

                await database.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                Guid_PublisherEntity_Client? fetched = await database.ScalarAsync<Guid_PublisherEntity_Client>(item.Id, transactionContext).ConfigureAwait(false);

                Assert.Equal(item.LastTime, fetched!.LastTime);

                fetched.Name = "ssssss";

                await database.UpdateAsync(fetched, "xxx", transactionContext).ConfigureAwait(false);

                await transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

        }

        /// <summary>
        /// Test_EntityMapperAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [Fact]
        public async Task Test_EntityMapperAsync()
        {
            IDatabase database = _sqlite;

            #region

            PublisherEntity3_Client? publisher3 = new PublisherEntity3_Client();

            await database.AddAsync(publisher3, "sss", null).ConfigureAwait(false);

            PublisherEntity3_Client? stored3 = await database.ScalarAsync<PublisherEntity3_Client>(publisher3.Id, null).ConfigureAwait(false);

            Assert.Equal(SerializeUtil.ToJson(publisher3), SerializeUtil.ToJson(stored3));

            #endregion

            #region 

            IList<PublisherEntity2_Client>? publishers2 = Mocker.GetPublishers2_Client();


            foreach (PublisherEntity2_Client publisher in publishers2)
            {
                await database.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }


            PublisherEntity2_Client? publisher2 = await database.ScalarAsync<PublisherEntity2_Client>(publishers2[0].Id, null).ConfigureAwait(false);

            Assert.Equal(SerializeUtil.ToJson(publisher2), SerializeUtil.ToJson(publishers2[0]));

            #endregion

            #region 

            List<PublisherEntity_Client>? publishers = Mocker.GetPublishers_Client();


            foreach (PublisherEntity_Client publisher in publishers)
            {
                await database.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }


            PublisherEntity_Client? publisher1 = await database.ScalarAsync<PublisherEntity_Client>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.Equal(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));
            #endregion
        }

        /// <summary>
        /// Test_EntityMapperPerformanceAsync
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [Theory]
        [InlineData(1)]
        public async Task Test_EntityMapperPerformanceAsync(int index)
        {
            index++;



            IDatabase database = _sqlite;

            IList<BookEntity_Client>? books = Mocker.GetBooks_Client(500);

            TransactionContext? trans = await _sqlIteTransaction.BeginTransactionAsync<BookEntity_Client>().ConfigureAwait(false);

            IEnumerable<BookEntity_Client> re = await database.RetrieveAsync<BookEntity_Client>(b => b.Deleted, trans).ConfigureAwait(false);

            await database.AddAsync<BookEntity_Client>(Mocker.GetBooks_Client(1)[0], "", trans).ConfigureAwait(false);

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

            using SqliteConnection mySqlConnection = new SqliteConnection("Data Source=sqlite_test2.db");



            //time = 0;
            int loop = 10;

            TimeSpan time0 = TimeSpan.Zero, time1 = TimeSpan.Zero, time2 = TimeSpan.Zero, time3 = TimeSpan.Zero;
            for (int cur = 0; cur < loop; ++cur)
            {

                await mySqlConnection.OpenAsync().ConfigureAwait(false);


                using SqliteCommand command0 = new SqliteCommand("select * from tb_bookentity_client limit 1000", mySqlConnection);

                SqliteDataReader? reader0 = await command0.ExecuteReaderAsync().ConfigureAwait(false);

                List<BookEntity_Client> list1 = new List<BookEntity_Client>();
                List<BookEntity_Client> list2 = new List<BookEntity_Client>();
                List<BookEntity_Client> list3 = new List<BookEntity_Client>();

                int len = reader0.FieldCount;
                EntityPropertyDef[] propertyDefs = new EntityPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                EntityDef definition = EntityDefFactory.GetDef<BookEntity_Client>()!;

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetPropertyDef(reader0.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }


                Func<IDataReader, object> mapper1 = EntityMapperDelegateCreator.CreateToEntityDelegate(definition, reader0, 0, definition.FieldCount, false, Database.Engine.EngineType.SQLite);


                //Warning: 如果用Dapper，小心DateTimeOffset的存储，会丢失offset，然后转回来时候，会加上当地时间的offset
                TypeHandlerHelper.AddTypeHandlerImpl(typeof(DateTimeOffset), new DateTimeOffsetTypeHandler(), false);
                Func<IDataReader, object> mapper2 = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(BookEntity_Client), reader0);



                Stopwatch stopwatch1 = new Stopwatch();
                Stopwatch stopwatch2 = new Stopwatch();
                Stopwatch stopwatch3 = new Stopwatch();

                while (reader0.Read())
                {
                    stopwatch1.Start();

                    object obj1 = mapper1(reader0);

                    list1.Add((BookEntity_Client)obj1);
                    stopwatch1.Stop();



                    stopwatch2.Start();
                    object obj2 = mapper2(reader0);

                    list2.Add((BookEntity_Client)obj2);
                    stopwatch2.Stop();


                    stopwatch3.Start();

                    BookEntity_Client item = new BookEntity_Client();

                    for (int i = 0; i < len; ++i)
                    {
                        EntityPropertyDef property = propertyDefs[i];

                        object? value = TypeConvert.DbValueToTypeValue(reader0[i], property, Database.Engine.EngineType.SQLite);

                        if (value != null)
                        {
                            setMethods[i].Invoke(item, new object?[] { value });
                        }

                    }

                    list3.Add(item);

                    stopwatch3.Stop();


                }

                time1 += stopwatch1.Elapsed;
                time2 += stopwatch2.Elapsed;
                time3 += stopwatch3.Elapsed;

                await reader0.DisposeAsync().ConfigureAwait(false);
                command0.Dispose();

                await mySqlConnection.CloseAsync().ConfigureAwait(false);

            }

            _output.WriteLine("Emit Coding : " + (time1.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            _output.WriteLine("Dapper : " + (time2.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            _output.WriteLine("Reflection : " + (time3.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));

        }

        [Fact]
        public async Task Test_10_Enum_TestAsync()
        {
            IDatabase database = _sqlite;
            ITransaction transaction = _sqlIteTransaction;

            IList<Guid_PublisherEntity> publishers = Mocker.Guid_GetPublishers();

            TransactionContext transactionContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                await database.BatchAddAsync(publishers, "lastUsre", transactionContext).ConfigureAwait(false);

                await transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                await transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            IEnumerable<Guid_PublisherEntity> publisherEntities = await database.RetrieveAsync<Guid_PublisherEntity>(p => p.Type == PublisherType.Big, null).ConfigureAwait(false);

            Assert.True(publisherEntities.All(p => p.Type == PublisherType.Big));
        }

        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "<Pending>")]
        public void TestSQLite_Changes_Test()
        {
            string connectString = $"Data Source=sqlite_test2.db";
            using SqliteConnection conn = new SqliteConnection(connectString);
            conn.Open();

            string guid = SecurityUtil.CreateUniqueToken();

            string insertCommandText = $"insert into tb_publisher(`Name`, `LastTime`, `Guid`, `Version`) values('SSFS', 100, '{guid}', 1)";

            using SqliteCommand insertCommand = new SqliteCommand(insertCommandText, conn);

            insertCommand.ExecuteScalar();


            string commandText = $"update `tb_publisher` set  `Name`='{new Random().NextDouble()}', `Version`=2 WHERE `Guid`='{guid}' ;";

            using SqliteCommand mySqlCommand1 = new SqliteCommand(commandText, conn);

            int rt1 = mySqlCommand1.ExecuteNonQuery();

            using SqliteCommand rowCountCommand1 = new SqliteCommand("select changes()", conn);

            long? rowCount1 = (long?)rowCountCommand1.ExecuteScalar();

            using SqliteCommand mySqlCommand2 = new SqliteCommand(commandText, conn);

            int rt2 = mySqlCommand1.ExecuteNonQuery();

            using SqliteCommand rowCountCommand2 = new SqliteCommand("select changes()", conn);

            long? rowCount2 = (long?)rowCountCommand2.ExecuteScalar();

            Assert.Equal(rt1, rt2);
            Assert.Equal(rowCount1, rowCount2);
        }

    }
}
