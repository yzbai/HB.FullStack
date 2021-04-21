using ClassLibrary1;

using HB.FullStack.Database;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;
using HB.FullStack.DatabaseTests.Data;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MySqlConnector;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace HB.FullStack.DatabaseTests
{
    //[TestCaseOrderer("HB.FullStack.Database.Test.TestCaseOrdererByTestName", "HB.FullStack.Database.Test")]
    public class MySQLBasicAsyncTest : IClassFixture<ServiceFixture_MySql>
    {
        private readonly IDatabase _mysql;
        private readonly ITransaction _mysqlTransaction;
        private readonly ITestOutputHelper _output;
        private readonly string _mysqlConnectionString;

        public MySQLBasicAsyncTest(ITestOutputHelper testOutputHelper, ServiceFixture_MySql serviceFixture)
        {
            TestCls testCls = serviceFixture.ServiceProvider.GetRequiredService<TestCls>();

            _output = testOutputHelper;

            _mysql = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();
            _mysqlTransaction = serviceFixture.ServiceProvider.GetRequiredService<ITransaction>();

            _mysqlConnectionString = serviceFixture.Configuration["MySQL:Connections:0:ConnectionString"];
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
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;

            IList<PublisherEntity> publishers = Mocker.GetPublishers();

            TransactionContext transactionContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

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
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext transContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                IEnumerable<PublisherEntity> lst = await database.RetrieveAllAsync<PublisherEntity>(transContext).ConfigureAwait(false);

                for (int i = 0; i < lst.Count(); i += 2)
                {
                    PublisherEntity entity = lst.ElementAt(i);
                    //entity.Guid = Guid.NewGuid().ToString();
                    entity.Type = PublisherType.Online;
                    entity.Name = "��sfasfaf������";
                    entity.Books = new List<string>() { "xxx", "tttt" };
                    entity.BookAuthors = new Dictionary<string, Author>()
                    {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                };
                }

                await database.BatchUpdateAsync(lst, "lastUsre", transContext).ConfigureAwait(false);

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
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext transactionContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity> lst = (await database.PageAsync<PublisherEntity>(2, 100, transactionContext).ConfigureAwait(false)).ToList();

                if (lst.Count != 0)
                {
                    await database.BatchDeleteAsync(lst, "lastUsre", transactionContext).ConfigureAwait(false);

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
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext tContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity> lst = new List<PublisherEntity>();

                for (int i = 0; i < 10; ++i)
                {
                    PublisherEntity entity = Mocker.MockOnePublisherEntity();

                    await database.AddAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                    lst.Add(entity);
                }

                await transaction.CommitAsync(tContext).ConfigureAwait(false);

                Assert.True(lst.All(p => p.Id > 0));
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
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext tContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity> testEntities = (await database.PageAsync<PublisherEntity>(1, 1, tContext).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await database.UpdateAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                PublisherEntity? stored = await database.ScalarAsync<PublisherEntity>(entity.Id, tContext).ConfigureAwait(false);

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
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext tContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity> testEntities = (await database.RetrieveAllAsync<PublisherEntity>(tContext).ConfigureAwait(false)).ToList();

                foreach (var entity in testEntities)
                {
                    await database.DeleteAsync(entity, "lastUsre", tContext).ConfigureAwait(false);
                }

                long count = await database.CountAsync<PublisherEntity>(tContext).ConfigureAwait(false);

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


        //public async Task Test_7_AddOrUpdate_PublisherEntityAsync()
        //{
        //    IDatabase database = _mysql;
        //    ITransaction transaction = _mysqlTransaction;
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

        /// <summary>
        /// Test_8_LastTimeTestAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [Fact]


        public async Task Test_8_LastTimeTestAsync()
        {
            IDatabase database = _mysql;

            PublisherEntity item = Mocker.MockOnePublisherEntity();

            await database.AddAsync(item, "xx", null).ConfigureAwait(false);

            var fetched = await database.ScalarAsync<PublisherEntity>(item.Id, null).ConfigureAwait(false);

            Assert.Equal(item.LastTime, fetched!.LastTime);

            fetched.Name = "ssssss";

            await database.UpdateAsync(fetched, "xxx", null).ConfigureAwait(false);

            fetched = await database.ScalarAsync<PublisherEntity>(item.Id, null).ConfigureAwait(false);

            //await database.AddOrUpdateAsync(item, "ss", null);

            fetched = await database.ScalarAsync<PublisherEntity>(item.Id, null).ConfigureAwait(false);



            //Batch

            var items = Mocker.GetPublishers();

            ITransaction transaction = _mysqlTransaction;

            TransactionContext trans = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                await database.BatchAddAsync(items, "xx", trans).ConfigureAwait(false);


                var results = await database.RetrieveAsync<PublisherEntity>(item => SqlStatement.In(item.Id, true, items.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await database.BatchUpdateAsync(items, "xx", trans).ConfigureAwait(false);

                var items2 = Mocker.GetPublishers();

                await database.BatchAddAsync(items2, "xx", trans).ConfigureAwait(false);

                results = await database.RetrieveAsync<PublisherEntity>(item => SqlStatement.In(item.Id, true, items2.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await database.BatchUpdateAsync(items2, "xx", trans).ConfigureAwait(false);


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
            IDatabase database = _mysql;

            ITransaction transaction = _mysqlTransaction;

            TransactionContext transactionContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);
            //TransactionContext? transactionContext = null;

            try
            {

                PublisherEntity item = Mocker.MockOnePublisherEntity();


                await database.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);


                //await database.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);


                await database.DeleteAsync(item, "xxx", transactionContext).ConfigureAwait(false);


                IList<PublisherEntity> testEntities = (await database.PageAsync<PublisherEntity>(1, 1, transactionContext).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await database.UpdateAsync(entity, "lastUsre", transactionContext).ConfigureAwait(false);

                PublisherEntity? stored = await database.ScalarAsync<PublisherEntity>(entity.Id, transactionContext).ConfigureAwait(false);



                item = Mocker.MockOnePublisherEntity();

                await database.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                var fetched = await database.ScalarAsync<PublisherEntity>(item.Id, transactionContext).ConfigureAwait(false);

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
            GlobalSettings.Logger.LogInformation($"��ǰProcess,{Environment.ProcessId}");

            IDatabase database = _mysql;

            #region

            var publisher3 = new PublisherEntity3();

            await database.AddAsync(publisher3, "sss", null).ConfigureAwait(false);

            var stored3 = await database.ScalarAsync<PublisherEntity3>(publisher3.Id, null).ConfigureAwait(false);

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
            IDatabase database = _mysql;

            var books = Mocker.GetBooks(5000);

            var trans = await _mysqlTransaction.BeginTransactionAsync<BookEntity>().ConfigureAwait(false);

            IEnumerable<BookEntity> re = await database.RetrieveAsync<BookEntity>(b => b.Deleted, trans).ConfigureAwait(false);

            await database.AddAsync(Mocker.GetBooks(1)[0], "", trans).ConfigureAwait(false);

            try
            {

                //await database.AddAsync<BookEntity>(books[0], "", trans);

                await database.BatchAddAsync(books, "x", trans).ConfigureAwait(false);
                await _mysqlTransaction.CommitAsync(trans).ConfigureAwait(false);
            }
            catch
            {
                await _mysqlTransaction.RollbackAsync(trans).ConfigureAwait(false);
            }


            Stopwatch stopwatch = new Stopwatch();

            using MySqlConnection mySqlConnection = new MySqlConnection(_mysqlConnectionString);

            TypeHandlerHelper.AddTypeHandlerImpl(typeof(DateTimeOffset), new DateTimeOffsetTypeHandler(), false);

            //time = 0;
            int loop = 100;

            TimeSpan time0 = TimeSpan.Zero, time1 = TimeSpan.Zero, time2 = TimeSpan.Zero, time3 = TimeSpan.Zero;
            for (int cur = 0; cur < loop; ++cur)
            {

                await mySqlConnection.OpenAsync().ConfigureAwait(false);


                using MySqlCommand command0 = new MySqlCommand("select * from tb_bookentity limit 10000", mySqlConnection);

                var reader0 = await command0.ExecuteReaderAsync().ConfigureAwait(false);

                List<BookEntity> list1 = new List<BookEntity>();
                List<BookEntity> list2 = new List<BookEntity>();
                List<BookEntity> list3 = new List<BookEntity>();

                int len = reader0.FieldCount;
                EntityPropertyDef[] propertyDefs = new EntityPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                EntityDef definition = EntityDefFactory.GetDef<BookEntity>()!;

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetPropertyDef(reader0.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }


                Func<IDataReader, object> mapper1 = EntityMapperDelegateCreator.CreateToEntityDelegate(definition, reader0, 0, definition.FieldCount, false, Database.Engine.EngineType.MySQL);

                //Warning: �����Dapper��С��DateTimeOffset�Ĵ洢���ᶪʧoffset��Ȼ��ת����ʱ�򣬻���ϵ���ʱ���offset
                Func<IDataReader, object> mapper2 = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(BookEntity), reader0);



                Stopwatch stopwatch1 = new Stopwatch();
                Stopwatch stopwatch2 = new Stopwatch();
                Stopwatch stopwatch3 = new Stopwatch();




                while (reader0.Read())
                {
                    stopwatch1.Start();

                    object obj1 = mapper1(reader0);

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
                        EntityPropertyDef property = propertyDefs[i];

                        object? value = TypeConvert.DbValueToTypeValue(reader0[i], property, Database.Engine.EngineType.MySQL);

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

        /// <summary>
        /// EntityMapper_ToParameter_Test
        /// </summary>
        /// <param name="engineType"></param>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [Theory]
        [InlineData(EngineType.MySQL)]
        [InlineData(EngineType.SQLite)]
        public void EntityMapper_ToParameter_Test(EngineType engineType)
        {
            PublisherEntity publisherEntity = Mocker.MockOnePublisherEntity();

            var emit_results = publisherEntity.ToParameters(EntityDefFactory.GetDef<PublisherEntity>()!, engineType, 1);

            var reflect_results = publisherEntity.ToParametersUsingReflection(EntityDefFactory.GetDef<PublisherEntity>()!, engineType, 1);

            AssertEqual(emit_results, reflect_results, engineType);

            //PublisherEntity2

            PublisherEntity2 publisherEntity2 = new PublisherEntity2();

            var emit_results2 = publisherEntity2.ToParameters(EntityDefFactory.GetDef<PublisherEntity2>()!, engineType, 1);

            var reflect_results2 = publisherEntity2.ToParametersUsingReflection(EntityDefFactory.GetDef<PublisherEntity2>()!, engineType, 1);

            AssertEqual(emit_results2, reflect_results2, engineType);

            //PublisherEntity3

            PublisherEntity3 publisherEntity3 = new PublisherEntity3();

            var emit_results3 = publisherEntity3.ToParameters(EntityDefFactory.GetDef<PublisherEntity3>()!, engineType, 1);

            var reflect_results3 = publisherEntity3.ToParametersUsingReflection(EntityDefFactory.GetDef<PublisherEntity3>()!, engineType, 1);

            AssertEqual(emit_results3, reflect_results3, engineType);

        }

        /// <summary>
        /// AssertEqual
        /// </summary>
        /// <param name="emit_results"></param>
        /// <param name="results"></param>
        /// <param name="engineType"></param>
        /// <exception cref="DatabaseException">Ignore.</exception>
        /// <exception cref="DatabaseException">Ignore.</exception>
        private static void AssertEqual(IEnumerable<KeyValuePair<string, object>> emit_results, IEnumerable<KeyValuePair<string, object>> results, EngineType engineType)
        {
            var dict = results.ToDictionary(kv => kv.Key);

            Assert.True(emit_results.Count() == dict.Count);

            foreach (var kv in emit_results)
            {
                Assert.True(dict.ContainsKey(kv.Key));

                Assert.True(TypeConvert.TypeValueToDbValueStatement(dict[kv.Key].Value, false, engineType) ==

                    TypeConvert.TypeValueToDbValueStatement(kv.Value, false, engineType));
            }
        }

        /// <summary>
        /// EntityMapper_ToParameter_Performance_Test
        /// </summary>
        /// <param name="engineType"></param>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [Theory]
        [InlineData(EngineType.MySQL)]
        [InlineData(EngineType.SQLite)]
        public void EntityMapper_ToParameter_Performance_Test(EngineType engineType)
        {
            var entities = Mocker.GetPublishers(1000000);

            var def = EntityDefFactory.GetDef<PublisherEntity>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Restart();
            foreach (var entity in entities)
            {
                _ = entity.ToParameters(def!, engineType);
            }
            stopwatch.Stop();

            _output.WriteLine($"Emit: {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            foreach (var entity in entities)
            {
                _ = entity.ToParametersUsingReflection(def!, engineType);
            }
            stopwatch.Stop();

            _output.WriteLine($"Reflection: {stopwatch.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task Test_10_Enum_TestAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;

            IList<PublisherEntity> publishers = Mocker.GetPublishers();

            TransactionContext transactionContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

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

            IEnumerable<PublisherEntity> publisherEntities = await database.RetrieveAsync<PublisherEntity>(p => p.Type == PublisherType.Big && p.LastUser == "lastUsre", null).ConfigureAwait(false);

            Assert.True(publisherEntities.Any() && publisherEntities.All(p => p.Type == PublisherType.Big));
        }
    }
}
