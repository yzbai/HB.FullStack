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
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;
using HB.FullStack.DatabaseTests.Data;

using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests.SQLite
{
    [TestClass]
    public class BasicTest_Sqlite_Guid : BaseTestClass
    {
        [TestMethod]
        public async Task Test_1_Batch_Add_PublisherModelAsync()
        {
            IList<Guid_PublisherModel_Client> publishers = Mocker.Guid_GetPublishers_Client();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherModel_Client>().ConfigureAwait(false);

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

        /// <summary>
        /// //NOTICE: Mysql执行多条语句的时候，ExecuteCommandReader只返回最后一个结果。Sqlite同样
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_Mult_SQL_Return_With_Reader()
        {
            Guid_BookModel book = Mocker.Guid_GetBooks(1).First();

            await Db.AddAsync(book, "tester", null);

            long timestamp = TimeUtil.UtcNowTicks;
            string sql = $@"
update tb_guid_bookmodel set LastUser='TTTgdTTTEEST' where Id = '{book.Id}' and Deleted = 0 and Timestamp={timestamp};
select count(1) from tb_guid_bookmodel where Id = '{book.Id}' and Deleted = 0;
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
        public async Task Test_2_Batch_Update_PublisherModelAsync()
        {
            TransactionContext transContext = await Trans.BeginTransactionAsync<Guid_PublisherModel_Client>().ConfigureAwait(false);

            try
            {
                IEnumerable<Guid_PublisherModel_Client> lst = await Db.RetrieveAllAsync<Guid_PublisherModel_Client>(transContext).ConfigureAwait(false);

                for (int i = 0; i < lst.Count(); i += 2)
                {
                    Guid_PublisherModel_Client model = lst.ElementAt(i);
                    //model.Guid = Guid.NewGuid().ToString();
                    model.Type = PublisherType.Online;
                    model.Name = "中sfasfaf文名字";
                    model.Books = new List<string>() { "xxx", "tttt" };
                    model.BookAuthors = new Dictionary<string, Author>()
                    {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                };
                }

                await Db.BatchUpdateAsync(lst, "lastUsre", transContext).ConfigureAwait(false);

                await Trans.CommitAsync(transContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(transContext).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_3_Batch_Delete_PublisherModelAsync()
        {
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherModel_Client>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherModel_Client> lst = (await Db.RetrieveAllAsync<Guid_PublisherModel_Client>(transactionContext, 1, 100).ConfigureAwait(false)).ToList();

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
        public async Task Test_4_Add_PublisherModelAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<Guid_PublisherModel_Client>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherModel_Client> lst = new List<Guid_PublisherModel_Client>();

                for (int i = 0; i < 10; ++i)
                {
                    Guid_PublisherModel_Client model = Mocker.Guid_MockOnePublisherModel_Client();

                    await Db.AddAsync(model, "lastUsre", tContext).ConfigureAwait(false);

                    lst.Add(model);
                }

                await Trans.CommitAsync(tContext).ConfigureAwait(false);

                //Assert.IsTrue(lst.All(p => p.Version == 0));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(tContext).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_5_Update_PublisherModelAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<Guid_PublisherModel_Client>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherModel_Client> testModels = (await Db.RetrieveAllAsync<Guid_PublisherModel_Client>(tContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testModels.Count == 0)
                {
                    throw new Exception("No Model to update");
                }

                Guid_PublisherModel_Client model = testModels[0];

                model.Books.Add("New Book2");
                //model.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Code = "Yuzhaobai" });

                await Db.UpdateAsync(model, "lastUsre", tContext).ConfigureAwait(false);

                Guid_PublisherModel_Client? stored = await Db.ScalarAsync<Guid_PublisherModel_Client>(model.Id, tContext).ConfigureAwait(false);

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
        public async Task Test_6_Delete_PublisherModelAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<Guid_PublisherModel_Client>().ConfigureAwait(false);

            try
            {
                IList<Guid_PublisherModel_Client> testModels = (await Db.RetrieveAllAsync<Guid_PublisherModel_Client>(tContext).ConfigureAwait(false)).ToList();

                foreach (Guid_PublisherModel_Client? model in testModels)
                {
                    await Db.DeleteAsync(model, "lastUsre", tContext).ConfigureAwait(false);
                }

                long count = await Db.CountAsync<Guid_PublisherModel_Client>(tContext).ConfigureAwait(false);

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
        public async Task Test_8_LastTimeTestAsync()
        {
            Guid_PublisherModel_Client item = Mocker.Guid_MockOnePublisherModel_Client();

            await Db.AddAsync(item, "xx", null).ConfigureAwait(false);

            Guid_PublisherModel_Client? fetched = await Db.ScalarAsync<Guid_PublisherModel_Client>(item.Id, null).ConfigureAwait(false);

            Assert.AreEqual(item.Timestamp, fetched!.Timestamp);

            fetched.Name = "ssssss";

            await Db.UpdateAsync(fetched, "xxx", null).ConfigureAwait(false);

            fetched = await Db.ScalarAsync<Guid_PublisherModel_Client>(item.Id, null).ConfigureAwait(false);

            //await Db.AddOrUpdateAsync(item, "ss", null);

            fetched = await Db.ScalarAsync<Guid_PublisherModel_Client>(item.Id, null).ConfigureAwait(false);

            //Batch

            List<Guid_PublisherModel_Client>? items = Mocker.Guid_GetPublishers_Client();

            TransactionContext trans = await Trans.BeginTransactionAsync<Guid_PublisherModel_Client>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(items, "xx", trans).ConfigureAwait(false);

                IEnumerable<Guid_PublisherModel_Client>? results = await Db.RetrieveAsync<Guid_PublisherModel_Client>(item => SqlStatement.In(item.Id, true, items.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await Db.BatchUpdateAsync(items, "xx", trans).ConfigureAwait(false);

                List<Guid_PublisherModel_Client>? items2 = Mocker.Guid_GetPublishers_Client();

                await Db.BatchAddAsync(items2, "xx", trans).ConfigureAwait(false);

                results = await Db.RetrieveAsync<Guid_PublisherModel_Client>(item => SqlStatement.In(item.Id, true, items2.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

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

        [TestMethod]
        public async Task Test_9_UpdateLastTimeTestAsync()
        {
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Guid_PublisherModel_Client>().ConfigureAwait(false);
            //TransactionContext? transactionContext = null;

            try
            {
                Guid_PublisherModel_Client item = Mocker.Guid_MockOnePublisherModel_Client();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                //await Db.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);

                await Db.DeleteAsync(item, "xxx", transactionContext).ConfigureAwait(false);

                IList<Guid_PublisherModel_Client> testModels = (await Db.RetrieveAllAsync<Guid_PublisherModel_Client>(transactionContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testModels.Count == 0)
                {
                    throw new Exception("No Model to update");
                }

                Guid_PublisherModel_Client model = testModels[0];

                model.Books.Add("New Book2");
                //model.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Code = "Yuzhaobai" });

                await Db.UpdateAsync(model, "lastUsre", transactionContext).ConfigureAwait(false);

                Guid_PublisherModel_Client? stored = await Db.ScalarAsync<Guid_PublisherModel_Client>(model.Id, transactionContext).ConfigureAwait(false);

                item = Mocker.Guid_MockOnePublisherModel_Client();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                Guid_PublisherModel_Client? fetched = await Db.ScalarAsync<Guid_PublisherModel_Client>(item.Id, transactionContext).ConfigureAwait(false);

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
        public async Task Test_ModelMapperAsync()
        {
            #region

            PublisherModel3_Client? publisher3 = new PublisherModel3_Client();

            await Db.AddAsync(publisher3, "sss", null).ConfigureAwait(false);

            PublisherModel3_Client? stored3 = await Db.ScalarAsync<PublisherModel3_Client>(publisher3.Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher3), SerializeUtil.ToJson(stored3));

            #endregion

            #region

            IList<PublisherModel2_Client>? publishers2 = Mocker.GetPublishers2_Client();

            foreach (PublisherModel2_Client publisher in publishers2)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            PublisherModel2_Client? publisher2 = await Db.ScalarAsync<PublisherModel2_Client>(publishers2[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher2), SerializeUtil.ToJson(publishers2[0]));

            #endregion

            #region

            List<PublisherModel_Client>? publishers = Mocker.GetPublishers_Client();

            foreach (PublisherModel_Client publisher in publishers)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            PublisherModel_Client? publisher1 = await Db.ScalarAsync<PublisherModel_Client>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));
            #endregion
        }

        [TestMethod]
        public async Task Test_ModelMapperPerformanceAsync()
        {
            IList<BookModel_Client>? books = Mocker.GetBooks_Client(500);

            TransactionContext? trans = await Trans.BeginTransactionAsync<BookModel_Client>().ConfigureAwait(false);

            IEnumerable<BookModel_Client> re = await Db.RetrieveAsync<BookModel_Client>(b => b.Deleted, trans).ConfigureAwait(false);

            await Db.AddAsync(Mocker.GetBooks_Client(1)[0], "", trans).ConfigureAwait(false);

            try
            {
                //await Db.AddAsync<BookModel>(books[0], "", trans);

                await Db.BatchAddAsync(books, "x", trans).ConfigureAwait(false);
                await Trans.CommitAsync(trans).ConfigureAwait(false);
            }
            catch
            {
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
            }

            Stopwatch stopwatch = new Stopwatch();

            using SqliteConnection mySqlConnection = new SqliteConnection($"Data Source={DbName}");

            //time = 0;
            int loop = 10;

            TimeSpan time0 = TimeSpan.Zero, time1 = TimeSpan.Zero, time2 = TimeSpan.Zero, time3 = TimeSpan.Zero;
            for (int cur = 0; cur < loop; ++cur)
            {
                await mySqlConnection.OpenAsync().ConfigureAwait(false);

                using SqliteCommand command0 = new SqliteCommand("select * from tb_bookmodel_client limit 1000", mySqlConnection);

                SqliteDataReader? reader0 = await command0.ExecuteReaderAsync().ConfigureAwait(false);

                List<BookModel_Client> list1 = new List<BookModel_Client>();
                List<BookModel_Client> list2 = new List<BookModel_Client>();
                List<BookModel_Client> list3 = new List<BookModel_Client>();

                int len = reader0.FieldCount;
                DbModelPropertyDef[] propertyDefs = new DbModelPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                DbModelDef definition = Db.ModelDefFactory.GetDef<BookModel_Client>()!;

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetPropertyDef(reader0.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }

                Func<IDbModelDefFactory, IDataReader, object> mapper1 = DbModelConvert.CreateDataReaderRowToModelDelegate(definition, reader0, 0, definition.FieldCount, false, Database.Engine.EngineType.SQLite);

                //Warning: 如果用Dapper，小心DateTimeOffset的存储，会丢失offset，然后转回来时候，会加上当地时间的offset
                TypeHandlerHelper.AddTypeHandlerImpl(typeof(DateTimeOffset), new DateTimeOffsetTypeHandler(), false);
                Func<IDataReader, object> mapper2 = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(BookModel_Client), reader0);

                Stopwatch stopwatch1 = new Stopwatch();
                Stopwatch stopwatch2 = new Stopwatch();
                Stopwatch stopwatch3 = new Stopwatch();

                while (reader0.Read())
                {
                    stopwatch1.Start();

                    object obj1 = mapper1(Db.ModelDefFactory, reader0);

                    list1.Add((BookModel_Client)obj1);
                    stopwatch1.Stop();

                    stopwatch2.Start();
                    object obj2 = mapper2(reader0);

                    list2.Add((BookModel_Client)obj2);
                    stopwatch2.Stop();

                    stopwatch3.Start();

                    BookModel_Client item = new BookModel_Client();

                    for (int i = 0; i < len; ++i)
                    {
                        DbModelPropertyDef property = propertyDefs[i];

                        object? value = DbValueConvert.DbValueToTypeValue(reader0[i], property, Database.Engine.EngineType.SQLite);

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

            Console.WriteLine("Emit Coding : " + (time1.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("Dapper : " + (time2.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("Reflection : " + (time3.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public async Task Test_10_Enum_TestAsync()
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

            IEnumerable<Guid_PublisherModel> publisherModels = await Db.RetrieveAsync<Guid_PublisherModel>(p => p.Type == PublisherType.Big, null).ConfigureAwait(false);

            Assert.IsTrue(publisherModels.All(p => p.Type == PublisherType.Big));
        }
    }
}