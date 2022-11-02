using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using ClassLibrary1;

using HB.FullStack.BaseTest;
using HB.FullStack.Database;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;
using HB.FullStack.BaseTest.Data.MySqls;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MySqlConnector;

namespace HB.FullStack.DatabaseTests.MySQL
{
    [TestClass]
    public class BasicTest_Guid_MySQL : BaseTestClass
    {
        [TestMethod]
        public async Task Test_Add_Key_Conflict_ErrorAsync()
        {
            Guid_BookModel book = Mocker.Guid_GetBooks(1).First();

            await Db.AddAsync(book, "tester", null);

            try
            {
                await Db.AddAsync(book, "tester", null);
            }
            catch (DatabaseException e)
            {
                Assert.IsTrue(e.ErrorCode == ErrorCodes.DuplicateKeyEntry);
            }

            PublisherModel publisherModel = Mocker.MockOnePublisherModel();

            await Db.AddAsync(publisherModel, "", null);

            try
            {
                await Db.AddAsync(publisherModel, "", null);
            }
            catch (DatabaseException e)
            {
                Assert.IsTrue(e.ErrorCode == ErrorCodes.DuplicateKeyEntry);
            }

        }

        [TestMethod]
        public async Task Test_BatchAdd_Key_Conflict_ErrorAsync()
        {
            var books = Mocker.Guid_GetBooks(2);

            await Db.BatchAddAsync(books, "tester", null);

            try
            {
                await Db.BatchAddAsync(books, "tester", null);
            }
            catch (DatabaseException e)
            {
                Assert.IsTrue(e.ErrorCode == ErrorCodes.DuplicateKeyEntry);
            }

        }

        [TestMethod]
        public async Task Test_Update_Fields_By_Compare_Version()
        {
            //Add
            Guid_BookModel book = Mocker.Guid_GetBooks(1).First();

            await Db.AddAsync(book, "tester", null);

            //update-fields

            List<(string, object?)> toUpdate = new List<(string, object?)>();

            toUpdate.Add((nameof(Guid_BookModel.Price), 123456.789));
            toUpdate.Add((nameof(Guid_BookModel.Name), "TTTTTXXXXTTTTT"));

            await Db.UpdatePropertiesAsync<Guid_BookModel>(book.Id, toUpdate, book.Timestamp, "UPDATE_FIELDS_VERSION", null);

            Guid_BookModel? updatedBook = await Db.ScalarAsync<Guid_BookModel>(book.Id, null);

            Assert.IsNotNull(updatedBook);

            Assert.IsTrue(updatedBook.Price == 123456.789);
            Assert.IsTrue(updatedBook.Name == "TTTTTXXXXTTTTT");
            Assert.IsTrue(updatedBook.LastUser == "UPDATE_FIELDS_VERSION");
            Assert.IsTrue(updatedBook.Timestamp > book.Timestamp);

            //应该抛出冲突异常
            try
            {
                await Db.UpdatePropertiesAsync<Guid_BookModel>(book.Id, toUpdate, book.Timestamp, "UPDATE_FIELDS_VERSION", null);
            }
            catch (DatabaseException ex)
            {
                Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);

                if (ex.ErrorCode != ErrorCodes.ConcurrencyConflict)
                {
                    throw ex;
                }
            }

        }

        [TestMethod]
        public async Task Test_Update_Fields_By_Compare_OldNewValues()
        {
            //Add
            Guid_BookModel book = Mocker.Guid_GetBooks(1).First();

            await Db.AddAsync(book, "tester", null);

            //update-fields

            List<(string, object?, object?)> toUpdate = new List<(string, object?, object?)>();

            toUpdate.Add((nameof(Guid_BookModel.Price), book.Price, 123456.789));
            toUpdate.Add((nameof(Guid_BookModel.Name), book.Name, "TTTTTXXXXTTTTT"));

            await Db.UpdatePropertiesAsync<Guid_BookModel>(book.Id, toUpdate, "UPDATE_FIELDS_VERSION", null);

            Guid_BookModel? updatedBook = await Db.ScalarAsync<Guid_BookModel>(book.Id, null);

            Assert.IsNotNull(updatedBook);

            Assert.IsTrue(updatedBook.Price == 123456.789);
            Assert.IsTrue(updatedBook.Name == "TTTTTXXXXTTTTT");
            Assert.IsTrue(updatedBook.LastUser == "UPDATE_FIELDS_VERSION");
            Assert.IsTrue(updatedBook.Timestamp > book.Timestamp);

            //应该抛出冲突异常
            try
            {
                await Db.UpdatePropertiesAsync<Guid_BookModel>(book.Id, toUpdate, "UPDATE_FIELDS_VERSION", null);
            }
            catch (DatabaseException ex)
            {
                Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);

                if (ex.ErrorCode != ErrorCodes.ConcurrencyConflict)
                {
                    throw ex;
                }
            }
        }

        [TestMethod]
        public async Task Test_Version_Error()
        {
            Guid_BookModel book = Mocker.Guid_GetBooks(1).First();

            await Db.AddAsync(book, "tester", null);

            Guid_BookModel? book1 = await Db.ScalarAsync<Guid_BookModel>(book.Id, null);
            Guid_BookModel? book2 = await Db.ScalarAsync<Guid_BookModel>(book.Id, null);

            //update book1
            book1!.Name = "Update Book1";
            await Db.UpdateAsync(book1, "test", null);

            //update book2
            try
            {
                book2!.Name = "Update book2";
                await Db.UpdateAsync(book2, "tester", null);
            }
            catch (DatabaseException ex)
            {
                Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);

                if (ex.ErrorCode != ErrorCodes.ConcurrencyConflict)
                {
                    throw;
                }
            }

            Guid_BookModel? book3 = await Db.ScalarAsync<Guid_BookModel>(book.Id, null);

            Assert.IsTrue(SerializeUtil.ToJson(book1) == SerializeUtil.ToJson(book3));
        }

        /// <summary>
        /// //NOTICE: 在sqlite下，重复update，返回1.即matched
        /// //NOTICE: 在mysql下，重复update，返回1，即mactched
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_Repeate_Update_Return()
        {
            Book2Model book = Mocker.GetBooks(1).First();

            await Db.AddAsync(book, "tester", null);

            Book2Model? book1 = await Db.ScalarAsync<Book2Model>(book.Id, null);

            var engine = DbManager.GetDatabaseEngine(DbSchema_Mysql);

            var connectionString = DbManager.GetConnectionString(DbSchema_Mysql, true);

            int rt = await engine.ExecuteCommandNonQueryAsync(connectionString,
                new EngineCommand($"update tb_Book set Name='Update_xxx' where Id = {book1!.Id}"));

            int rt2 = await engine.ExecuteCommandNonQueryAsync(connectionString,
                new EngineCommand($"update tb_Book set Name='Update_xxx' where Id = {book1.Id}"));

            int rt3 = await engine.ExecuteCommandNonQueryAsync(connectionString,
                new EngineCommand($"update tb_Book set Name='Update_xxx' where Id = {book1.Id}"));

            Assert.AreEqual(rt, rt2, rt3);
        }

        /// <summary>
        /// //NOTICE: Mysql执行多条语句的时候，ExecuteCommandReader只返回最后一个结果。
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_Mult_SQL_Return_With_Reader()
        {
            Guid_BookModel book = Mocker.Guid_GetBooks(1).First();
            book.Id = new Guid("cd5bda08-e5e2-409f-89c5-bea1ae49f2a0");

            await Db.AddAsync(book, "tester", null);

            long timestamp = TimeUtil.Timestamp;

            string sql = @$"
update tb_Guid_Book set LastUser='TTTgdTTTEEST' where Id = uuid_to_bin('08da5bcd-e2e5-9f40-89c5-bea1ae49f2a0') and Deleted = 0 and Timestamp={timestamp};
select count(1) from tb_Guid_Book where Id = uuid_to_bin('08da5bcd-e2e5-9f40-89c5-bea1ae49f2a0') and Deleted = 0;
";
            var engine = DbManager.GetDatabaseEngine(DbSchema_Mysql);

            using IDataReader reader = await engine.ExecuteCommandReaderAsync(DbManager.GetConnectionString(DbSchema_Mysql, true), new EngineCommand(sql));

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
        public async Task Guid_Test_01_Batch_Add_PublisherModelAsync()
        {
            IList<Guid_PublisherModel> publishers = Mocker.Guid_GetPublishers();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

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
        public async Task Guid_Test_02_Batch_Update_PublisherModelAsync()
        {
            TransactionContext transContext = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

            string updatedName = SecurityUtil.CreateUniqueToken();

            int count = 800;

            try
            {
                IEnumerable<Guid_PublisherModel> lst = await Db.RetrieveAllAsync<Guid_PublisherModel>(transContext, 0, count).ConfigureAwait(false);

                for (int i = 0; i < lst.Count(); i++)
                {
                    Guid_PublisherModel model = lst.ElementAt(i);
                    //model.Guid = Guid.NewGuid().ToString();
                    model.Type = PublisherType.Online;
                    model.Name = updatedName;
                    model.Books = new List<string>() { "xxx", "tttt" };
                    model.BookAuthors = new Dictionary<string, Author>()
                    {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                };
                }

                await Db.BatchUpdateAsync(lst, "lastUsre", transContext).ConfigureAwait(false);

                await Trans.CommitAsync(transContext).ConfigureAwait(false);

                lst = await Db.RetrieveAllAsync<Guid_PublisherModel>(null, 0, count);

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
        public async Task Guid_Test_03_Batch_Delete_PublisherModelAsync()
        {
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherModel> lst = (await Db.RetrieveAllAsync<Guid_PublisherModel>(transactionContext, 1, 5).ConfigureAwait(false)).ToList();

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
        public async Task Guid_Test_04_Add_PublisherModelAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherModel> lst = new List<Guid_PublisherModel>();

                for (int i = 0; i < 10; ++i)
                {
                    Guid_PublisherModel model = Mocker.Guid_MockOnePublisherModel();

                    await Db.AddAsync(model, "lastUsre", tContext).ConfigureAwait(false);

                    lst.Add(model);
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
        public async Task Guid_Test_05_Update_PublisherModelAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherModel> testModels = (await Db.RetrieveAllAsync<Guid_PublisherModel>(tContext, 1, 1).ConfigureAwait(false)).ToList();

                if (testModels.Count == 0)
                {
                    throw new Exception("No Model to update");
                }

                Guid_PublisherModel model = testModels[0];

                model.Books.Add("New Book2");
                //model.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Code = "Yuzhaobai" });

                await Db.UpdateAsync(model, "lastUsre", tContext).ConfigureAwait(false);

                Guid_PublisherModel? stored = await Db.ScalarAsync<Guid_PublisherModel>(model.Id, tContext).ConfigureAwait(false);

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
        public async Task Guid_Test_06_Delete_PublisherModelAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherModel> testModels = (await Db.RetrieveAllAsync<Guid_PublisherModel>(tContext).ConfigureAwait(false)).ToList();

                foreach (var model in testModels)
                {
                    await Db.DeleteAsync(model, "lastUsre", tContext).ConfigureAwait(false);
                }

                long count = await Db.CountAsync<Guid_PublisherModel>(tContext).ConfigureAwait(false);

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
            Guid_PublisherModel item = Mocker.Guid_MockOnePublisherModel();

            await Db.AddAsync(item, "xx", null).ConfigureAwait(false);

            var fetched = await Db.ScalarAsync<Guid_PublisherModel>(item.Id, null).ConfigureAwait(false);

            Assert.AreEqual(item.Timestamp, fetched!.Timestamp);

            fetched.Name = "ssssss";

            await Db.UpdateAsync(fetched, "xxx", null).ConfigureAwait(false);

            fetched = await Db.ScalarAsync<Guid_PublisherModel>(item.Id, null).ConfigureAwait(false);

            //await Db.AddOrUpdateAsync(item, "ss", null);

            fetched = await Db.ScalarAsync<Guid_PublisherModel>(item.Id, null).ConfigureAwait(false);

            //Batch

            var items = Mocker.Guid_GetPublishers();

            TransactionContext trans = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(items, "xx", trans).ConfigureAwait(false);

                var results = await Db.RetrieveAsync<Guid_PublisherModel>(item => SqlStatement.In(item.Id, true, items.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await Db.BatchUpdateAsync(items, "xx", trans).ConfigureAwait(false);

                var items2 = Mocker.Guid_GetPublishers();

                await Db.BatchAddAsync(items2, "xx", trans).ConfigureAwait(false);

                results = await Db.RetrieveAsync<Guid_PublisherModel>(item => SqlStatement.In(item.Id, true, items2.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

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
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                Guid_PublisherModel item = Mocker.Guid_MockOnePublisherModel();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                IList<Guid_PublisherModel> testModels = (await Db.RetrieveAllAsync<Guid_PublisherModel>(transactionContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testModels.Count == 0)
                {
                    throw new Exception("No Model to update");
                }

                Guid_PublisherModel model = testModels[0];

                model.Books.Add("New Book2");
                //model.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Code = "Yuzhaobai" });

                await Db.UpdateAsync(model, "lastUsre", transactionContext).ConfigureAwait(false);

                Guid_PublisherModel? stored = await Db.ScalarAsync<Guid_PublisherModel>(model.Id, transactionContext).ConfigureAwait(false);

                item = Mocker.Guid_MockOnePublisherModel();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                var fetched = await Db.ScalarAsync<Guid_PublisherModel>(item.Id, transactionContext).ConfigureAwait(false);

                Assert.AreEqual(item.Timestamp, fetched!.Timestamp);

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
            IList<Guid_PublisherModel> publishers = Mocker.Guid_GetPublishers();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

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

            IEnumerable<Guid_PublisherModel> publisherModels = await Db.RetrieveAsync<Guid_PublisherModel>(p => p.Type == PublisherType.Big && p.LastUser == "lastUsre", null).ConfigureAwait(false);

            Assert.IsTrue(publisherModels.Any() && publisherModels.All(p => p.Type == PublisherType.Big));
        }

        [TestMethod]
        public async Task Guid_Test_11_StartWith_TestAsync()
        {
            IList<Guid_PublisherModel> publishers = Mocker.Guid_GetPublishers();

            foreach (var model in publishers)
            {
                model.Name = "StartWithTest_xxx";
            }

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

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

            IEnumerable<Guid_PublisherModel> models = await Db.RetrieveAsync<Guid_PublisherModel>(t => t.Name.StartsWith("Star"), null);

            Assert.IsTrue(models.Any());

            Assert.IsTrue(models.All(t => t.Name.StartsWith("Star")));
        }

        [TestMethod]
        public async Task Guid_Test_12_Binary_TestAsync()
        {
            IList<Guid_PublisherModel> publishers = Mocker.Guid_GetPublishers();

            foreach (var model in publishers)
            {
                model.Name = "StartWithTest_xxx";
            }

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherModel>().ConfigureAwait(false);

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

            IEnumerable<Guid_PublisherModel> models = await Db.RetrieveAsync<Guid_PublisherModel>(
                t => t.Name.StartsWith("Star") && publishers.Any(), null);

            //IEnumerable<Guid_PublisherModel> models = await Db.RetrieveAsync<Guid_PublisherModel>(
            //    t => ReturnGuid() == ReturnGuid(), null);

            Assert.IsTrue(models.Any());

            Assert.IsTrue(models.All(t => t.Name.StartsWith("Star")));
        }

        [TestMethod]
        public async Task Guid_Test_13_Mapper_ToModelAsync()
        {
            Globals.Logger.LogDebug($"��ǰProcess,{Environment.ProcessId}");

            #region Json验证1

            var publisher3 = new Guid_PublisherModel();

            await Db.AddAsync(publisher3, "sss", null).ConfigureAwait(false);

            var stored3 = await Db.ScalarAsync<Guid_PublisherModel>(publisher3.Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher3), SerializeUtil.ToJson(stored3));

            #endregion

            #region Json验证2

            var publisher2s = Mocker.Guid_GetPublishers2();

            foreach (Guid_PublisherModel2 publisher in publisher2s)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            Guid_PublisherModel2? publisher2 = await Db.ScalarAsync<Guid_PublisherModel2>(publisher2s[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher2), SerializeUtil.ToJson(publisher2s[0]));

            #endregion

            #region Json验证3

            var publishers = Mocker.Guid_GetPublishers();

            foreach (Guid_PublisherModel publisher in publishers)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            Guid_PublisherModel? publisher1 = await Db.ScalarAsync<Guid_PublisherModel>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));

            #endregion
        }

        [TestMethod]
        public async Task Guid_Test_14_Mapper_ToModel_PerformanceAsync()
        {
            var books = Mocker.Guid_GetBooks(50);

            var trans = await Trans.BeginTransactionAsync<Guid_BookModel>().ConfigureAwait(false);

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

            using MySqlConnection mySqlConnection = new MySqlConnection(DbManager.GetConnectionString(DbSchema_Mysql, true).ToString());

            TypeHandlerHelper.AddTypeHandlerImpl(typeof(DateTimeOffset), new DateTimeOffsetTypeHandler(), false);
            TypeHandlerHelper.AddTypeHandlerImpl(typeof(Guid), new MySqlGuidTypeHandler(), false);

            //time = 0;
            int loop = 1;

            TimeSpan time0 = TimeSpan.Zero, time1 = TimeSpan.Zero, time2 = TimeSpan.Zero, time3 = TimeSpan.Zero;
            for (int cur = 0; cur < loop; ++cur)
            {
                await mySqlConnection.OpenAsync().ConfigureAwait(false);

                using MySqlCommand command0 = new MySqlCommand("select * from tb_Guid_Book limit 5000", mySqlConnection);

                var reader = await command0.ExecuteReaderAsync().ConfigureAwait(false);

                List<Guid_BookModel> list1 = new List<Guid_BookModel>();
                List<Guid_BookModel> list2 = new List<Guid_BookModel>();
                List<Guid_BookModel> list3 = new List<Guid_BookModel>();

                int len = reader.FieldCount;
                DbModelPropertyDef[] propertyDefs = new DbModelPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                DbModelDef definition = Db.ModelDefFactory.GetDef<Guid_BookModel>()!;

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetDbPropertyDef(reader.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }

                Func<IDbModelDefFactory, IDataReader, object> fullStack_mapper = DbModelConvert.CreateDataReaderRowToModelDelegate(definition, reader, 0, definition.FieldCount, false);

                Func<IDataReader, object> dapper_mapper = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(Guid_BookModel), reader);

                Func<IDataReader, object> reflection_mapper = (r) =>
                {
                    Guid_BookModel item = new Guid_BookModel();

                    for (int i = 0; i < len; ++i)
                    {
                        DbModelPropertyDef property = propertyDefs[i];

                        object? value = DbPropertyConvert.DbFieldValueToPropertyValue(r[i], property, EngineType.MySQL);

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
                    object obj1 = fullStack_mapper(Db.ModelDefFactory, reader);
                    list1.Add((Guid_BookModel)obj1);
                    stopwatch1.Stop();

                    stopwatch2.Start();
                    object obj2 = dapper_mapper(reader);
                    list2.Add((Guid_BookModel)obj2);
                    stopwatch2.Stop();

                    stopwatch3.Start();
                    object obj3 = reflection_mapper(reader);
                    list3.Add((Guid_BookModel)obj3);
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
            Guid_PublisherModel publisherModel = Mocker.Guid_MockOnePublisherModel();
            //publisherModel.Version = 0;

            var emit_results = publisherModel.ToDbParameters(Db.ModelDefFactory.GetDef<Guid_PublisherModel>()!, Db.ModelDefFactory, 1);

            var reflect_results = publisherModel.ToDbParametersUsingReflection(Db.ModelDefFactory.GetDef<Guid_PublisherModel>()!, 1);

            AssertEqual(emit_results, reflect_results, EngineType.MySQL);

            //PublisherModel2

            Guid_PublisherModel2 publisherModel2 = new Guid_PublisherModel2();

            IList<KeyValuePair<string, object>>? emit_results2 = publisherModel2.ToDbParameters(Db.ModelDefFactory.GetDef<Guid_PublisherModel2>()!, Db.ModelDefFactory, 1);

            var reflect_results2 = publisherModel2.ToDbParametersUsingReflection(Db.ModelDefFactory.GetDef<Guid_PublisherModel2>()!, 1);

            AssertEqual(emit_results2, reflect_results2, EngineType.MySQL);

            //PublisherModel3

            Guid_PublisherModel3 publisherModel3 = new Guid_PublisherModel3();

            var emit_results3 = publisherModel3.ToDbParameters(Db.ModelDefFactory.GetDef<Guid_PublisherModel3>()!, Db.ModelDefFactory, 1);

            var reflect_results3 = publisherModel3.ToDbParametersUsingReflection(Db.ModelDefFactory.GetDef<Guid_PublisherModel3>()!, 1);

            AssertEqual(emit_results3, reflect_results3, EngineType.MySQL);
        }

        [TestMethod]
        public void Guid_Test_16_Mapper_ToParameter_Performance()
        {
            var models = Mocker.Guid_GetPublishers(1000000);

            var def = Db.ModelDefFactory.GetDef<Guid_PublisherModel>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Restart();
            foreach (var model in models)
            {
                _ = model.ToDbParameters(def!, Db.ModelDefFactory);
            }
            stopwatch.Stop();

            Console.WriteLine($"Emit: {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            foreach (var model in models)
            {
                _ = model.ToDbParametersUsingReflection(def!);
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

                Assert.IsTrue(DbPropertyConvert.DoNotUseUnSafePropertyValueToDbFieldValueStatement(dict[kv.Key].Value, false, engineType) ==

                    DbPropertyConvert.DoNotUseUnSafePropertyValueToDbFieldValueStatement(kv.Value, false, engineType));
            }
        }

        public static Guid ReturnGuid()
        {
            return Guid.NewGuid();
        }
    }
}