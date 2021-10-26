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
    public class BasicTest_MySQL_Guid : IClassFixture<ServiceFixture_MySql>
    {
        private readonly IDatabase _mysql;
        private readonly ITransaction _mysqlTransaction;
        private readonly ITestOutputHelper _output;
        private readonly string _mysqlConnectionString;

        public BasicTest_MySQL_Guid(ITestOutputHelper testOutputHelper, ServiceFixture_MySql serviceFixture)
        {
            //TestCls testCls = serviceFixture.ServiceProvider.GetRequiredService<TestCls>();

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
        public async Task Guid_Test_01_Batch_Add_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;

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
        }

        /// <summary>
        /// Test_2_Batch_Update_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]
        public async Task Guid_Test_02_Batch_Update_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext transContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            string updatedName = SecurityUtil.CreateUniqueToken();

            int count = 800;

            try
            {
                IEnumerable<Guid_PublisherEntity> lst = await database.PageAsync<Guid_PublisherEntity>(1, count, transContext).ConfigureAwait(false);

                for (int i = 0; i < lst.Count(); i++)
                {
                    Guid_PublisherEntity entity = lst.ElementAt(i);
                    //entity.Guid = Guid.NewGuid().ToString();
                    entity.Type = PublisherType.Online;
                    entity.Name = updatedName;
                    entity.Books = new List<string>() { "xxx", "tttt" };
                    entity.BookAuthors = new Dictionary<string, Author>()
                    {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                };
                }

                await database.BatchUpdateAsync(lst, "lastUsre", transContext).ConfigureAwait(false);

                await transaction.CommitAsync(transContext).ConfigureAwait(false);

                lst = await database.PageAsync<Guid_PublisherEntity>(1, count, null);

                Assert.True(lst.All(t => t.Name == updatedName));

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
        public async Task Guid_Test_03_Batch_Delete_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext transactionContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity> lst = (await database.PageAsync<Guid_PublisherEntity>(2, 5, transactionContext).ConfigureAwait(false)).ToList();

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
        public async Task Guid_Test_04_Add_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext tContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity> lst = new List<Guid_PublisherEntity>();

                for (int i = 0; i < 10; ++i)
                {
                    Guid_PublisherEntity entity = Mocker.Guid_MockOnePublisherEntity();

                    await database.AddAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                    lst.Add(entity);
                }

                await transaction.CommitAsync(tContext).ConfigureAwait(false);


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


        public async Task Guid_Test_05_Update_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext tContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity> testEntities = (await database.PageAsync<Guid_PublisherEntity>(2, 1, tContext).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                Guid_PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await database.UpdateAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                Guid_PublisherEntity? stored = await database.ScalarAsync<Guid_PublisherEntity>(entity.Id, tContext).ConfigureAwait(false);

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
        public async Task Guid_Test_06_Delete_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext tContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity> testEntities = (await database.RetrieveAllAsync<Guid_PublisherEntity>(tContext).ConfigureAwait(false)).ToList();

                foreach (var entity in testEntities)
                {
                    await database.DeleteAsync(entity, "lastUsre", tContext).ConfigureAwait(false);
                }

                long count = await database.CountAsync<Guid_PublisherEntity>(tContext).ConfigureAwait(false);

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
        public async Task Guid_Test_08_LastTimeTestAsync()
        {
            IDatabase database = _mysql;

            Guid_PublisherEntity item = Mocker.Guid_MockOnePublisherEntity();

            await database.AddAsync(item, "xx", null).ConfigureAwait(false);

            var fetched = await database.ScalarAsync<Guid_PublisherEntity>(item.Id, null).ConfigureAwait(false);

            Assert.Equal(item.LastTime, fetched!.LastTime);

            fetched.Name = "ssssss";

            await database.UpdateAsync(fetched, "xxx", null).ConfigureAwait(false);

            fetched = await database.ScalarAsync<Guid_PublisherEntity>(item.Id, null).ConfigureAwait(false);

            //await database.AddOrUpdateAsync(item, "ss", null);

            fetched = await database.ScalarAsync<Guid_PublisherEntity>(item.Id, null).ConfigureAwait(false);



            //Batch

            var items = Mocker.Guid_GetPublishers();

            ITransaction transaction = _mysqlTransaction;

            TransactionContext trans = await transaction.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                await database.BatchAddAsync(items, "xx", trans).ConfigureAwait(false);


                var results = await database.RetrieveAsync<Guid_PublisherEntity>(item => SqlStatement.In(item.Id, true, items.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await database.BatchUpdateAsync(items, "xx", trans).ConfigureAwait(false);

                var items2 = Mocker.Guid_GetPublishers();

                await database.BatchAddAsync(items2, "xx", trans).ConfigureAwait(false);

                results = await database.RetrieveAsync<Guid_PublisherEntity>(item => SqlStatement.In(item.Id, true, items2.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

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
        public async Task Guid_Test_09_UpdateLastTimeTestAsync()
        {
            IDatabase database = _mysql;

            ITransaction transaction = _mysqlTransaction;

            TransactionContext transactionContext = await transaction.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                Guid_PublisherEntity item = Mocker.Guid_MockOnePublisherEntity();

                await database.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                IList<Guid_PublisherEntity> testEntities = (await database.PageAsync<Guid_PublisherEntity>(1, 1, transactionContext).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                Guid_PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await database.UpdateAsync(entity, "lastUsre", transactionContext).ConfigureAwait(false);

                Guid_PublisherEntity? stored = await database.ScalarAsync<Guid_PublisherEntity>(entity.Id, transactionContext).ConfigureAwait(false);



                item = Mocker.Guid_MockOnePublisherEntity();

                await database.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                var fetched = await database.ScalarAsync<Guid_PublisherEntity>(item.Id, transactionContext).ConfigureAwait(false);

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

        [Fact]
        public async Task Guid_Test_10_Enum_TestAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;

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

            IEnumerable<Guid_PublisherEntity> publisherEntities = await database.RetrieveAsync<Guid_PublisherEntity>(p => p.Type == PublisherType.Big && p.LastUser == "lastUsre", null).ConfigureAwait(false);

            Assert.True(publisherEntities.Any() && publisherEntities.All(p => p.Type == PublisherType.Big));
        }

        [Fact]
        public async Task Guid_Test_11_StartWith_TestAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;

            IList<Guid_PublisherEntity> publishers = Mocker.Guid_GetPublishers();

            foreach (var entity in publishers)
            {
                entity.Name = "StartWithTest_xxx";
            }

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

            IEnumerable<Guid_PublisherEntity> entities = await database.RetrieveAsync<Guid_PublisherEntity>(t => t.Name.StartsWith("Star"), null);

            Assert.True(entities.Any());

            Assert.All(entities, t => t.Name.StartsWith("Star"));
        }

        [Fact]
        public async Task Guid_Test_12_Binary_TestAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;

            IList<Guid_PublisherEntity> publishers = Mocker.Guid_GetPublishers();

            foreach (var entity in publishers)
            {
                entity.Name = "StartWithTest_xxx";
            }

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

            IEnumerable<Guid_PublisherEntity> entities = await database.RetrieveAsync<Guid_PublisherEntity>(
                t => t.Name.StartsWith("Star") && publishers.Any(), null);

            //IEnumerable<Guid_PublisherEntity> entities = await database.RetrieveAsync<Guid_PublisherEntity>(
            //    t => ReturnGuid() == ReturnGuid(), null);

            Assert.True(entities.Any());

            Assert.All(entities, t => t.Name.StartsWith("Star"));
        }

        /// <summary>
        /// Test_EntityMapperAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [Fact]
        public async Task Guid_Test_13_Mapper_ToEntityAsync()
        {
            GlobalSettings.Logger.LogDebug($"��ǰProcess,{Environment.ProcessId}");

            IDatabase database = _mysql;

            #region Json验证1

            var publisher3 = new Guid_PublisherEntity();

            await database.AddAsync(publisher3, "sss", null).ConfigureAwait(false);

            var stored3 = await database.ScalarAsync<Guid_PublisherEntity>(publisher3.Id, null).ConfigureAwait(false);

            Assert.Equal(SerializeUtil.ToJson(publisher3), SerializeUtil.ToJson(stored3));

            #endregion

            #region Json验证2

            var publisher2s = Mocker.Guid_GetPublishers2();


            foreach (Guid_PublisherEntity2 publisher in publisher2s)
            {
                await database.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }


            Guid_PublisherEntity2? publisher2 = await database.ScalarAsync<Guid_PublisherEntity2>(publisher2s[0].Id, null).ConfigureAwait(false);

            Assert.Equal(SerializeUtil.ToJson(publisher2), SerializeUtil.ToJson(publisher2s[0]));

            #endregion

            #region Json验证3

            var publishers = Mocker.Guid_GetPublishers();


            foreach (Guid_PublisherEntity publisher in publishers)
            {
                await database.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }


            Guid_PublisherEntity? publisher1 = await database.ScalarAsync<Guid_PublisherEntity>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.Equal(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));
            #endregion
        }

        /// <summary>
        /// Test_EntityMapperPerformanceAsync
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [Fact]
        public async Task Guid_Test_14_Mapper_ToEntity_PerformanceAsync()
        {
            IDatabase database = _mysql;

            var books = Mocker.Guid_GetBooks(5000);

            var trans = await _mysqlTransaction.BeginTransactionAsync<Guid_BookEntity>().ConfigureAwait(false);

            try
            {

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
            TypeHandlerHelper.AddTypeHandlerImpl(typeof(Guid), new MySqlGuidTypeHandler(), false);

            //time = 0;
            int loop = 1;

            TimeSpan time0 = TimeSpan.Zero, time1 = TimeSpan.Zero, time2 = TimeSpan.Zero, time3 = TimeSpan.Zero;
            for (int cur = 0; cur < loop; ++cur)
            {

                await mySqlConnection.OpenAsync().ConfigureAwait(false);


                using MySqlCommand command0 = new MySqlCommand("select * from tb_guid_bookentity limit 5000", mySqlConnection);

                var reader = await command0.ExecuteReaderAsync().ConfigureAwait(false);

                List<Guid_BookEntity> list1 = new List<Guid_BookEntity>();
                List<Guid_BookEntity> list2 = new List<Guid_BookEntity>();
                List<Guid_BookEntity> list3 = new List<Guid_BookEntity>();

                int len = reader.FieldCount;
                EntityPropertyDef[] propertyDefs = new EntityPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                EntityDef definition = EntityDefFactory.GetDef<Guid_BookEntity>()!;

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetPropertyDef(reader.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }

                Func<IDataReader, object> fullStack_mapper = EntityMapperDelegateCreator.CreateToEntityDelegate(definition, reader, 0, definition.FieldCount, false, EngineType.MySQL);

                Func<IDataReader, object> dapper_mapper = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(Guid_BookEntity), reader);

                Func<IDataReader, object> reflection_mapper = (r) =>
                {
                    Guid_BookEntity item = new Guid_BookEntity();

                    for (int i = 0; i < len; ++i)
                    {
                        EntityPropertyDef property = propertyDefs[i];

                        object? value = TypeConvert.DbValueToTypeValue(r[i], property, EngineType.MySQL);

                        if (value != null)
                        {
                            setMethods[i].Invoke(item, new object?[] { value });
                        }

                    }

                    return item;
                };

                Stopwatch stopwatch1 = new Stopwatch();
                Stopwatch stopwatch2 = new Stopwatch();
                Stopwatch stopwatch3 = new Stopwatch();

                while (reader.Read())
                {
                    stopwatch1.Start();
                    object obj1 = fullStack_mapper(reader);
                    list1.Add((Guid_BookEntity)obj1);
                    stopwatch1.Stop();

                    stopwatch2.Start();
                    object obj2 = dapper_mapper(reader);
                    list2.Add((Guid_BookEntity)obj2);
                    stopwatch2.Stop();

                    stopwatch3.Start();
                    object obj3 = reflection_mapper(reader);
                    list3.Add((Guid_BookEntity)obj3);
                    stopwatch3.Stop();
                }

                time1 += stopwatch1.Elapsed;
                time2 += stopwatch2.Elapsed;
                time3 += stopwatch3.Elapsed;

                await reader.DisposeAsync().ConfigureAwait(false);
                command0.Dispose();

                await mySqlConnection.CloseAsync().ConfigureAwait(false);

            }

            _output.WriteLine("FullStack_Emit : " + (time1.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            _output.WriteLine("Dapper : " + (time2.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            _output.WriteLine("FullStack_Reflection : " + (time3.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// EntityMapper_ToParameter_Test
        /// </summary>
        /// <param name="engineType"></param>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [Fact]
        public void Guid_Test_15_Mapper_ToParameter()
        {
            Guid_PublisherEntity publisherEntity = Mocker.Guid_MockOnePublisherEntity();
            publisherEntity.Version = 0;

            var emit_results = publisherEntity.ToParameters(EntityDefFactory.GetDef<Guid_PublisherEntity>()!, EngineType.MySQL, 1);

            var reflect_results = publisherEntity.ToParametersUsingReflection(EntityDefFactory.GetDef<Guid_PublisherEntity>()!, EngineType.MySQL, 1);

            AssertEqual(emit_results, reflect_results, EngineType.MySQL);

            //PublisherEntity2

            Guid_PublisherEntity2 publisherEntity2 = new Guid_PublisherEntity2
            {
                Version = 0
            };

            IList<KeyValuePair<string, object>>? emit_results2 = publisherEntity2.ToParameters(EntityDefFactory.GetDef<Guid_PublisherEntity2>()!, EngineType.MySQL, 1);

            var reflect_results2 = publisherEntity2.ToParametersUsingReflection(EntityDefFactory.GetDef<Guid_PublisherEntity2>()!, EngineType.MySQL, 1);

            AssertEqual(emit_results2, reflect_results2, EngineType.MySQL);

            //PublisherEntity3

            Guid_PublisherEntity3 publisherEntity3 = new Guid_PublisherEntity3
            {
                Version = 0
            };

            var emit_results3 = publisherEntity3.ToParameters(EntityDefFactory.GetDef<Guid_PublisherEntity3>()!, EngineType.MySQL, 1);

            var reflect_results3 = publisherEntity3.ToParametersUsingReflection(EntityDefFactory.GetDef<Guid_PublisherEntity3>()!, EngineType.MySQL, 1);

            AssertEqual(emit_results3, reflect_results3, EngineType.MySQL);

        }

        /// <summary>
        /// EntityMapper_ToParameter_Performance_Test
        /// </summary>
        /// <param name="engineType"></param>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [Fact]
        public void Guid_Test_16_Mapper_ToParameter_Performance()
        {
            var entities = Mocker.Guid_GetPublishers(1000000);

            foreach (var entity in entities)
            {
                entity.Version = 0;
            }

            var def = EntityDefFactory.GetDef<Guid_PublisherEntity>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Restart();
            foreach (var entity in entities)
            {
                _ = entity.ToParameters(def!, EngineType.MySQL);
            }
            stopwatch.Stop();

            _output.WriteLine($"Emit: {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            foreach (var entity in entities)
            {
                _ = entity.ToParametersUsingReflection(def!, EngineType.MySQL);
            }
            stopwatch.Stop();

            _output.WriteLine($"Reflection: {stopwatch.ElapsedMilliseconds}");
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

                Assert.True(TypeConvert.DoNotUseUnSafeTypeValueToDbValueStatement(dict[kv.Key].Value, false, engineType) ==

                    TypeConvert.DoNotUseUnSafeTypeValueToDbValueStatement(kv.Value, false, engineType));
            }
        }

        public static Guid ReturnGuid()
        {
            return Guid.NewGuid();
        }
    }
}
