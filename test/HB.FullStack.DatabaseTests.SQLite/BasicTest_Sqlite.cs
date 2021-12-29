using ClassLibrary1;

using HB.FullStack.Database;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;
using HB.FullStack.DatabaseTests.Data;

using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    public class BasicTest_Sqlite : BaseTestClass
    {
        [TestMethod]
        public async Task Test_1_Batch_Add_PublisherEntityAsync()
        {
            IList<PublisherEntity_Client> publishers = Mocker.GetPublishers_Client();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<PublisherEntity_Client>().ConfigureAwait(false);

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
        public async Task Test_2_Batch_Update_PublisherEntityAsync()
        {
            TransactionContext transContext = await Trans.BeginTransactionAsync<PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                IEnumerable<PublisherEntity_Client> lst = await Db.RetrieveAllAsync<PublisherEntity_Client>(transContext).ConfigureAwait(false);

                for (int i = 0; i < lst.Count(); i += 2)
                {
                    PublisherEntity_Client entity = lst.ElementAt(i);
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
        public async Task Test_3_Batch_Delete_PublisherEntityAsync()
        {
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity_Client> lst = (await Db.RetrieveAllAsync<PublisherEntity_Client>(transactionContext, 1, 100).ConfigureAwait(false)).ToList();

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
        public async Task Test_4_Add_PublisherEntityAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity_Client> lst = new List<PublisherEntity_Client>();

                for (int i = 0; i < 10; ++i)
                {
                    PublisherEntity_Client entity = Mocker.MockOnePublisherEntity_Client();

                    await Db.AddAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                    lst.Add(entity);
                }

                await Trans.CommitAsync(tContext).ConfigureAwait(false);

                Assert.IsTrue(lst.All(p => p.Version == 0));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(tContext).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_5_Update_PublisherEntityAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<PublisherEntity_Client>().ConfigureAwait(false);

            IList<PublisherEntity_Client> publishers = Mocker.GetPublishers_Client();

            try
            {
                await Db.BatchAddAsync(publishers, "lastUsre", tContext).ConfigureAwait(false);

                IList<PublisherEntity_Client> testEntities = (await Db.RetrieveAllAsync<PublisherEntity_Client>(tContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                PublisherEntity_Client entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await Db.UpdateAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                PublisherEntity_Client? stored = await Db.ScalarAsync<PublisherEntity_Client>(entity.Id, tContext).ConfigureAwait(false);

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
        public async Task Test_6_Delete_PublisherEntityAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity_Client> testEntities = (await Db.RetrieveAllAsync<PublisherEntity_Client>(tContext).ConfigureAwait(false)).ToList();

                foreach (PublisherEntity_Client? entity in testEntities)
                {
                    await Db.DeleteAsync(entity, "lastUsre", tContext).ConfigureAwait(false);
                }

                long count = await Db.CountAsync<PublisherEntity_Client>(tContext).ConfigureAwait(false);

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
            PublisherEntity_Client item = Mocker.MockOnePublisherEntity_Client();

            await Db.AddAsync(item, "xx", null).ConfigureAwait(false);

            PublisherEntity_Client? fetched = await Db.ScalarAsync<PublisherEntity_Client>(item.Id, null).ConfigureAwait(false);

            Assert.AreEqual(item.LastTime, fetched!.LastTime);

            fetched.Name = "ssssss";

            await Db.UpdateAsync(fetched, "xxx", null).ConfigureAwait(false);

            fetched = await Db.ScalarAsync<PublisherEntity_Client>(item.Id, null).ConfigureAwait(false);

            //await Db.AddOrUpdateAsync(item, "ss", null);

            fetched = await Db.ScalarAsync<PublisherEntity_Client>(item.Id, null).ConfigureAwait(false);

            //Batch

            List<PublisherEntity_Client>? items = Mocker.GetPublishers_Client();

            TransactionContext trans = await Trans.BeginTransactionAsync<PublisherEntity_Client>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(items, "xx", trans).ConfigureAwait(false);

                IEnumerable<PublisherEntity_Client>? results = await Db.RetrieveAsync<PublisherEntity_Client>(item => SqlStatement.In(item.Id, true, items.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await Db.BatchUpdateAsync(items, "xx", trans).ConfigureAwait(false);

                List<PublisherEntity_Client>? items2 = Mocker.GetPublishers_Client();

                await Db.BatchAddAsync(items2, "xx", trans).ConfigureAwait(false);

                results = await Db.RetrieveAsync<PublisherEntity_Client>(item => SqlStatement.In(item.Id, true, items2.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

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
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<PublisherEntity_Client>().ConfigureAwait(false);
            //TransactionContext? transactionContext = null;

            try
            {
                PublisherEntity_Client item = Mocker.MockOnePublisherEntity_Client();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                //await Db.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);

                await Db.DeleteAsync(item, "xxx", transactionContext).ConfigureAwait(false);

                IList<PublisherEntity_Client> testEntities = (await Db.RetrieveAllAsync<PublisherEntity_Client>(transactionContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                PublisherEntity_Client entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await Db.UpdateAsync(entity, "lastUsre", transactionContext).ConfigureAwait(false);

                PublisherEntity_Client? stored = await Db.ScalarAsync<PublisherEntity_Client>(entity.Id, transactionContext).ConfigureAwait(false);

                item = Mocker.MockOnePublisherEntity_Client();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                PublisherEntity_Client? fetched = await Db.ScalarAsync<PublisherEntity_Client>(item.Id, transactionContext).ConfigureAwait(false);

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
        public async Task Test_EntityMapperAsync()
        {
            #region

            PublisherEntity3_Client? publisher3 = new PublisherEntity3_Client();

            await Db.AddAsync(publisher3, "sss", null).ConfigureAwait(false);

            PublisherEntity3_Client? stored3 = await Db.ScalarAsync<PublisherEntity3_Client>(publisher3.Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher3), SerializeUtil.ToJson(stored3));

            #endregion

            #region

            IList<PublisherEntity2_Client>? publishers2 = Mocker.GetPublishers2_Client();

            foreach (PublisherEntity2_Client publisher in publishers2)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            PublisherEntity2_Client? publisher2 = await Db.ScalarAsync<PublisherEntity2_Client>(publishers2[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher2), SerializeUtil.ToJson(publishers2[0]));

            #endregion

            #region

            List<PublisherEntity_Client>? publishers = Mocker.GetPublishers_Client();

            foreach (PublisherEntity_Client publisher in publishers)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            PublisherEntity_Client? publisher1 = await Db.ScalarAsync<PublisherEntity_Client>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));
            #endregion
        }

        [TestMethod]
        public async Task Test_EntityMapperPerformanceAsync()
        {
            IList<BookEntity_Client>? books = Mocker.GetBooks_Client(500);

            TransactionContext? trans = await Trans.BeginTransactionAsync<BookEntity_Client>().ConfigureAwait(false);

            IEnumerable<BookEntity_Client> re = await Db.RetrieveAsync<BookEntity_Client>(b => b.Deleted, trans).ConfigureAwait(false);

            await Db.AddAsync(Mocker.GetBooks_Client(1)[0], "", trans).ConfigureAwait(false);

            try
            {
                //await Db.AddAsync<BookEntity>(books[0], "", trans);

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

                using SqliteCommand command0 = new SqliteCommand("select * from tb_bookentity_client limit 1000", mySqlConnection);

                SqliteDataReader? reader0 = await command0.ExecuteReaderAsync().ConfigureAwait(false);

                List<BookEntity_Client> list1 = new List<BookEntity_Client>();
                List<BookEntity_Client> list2 = new List<BookEntity_Client>();
                List<BookEntity_Client> list3 = new List<BookEntity_Client>();

                int len = reader0.FieldCount;
                EntityPropertyDef[] propertyDefs = new EntityPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                EntityDef definition = Db.EntityDefFactory.GetDef<BookEntity_Client>()!;

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetPropertyDef(reader0.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }

                Func<IEntityDefFactory, IDataReader, object> mapper1 = EntityMapperDelegateCreator.CreateToEntityDelegate(definition, reader0, 0, definition.FieldCount, false, Database.Engine.EngineType.SQLite);

                //Warning: 如果用Dapper，小心DateTimeOffset的存储，会丢失offset，然后转回来时候，会加上当地时间的offset
                TypeHandlerHelper.AddTypeHandlerImpl(typeof(DateTimeOffset), new DateTimeOffsetTypeHandler(), false);
                Func<IDataReader, object> mapper2 = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(BookEntity_Client), reader0);

                Stopwatch stopwatch1 = new Stopwatch();
                Stopwatch stopwatch2 = new Stopwatch();
                Stopwatch stopwatch3 = new Stopwatch();

                while (reader0.Read())
                {
                    stopwatch1.Start();

                    object obj1 = mapper1(Db.EntityDefFactory, reader0);

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

            Console.WriteLine("Emit Coding : " + (time1.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("Dapper : " + (time2.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("Reflection : " + (time3.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public async Task Test_10_Enum_TestAsync()
        {
            IList<PublisherEntity> publishers = Mocker.GetPublishers();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

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

            IEnumerable<PublisherEntity> publisherEntities = await Db.RetrieveAsync<PublisherEntity>(p => p.Type == PublisherType.Big, null).ConfigureAwait(false);

            Assert.IsTrue(publisherEntities.All(p => p.Type == PublisherType.Big));
        }

        [TestMethod]
        public void TestSQLite_Changes_Test()
        {
            string connectString = $"Data Source={DbName}";
            using SqliteConnection conn = new SqliteConnection(connectString);
            conn.Open();

            long id = TimeUtil.UtcNowTicks.Ticks;

            string insertCommandText = $"insert into tb_publisherentity(`Name`, `LastTime`, `Id`, `Version`) values('FSFSF', 100, '{id}', 1)";

            using SqliteCommand insertCommand = new SqliteCommand(insertCommandText, conn);

            insertCommand.ExecuteScalar();

            string commandText = $"update `tb_publisherentity` set  `Name`='{new Random().NextDouble()}', `Version`=2 WHERE `Id`='{id}' ;";

            using SqliteCommand mySqlCommand1 = new SqliteCommand(commandText, conn);

            int rt1 = mySqlCommand1.ExecuteNonQuery();

            using SqliteCommand rowCountCommand1 = new SqliteCommand("select changes()", conn);

            long? rowCount1 = (long?)rowCountCommand1.ExecuteScalar();

            using SqliteCommand mySqlCommand2 = new SqliteCommand(commandText, conn);

            int rt2 = mySqlCommand1.ExecuteNonQuery();

            using SqliteCommand rowCountCommand2 = new SqliteCommand("select changes()", conn);

            long? rowCount2 = (long?)rowCountCommand2.ExecuteScalar();

            Assert.AreEqual(rt1, rt2);
            Assert.AreEqual(rowCount1, rowCount2);
        }
    }
}