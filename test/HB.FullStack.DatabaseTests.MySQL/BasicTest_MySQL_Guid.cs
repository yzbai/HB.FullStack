using ClassLibrary1;

using HB.FullStack.Database;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;
using HB.FullStack.DatabaseTests.Data;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MySqlConnector;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class BasicTest_MySQL_Guid : BaseTestClass
    {
        [TestMethod]
        public async Task Test_Update_Fields_By_Compare_Version()
        {
            //Add
            Guid_BookEntity book = Mocker.Guid_GetBooks(1).First();

            await Db.AddAsync(book, "tester", null);

            //update-fields

            List<(string, object?)> toUpdate = new List<(string, object?)>();

            toUpdate.Add((nameof(Guid_BookEntity.Price), 123456.789));
            toUpdate.Add((nameof(Guid_BookEntity.Name), "TTTTTXXXXTTTTT"));

            await Db.UpdateFieldsAsync<Guid_BookEntity>(book.Id, book.Version, "UPDATE_FIELDS_VERSION", toUpdate, null);

            Guid_BookEntity? updatedBook = await Db.ScalarAsync<Guid_BookEntity>(book.Id, null);

            Assert.IsNotNull(updatedBook);

            Assert.IsTrue(updatedBook.Version == book.Version + 1);
            Assert.IsTrue(updatedBook.Price == 123456.789);
            Assert.IsTrue(updatedBook.Name == "TTTTTXXXXTTTTT");
            Assert.IsTrue(updatedBook.LastUser == "UPDATE_FIELDS_VERSION");
            Assert.IsTrue(updatedBook.LastTime > book.LastTime);

            //应该抛出冲突异常
            try
            {
                await Db.UpdateFieldsAsync<Guid_BookEntity>(book.Id, book.Version, "UPDATE_FIELDS_VERSION", toUpdate, null);
            }
            catch (DatabaseException ex)
            {
                Assert.IsTrue(ex.ErrorCode == DatabaseErrorCodes.ConcurrencyConflict);

                if (ex.ErrorCode != DatabaseErrorCodes.ConcurrencyConflict)
                {
                    throw ex;
                }
            }

        }

        [TestMethod]
        public async Task Test_Update_Fields_By_Compare_OldNewValues()
        {
            //Add
            Guid_BookEntity book = Mocker.Guid_GetBooks(1).First();

            await Db.AddAsync(book, "tester", null);

            //update-fields

            List<(string, object?, object?)> toUpdate = new List<(string, object?, object?)>();

            toUpdate.Add((nameof(Guid_BookEntity.Price), book.Price, 123456.789));
            toUpdate.Add((nameof(Guid_BookEntity.Name), book.Name, "TTTTTXXXXTTTTT"));

            await Db.UpdateFieldsAsync<Guid_BookEntity>(book.Id, "UPDATE_FIELDS_VERSION", toUpdate, null);

            Guid_BookEntity? updatedBook = await Db.ScalarAsync<Guid_BookEntity>(book.Id, null);

            Assert.IsNotNull(updatedBook);

            Assert.IsTrue(updatedBook.Version == book.Version + 1);
            Assert.IsTrue(updatedBook.Price == 123456.789);
            Assert.IsTrue(updatedBook.Name == "TTTTTXXXXTTTTT");
            Assert.IsTrue(updatedBook.LastUser == "UPDATE_FIELDS_VERSION");
            Assert.IsTrue(updatedBook.LastTime > book.LastTime);

            //应该抛出冲突异常
            try
            {
                await Db.UpdateFieldsAsync<Guid_BookEntity>(book.Id, "UPDATE_FIELDS_VERSION", toUpdate, null);
            }
            catch (DatabaseException ex)
            {
                Assert.IsTrue(ex.ErrorCode == DatabaseErrorCodes.ConcurrencyConflict);

                if (ex.ErrorCode != DatabaseErrorCodes.ConcurrencyConflict)
                {
                    throw ex;
                }
            }
        }

        [TestMethod]
        public async Task Test_Version_Error()
        {
            Guid_BookEntity book = Mocker.Guid_GetBooks(1).First();

            await Db.AddAsync(book, "tester", null);

            Guid_BookEntity? book1 = await Db.ScalarAsync<Guid_BookEntity>(book.Id, null);
            Guid_BookEntity? book2 = await Db.ScalarAsync<Guid_BookEntity>(book.Id, null);

            //update book1
            book1!.Name = "Update Book1";
            await Db.UpdateAsync(book1, "test", null);

            //update book2
            book2!.Name = "Update book2";
            await Db.UpdateAsync(book2, "tester", null);



        }

        /// <summary>
        /// //NOTICE: 在sqlite下，重复update，返回1.即matched
        /// //NOTICE: 在mysql下，重复update，返回1，即mactched
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_Repeate_Update_Return()
        {
            Guid_BookEntity book = Mocker.Guid_GetBooks(1).First();
            book.Id = new Guid("cd5bda08-e5e2-409f-89c5-bea1ae49f2a0");

            await Db.AddAsync(book, "tester", null);

            Guid_BookEntity? book1 = await Db.ScalarAsync<Guid_BookEntity>(book.Id, null);

            int rt = await Db.DatabaseEngine.ExecuteCommandNonQueryAsync(null, Db.DatabaseNames.First(),
                new EngineCommand($"update tb_guid_bookentity set Name='Update_xxx' where Id = uuid_to_bin('08da5bcd-e2e5-9f40-89c5-bea1ae49f2a0')"));

            int rt2 = await Db.DatabaseEngine.ExecuteCommandNonQueryAsync(null, Db.DatabaseNames.First(),
                new EngineCommand($"update tb_guid_bookentity set Name='Update_xxx' where Id = uuid_to_bin('08da5bcd-e2e5-9f40-89c5-bea1ae49f2a0')"));

            int rt3 = await Db.DatabaseEngine.ExecuteCommandNonQueryAsync(null, Db.DatabaseNames.First(),
                new EngineCommand($"update tb_guid_bookentity set Name='Update_xxx' where Id = uuid_to_bin('08da5bcd-e2e5-9f40-89c5-bea1ae49f2a0')"));
        }

        /// <summary>
        /// //NOTICE: Mysql执行多条语句的时候，ExecuteCommandReader只返回最后一个结果。
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_Mult_SQL_Return_With_Reader()
        {
            Guid_BookEntity book = Mocker.Guid_GetBooks(1).First();
            book.Id = new Guid("cd5bda08-e5e2-409f-89c5-bea1ae49f2a0");

            await Db.AddAsync(book, "tester", null);

            string sql = @"
update tb_guid_bookentity set LastUser='TTTgdTTTEEST' where Id = uuid_to_bin('08da5bcd-e2e5-9f40-89c5-bea1ae49f2a0') and Deleted = 0 and Version='10';
select count(1) from tb_guid_bookentity where Id = uuid_to_bin('08da5bcd-e2e5-9f40-89c5-bea1ae49f2a0') and Deleted = 0;
";
            using IDataReader reader = await Db.DatabaseEngine.ExecuteCommandReaderAsync(null, Db.DatabaseNames.First(),
                new EngineCommand(sql), true);

            List<string?> rt = new List<string?>();

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    rt.Add(reader.GetValue(i)?.ToString());
                }
            }

            Assert.AreEqual(rt.Count, 1);
        }

        [TestMethod]
        public async Task Guid_Test_01_Batch_Add_PublisherEntityAsync()
        {
            IList<Guid_PublisherEntity> publishers = Mocker.Guid_GetPublishers();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(publishers, "lastUsre", transactionContext).ConfigureAwait(false);

                await Trans.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Guid_Test_02_Batch_Update_PublisherEntityAsync()
        {
            TransactionContext transContext = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            string updatedName = SecurityUtil.CreateUniqueToken();

            int count = 800;

            try
            {
                IEnumerable<Guid_PublisherEntity> lst = await Db.RetrieveAllAsync<Guid_PublisherEntity>(transContext, 0, count).ConfigureAwait(false);

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

                await Db.BatchUpdateAsync(lst, "lastUsre", transContext).ConfigureAwait(false);

                await Trans.CommitAsync(transContext).ConfigureAwait(false);

                lst = await Db.RetrieveAllAsync<Guid_PublisherEntity>(null, 0, count);

                Assert.IsTrue(lst.All(t => t.Name == updatedName));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(transContext).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Guid_Test_03_Batch_Delete_PublisherEntityAsync()
        {
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity> lst = (await Db.RetrieveAllAsync<Guid_PublisherEntity>(transactionContext, 1, 5).ConfigureAwait(false)).ToList();

                if (lst.Count != 0)
                {
                    await Db.BatchDeleteAsync(lst, "lastUsre", transactionContext).ConfigureAwait(false);
                }

                await Trans.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Guid_Test_04_Add_PublisherEntityAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity> lst = new List<Guid_PublisherEntity>();

                for (int i = 0; i < 10; ++i)
                {
                    Guid_PublisherEntity entity = Mocker.Guid_MockOnePublisherEntity();

                    await Db.AddAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                    lst.Add(entity);
                }

                await Trans.CommitAsync(tContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(tContext).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Guid_Test_05_Update_PublisherEntityAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity> testEntities = (await Db.RetrieveAllAsync<Guid_PublisherEntity>(tContext, 1, 1).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                Guid_PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Code = "Yuzhaobai" });

                await Db.UpdateAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                Guid_PublisherEntity? stored = await Db.ScalarAsync<Guid_PublisherEntity>(entity.Id, tContext).ConfigureAwait(false);

                await Trans.CommitAsync(tContext).ConfigureAwait(false);

                Assert.IsTrue(stored?.Books.Contains("New Book2"));
                //Assert.IsTrue(stored?.BookAuthors["New Book2"].Mobile == "15190208956");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(tContext).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Guid_Test_06_Delete_PublisherEntityAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherEntity> testEntities = (await Db.RetrieveAllAsync<Guid_PublisherEntity>(tContext).ConfigureAwait(false)).ToList();

                foreach (var entity in testEntities)
                {
                    await Db.DeleteAsync(entity, "lastUsre", tContext).ConfigureAwait(false);
                }

                long count = await Db.CountAsync<Guid_PublisherEntity>(tContext).ConfigureAwait(false);

                await Trans.CommitAsync(tContext).ConfigureAwait(false);

                Assert.IsTrue(count == 0);
            }
            catch (Exception ex)
            {
                await Trans.RollbackAsync(tContext).ConfigureAwait(false);
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task Guid_Test_08_LastTimeTestAsync()
        {
            Guid_PublisherEntity item = Mocker.Guid_MockOnePublisherEntity();

            await Db.AddAsync(item, "xx", null).ConfigureAwait(false);

            var fetched = await Db.ScalarAsync<Guid_PublisherEntity>(item.Id, null).ConfigureAwait(false);

            Assert.AreEqual(item.LastTime, fetched!.LastTime);

            fetched.Name = "ssssss";

            await Db.UpdateAsync(fetched, "xxx", null).ConfigureAwait(false);

            fetched = await Db.ScalarAsync<Guid_PublisherEntity>(item.Id, null).ConfigureAwait(false);

            //await Db.AddOrUpdateAsync(item, "ss", null);

            fetched = await Db.ScalarAsync<Guid_PublisherEntity>(item.Id, null).ConfigureAwait(false);

            //Batch

            var items = Mocker.Guid_GetPublishers();

            TransactionContext trans = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(items, "xx", trans).ConfigureAwait(false);

                var results = await Db.RetrieveAsync<Guid_PublisherEntity>(item => SqlStatement.In(item.Id, true, items.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await Db.BatchUpdateAsync(items, "xx", trans).ConfigureAwait(false);

                var items2 = Mocker.Guid_GetPublishers();

                await Db.BatchAddAsync(items2, "xx", trans).ConfigureAwait(false);

                results = await Db.RetrieveAsync<Guid_PublisherEntity>(item => SqlStatement.In(item.Id, true, items2.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await Db.BatchUpdateAsync(items2, "xx", trans).ConfigureAwait(false);

                await Trans.CommitAsync(trans).ConfigureAwait(false);
            }
            catch
            {
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
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
        [TestMethod]
        public async Task Guid_Test_09_UpdateLastTimeTestAsync()
        {
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                Guid_PublisherEntity item = Mocker.Guid_MockOnePublisherEntity();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                IList<Guid_PublisherEntity> testEntities = (await Db.RetrieveAllAsync<Guid_PublisherEntity>(transactionContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                Guid_PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Code = "Yuzhaobai" });

                await Db.UpdateAsync(entity, "lastUsre", transactionContext).ConfigureAwait(false);

                Guid_PublisherEntity? stored = await Db.ScalarAsync<Guid_PublisherEntity>(entity.Id, transactionContext).ConfigureAwait(false);

                item = Mocker.Guid_MockOnePublisherEntity();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                var fetched = await Db.ScalarAsync<Guid_PublisherEntity>(item.Id, transactionContext).ConfigureAwait(false);

                Assert.AreEqual(item.LastTime, fetched!.LastTime);

                fetched.Name = "ssssss";

                await Db.UpdateAsync(fetched, "xxx", transactionContext).ConfigureAwait(false);

                await Trans.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await Trans.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Guid_Test_10_Enum_TestAsync()
        {
            IList<Guid_PublisherEntity> publishers = Mocker.Guid_GetPublishers();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(publishers, "lastUsre", transactionContext).ConfigureAwait(false);

                await Trans.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            IEnumerable<Guid_PublisherEntity> publisherEntities = await Db.RetrieveAsync<Guid_PublisherEntity>(p => p.Type == PublisherType.Big && p.LastUser == "lastUsre", null).ConfigureAwait(false);

            Assert.IsTrue(publisherEntities.Any() && publisherEntities.All(p => p.Type == PublisherType.Big));
        }

        [TestMethod]
        public async Task Guid_Test_11_StartWith_TestAsync()
        {
            IList<Guid_PublisherEntity> publishers = Mocker.Guid_GetPublishers();

            foreach (var entity in publishers)
            {
                entity.Name = "StartWithTest_xxx";
            }

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(publishers, "lastUsre", transactionContext).ConfigureAwait(false);

                await Trans.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            IEnumerable<Guid_PublisherEntity> entities = await Db.RetrieveAsync<Guid_PublisherEntity>(t => t.Name.StartsWith("Star"), null);

            Assert.IsTrue(entities.Any());

            Assert.IsTrue(entities.All(t => t.Name.StartsWith("Star")));
        }

        [TestMethod]
        public async Task Guid_Test_12_Binary_TestAsync()
        {
            IList<Guid_PublisherEntity> publishers = Mocker.Guid_GetPublishers();

            foreach (var entity in publishers)
            {
                entity.Name = "StartWithTest_xxx";
            }

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherEntity>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(publishers, "lastUsre", transactionContext).ConfigureAwait(false);

                await Trans.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            IEnumerable<Guid_PublisherEntity> entities = await Db.RetrieveAsync<Guid_PublisherEntity>(
                t => t.Name.StartsWith("Star") && publishers.Any(), null);

            //IEnumerable<Guid_PublisherEntity> entities = await Db.RetrieveAsync<Guid_PublisherEntity>(
            //    t => ReturnGuid() == ReturnGuid(), null);

            Assert.IsTrue(entities.Any());

            Assert.IsTrue(entities.All(t => t.Name.StartsWith("Star")));
        }

        [TestMethod]
        public async Task Guid_Test_13_Mapper_ToEntityAsync()
        {
            GlobalSettings.Logger.LogDebug($"��ǰProcess,{Environment.ProcessId}");

            #region Json验证1

            var publisher3 = new Guid_PublisherEntity();

            await Db.AddAsync(publisher3, "sss", null).ConfigureAwait(false);

            var stored3 = await Db.ScalarAsync<Guid_PublisherEntity>(publisher3.Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher3), SerializeUtil.ToJson(stored3));

            #endregion

            #region Json验证2

            var publisher2s = Mocker.Guid_GetPublishers2();

            foreach (Guid_PublisherEntity2 publisher in publisher2s)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            Guid_PublisherEntity2? publisher2 = await Db.ScalarAsync<Guid_PublisherEntity2>(publisher2s[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher2), SerializeUtil.ToJson(publisher2s[0]));

            #endregion

            #region Json验证3

            var publishers = Mocker.Guid_GetPublishers();

            foreach (Guid_PublisherEntity publisher in publishers)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            Guid_PublisherEntity? publisher1 = await Db.ScalarAsync<Guid_PublisherEntity>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));

            #endregion
        }

        [TestMethod]
        public async Task Guid_Test_14_Mapper_ToEntity_PerformanceAsync()
        {
            var books = Mocker.Guid_GetBooks(50);

            var trans = await Trans.BeginTransactionAsync<Guid_BookEntity>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(books, "x", trans).ConfigureAwait(false);
                await Trans.CommitAsync(trans).ConfigureAwait(false);
            }
            catch
            {
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
            }

            Stopwatch stopwatch = new Stopwatch();

            using MySqlConnection mySqlConnection = new MySqlConnection(ConnectionString);

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

                EntityDef definition = Db.EntityDefFactory.GetDef<Guid_BookEntity>()!;

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetPropertyDef(reader.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }

                Func<IEntityDefFactory, IDataReader, object> fullStack_mapper = EntityMapperDelegateCreator.CreateToEntityDelegate(definition, reader, 0, definition.FieldCount, false, EngineType.MySQL);

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
                    object obj1 = fullStack_mapper(Db.EntityDefFactory, reader);
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

            Console.WriteLine("FullStack_Emit : " + (time1.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("Dapper : " + (time2.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("FullStack_Reflection : " + (time3.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void Guid_Test_15_Mapper_ToParameter()
        {
            Guid_PublisherEntity publisherEntity = Mocker.Guid_MockOnePublisherEntity();
            publisherEntity.Version = 0;

            var emit_results = publisherEntity.EntityToParameters(Db.EntityDefFactory.GetDef<Guid_PublisherEntity>()!, EngineType.MySQL, Db.EntityDefFactory, 1);

            var reflect_results = publisherEntity.EntityToParametersUsingReflection(Db.EntityDefFactory.GetDef<Guid_PublisherEntity>()!, EngineType.MySQL, 1);

            AssertEqual(emit_results, reflect_results, EngineType.MySQL);

            //PublisherEntity2

            Guid_PublisherEntity2 publisherEntity2 = new Guid_PublisherEntity2
            {
                Version = 0
            };

            IList<KeyValuePair<string, object>>? emit_results2 = publisherEntity2.EntityToParameters(Db.EntityDefFactory.GetDef<Guid_PublisherEntity2>()!, EngineType.MySQL, Db.EntityDefFactory, 1);

            var reflect_results2 = publisherEntity2.EntityToParametersUsingReflection(Db.EntityDefFactory.GetDef<Guid_PublisherEntity2>()!, EngineType.MySQL, 1);

            AssertEqual(emit_results2, reflect_results2, EngineType.MySQL);

            //PublisherEntity3

            Guid_PublisherEntity3 publisherEntity3 = new Guid_PublisherEntity3
            {
                Version = 0
            };

            var emit_results3 = publisherEntity3.EntityToParameters(Db.EntityDefFactory.GetDef<Guid_PublisherEntity3>()!, EngineType.MySQL, Db.EntityDefFactory, 1);

            var reflect_results3 = publisherEntity3.EntityToParametersUsingReflection(Db.EntityDefFactory.GetDef<Guid_PublisherEntity3>()!, EngineType.MySQL, 1);

            AssertEqual(emit_results3, reflect_results3, EngineType.MySQL);
        }

        [TestMethod]
        public void Guid_Test_16_Mapper_ToParameter_Performance()
        {
            var entities = Mocker.Guid_GetPublishers(1000000);

            foreach (var entity in entities)
            {
                entity.Version = 0;
            }

            var def = Db.EntityDefFactory.GetDef<Guid_PublisherEntity>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Restart();
            foreach (var entity in entities)
            {
                _ = entity.EntityToParameters(def!, EngineType.MySQL, Db.EntityDefFactory);
            }
            stopwatch.Stop();

            Console.WriteLine($"Emit: {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            foreach (var entity in entities)
            {
                _ = entity.EntityToParametersUsingReflection(def!, EngineType.MySQL);
            }
            stopwatch.Stop();

            Console.WriteLine($"Reflection: {stopwatch.ElapsedMilliseconds}");
        }

        private static void AssertEqual(IEnumerable<KeyValuePair<string, object>> emit_results, IEnumerable<KeyValuePair<string, object>> results, EngineType engineType)
        {
            var dict = results.ToDictionary(kv => kv.Key);

            Assert.IsTrue(emit_results.Count() == dict.Count);

            foreach (var kv in emit_results)
            {
                Assert.IsTrue(dict.ContainsKey(kv.Key));

                Assert.IsTrue(TypeConvert.DoNotUseUnSafeTypeValueToDbValueStatement(dict[kv.Key].Value, false, engineType) ==

                    TypeConvert.DoNotUseUnSafeTypeValueToDbValueStatement(kv.Value, false, engineType));
            }
        }

        public static Guid ReturnGuid()
        {
            return Guid.NewGuid();
        }
    }
}