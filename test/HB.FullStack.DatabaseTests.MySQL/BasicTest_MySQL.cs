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
    public class BasicTest_MySQL : BaseTestClass
    {
        [TestMethod]
        public async Task Test_1_Batch_Add_PublisherEntityAsync()
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
        }

        [TestMethod]
        public async Task Test_2_Batch_Update_PublisherEntityAsync()
        {
            TransactionContext transContext = await Trans.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                IEnumerable<PublisherEntity> lst = await Db.RetrieveAllAsync<PublisherEntity>(transContext).ConfigureAwait(false);

                for (int i = 0; i < lst.Count(); i += 2)
                {
                    PublisherEntity entity = lst.ElementAt(i);
                    //entity.Guid = Guid.NewGuid().ToString();
                    entity.Type = PublisherType.Online;
                    entity.Name = "Name_xxx";
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
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity> lst = (await Db.RetrieveAllAsync<PublisherEntity>(transactionContext, 1, 100).ConfigureAwait(false)).ToList();

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
            TransactionContext tContext = await Trans.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity> lst = new List<PublisherEntity>();

                for (int i = 0; i < 10; ++i)
                {
                    PublisherEntity entity = Mocker.MockOnePublisherEntity();

                    await Db.AddAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                    lst.Add(entity);
                }

                await Trans.CommitAsync(tContext).ConfigureAwait(false);

                Assert.IsTrue(lst.All(p => p.Id > 0));
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
            TransactionContext tContext = await Trans.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity> testEntities = (await Db.RetrieveAllAsync<PublisherEntity>(tContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await Db.UpdateAsync(entity, "lastUsre", tContext).ConfigureAwait(false);

                PublisherEntity? stored = await Db.ScalarAsync<PublisherEntity>(entity.Id, tContext).ConfigureAwait(false);

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
            TransactionContext tContext = await Trans.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                IList<PublisherEntity> testEntities = (await Db.RetrieveAllAsync<PublisherEntity>(tContext).ConfigureAwait(false)).ToList();

                foreach (var entity in testEntities)
                {
                    await Db.DeleteAsync(entity, "lastUsre", tContext).ConfigureAwait(false);
                }

                long count = await Db.CountAsync<PublisherEntity>(tContext).ConfigureAwait(false);

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
            PublisherEntity item = Mocker.MockOnePublisherEntity();

            await Db.AddAsync(item, "xx", null).ConfigureAwait(false);

            var fetched = await Db.ScalarAsync<PublisherEntity>(item.Id, null).ConfigureAwait(false);

            Assert.AreEqual(item.LastTime, fetched!.LastTime);

            fetched.Name = "ssssss";

            await Db.UpdateAsync(fetched, "xxx", null).ConfigureAwait(false);

            fetched = await Db.ScalarAsync<PublisherEntity>(item.Id, null).ConfigureAwait(false);

            //await Db.AddOrUpdateAsync(item, "ss", null);

            fetched = await Db.ScalarAsync<PublisherEntity>(item.Id, null).ConfigureAwait(false);

            //Batch

            var items = Mocker.GetPublishers();

            TransactionContext trans = await Trans.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(items, "xx", trans).ConfigureAwait(false);

                var results = await Db.RetrieveAsync<PublisherEntity>(item => SqlStatement.In(item.Id, true, items.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await Db.BatchUpdateAsync(items, "xx", trans).ConfigureAwait(false);

                var items2 = Mocker.GetPublishers();

                await Db.BatchAddAsync(items2, "xx", trans).ConfigureAwait(false);

                results = await Db.RetrieveAsync<PublisherEntity>(item => SqlStatement.In(item.Id, true, items2.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

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
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);
            //TransactionContext? transactionContext = null;

            try
            {
                PublisherEntity item = Mocker.MockOnePublisherEntity();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                IList<PublisherEntity> testEntities = (await Db.RetrieveAllAsync<PublisherEntity>(transactionContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testEntities.Count == 0)
                {
                    throw new Exception("No Entity to update");
                }

                PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                //entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                await Db.UpdateAsync(entity, "lastUsre", transactionContext).ConfigureAwait(false);

                PublisherEntity? stored = await Db.ScalarAsync<PublisherEntity>(entity.Id, transactionContext).ConfigureAwait(false);

                item = Mocker.MockOnePublisherEntity();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                var fetched = await Db.ScalarAsync<PublisherEntity>(item.Id, transactionContext).ConfigureAwait(false);

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
            GlobalSettings.Logger.LogDebug($"Process,{Environment.ProcessId}");

            #region

            var publisher3 = new PublisherEntity3();

            await Db.AddAsync(publisher3, "sss", null).ConfigureAwait(false);

            var stored3 = await Db.ScalarAsync<PublisherEntity3>(publisher3.Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher3), SerializeUtil.ToJson(stored3));

            #endregion

            #region

            var publishers2 = Mocker.GetPublishers2();

            foreach (PublisherEntity2 publisher in publishers2)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            PublisherEntity2? publisher2 = await Db.ScalarAsync<PublisherEntity2>(publishers2[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher2), SerializeUtil.ToJson(publishers2[0]));

            #endregion

            #region

            var publishers = Mocker.GetPublishers();

            foreach (PublisherEntity publisher in publishers)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            PublisherEntity? publisher1 = await Db.ScalarAsync<PublisherEntity>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));
            #endregion
        }

        [TestMethod]
        public async Task Test_EntityMapperPerformanceAsync()
        {
            var books = Mocker.GetBooks(50);

            var trans = await Trans.BeginTransactionAsync<BookEntity>().ConfigureAwait(false);

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
            int loop = 10;

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

                EntityDef definition = Db.EntityDefFactory.GetDef<BookEntity>()!;

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetPropertyDef(reader0.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }

                Func<IEntityDefFactory, IDataReader, object> mapper1 = EntityMapperDelegateCreator.CreateToEntityDelegate(definition, reader0, 0, definition.FieldCount, false, EngineType.MySQL);

                //Warning: �����Dapper��С��DateTimeOffset�Ĵ洢���ᶪʧoffset��Ȼ��ת����ʱ�򣬻���ϵ���ʱ���offset
                Func<IDataReader, object> mapper2 = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(BookEntity), reader0);

                Stopwatch stopwatch1 = new Stopwatch();
                Stopwatch stopwatch2 = new Stopwatch();
                Stopwatch stopwatch3 = new Stopwatch();

                while (reader0.Read())
                {
                    stopwatch1.Start();

                    object obj1 = mapper1(Db.EntityDefFactory, reader0);

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

                        object? value = TypeConvert.DbValueToTypeValue(reader0[i], property, EngineType.MySQL);

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

            IEnumerable<PublisherEntity> publisherEntities = await Db.RetrieveAsync<PublisherEntity>(p => p.Type == PublisherType.Big && p.LastUser == "lastUsre", null).ConfigureAwait(false);

            Assert.IsTrue(publisherEntities.Any() && publisherEntities.All(p => p.Type == PublisherType.Big));
        }

        //TODO: 考虑这个
        //[TestMethod]
        //[DataRow(true, "server=127.0.0.1;port=3306;user=admin;password=_admin;Db=test_db;SslMode=None;")]
        //[DataRow(false, "server=127.0.0.1;port=3306;user=admin;password=_admin;Db=test_db;SslMode=None;")]
        //[DataRow(null, "server=127.0.0.1;port=3306;user=admin;password=_admin;Db=test_db;SslMode=None;")]
        public void TestMySQL_UseAffectedRow_Test(bool? UseAffectedRows, string connectString)
        {
            if (UseAffectedRows.HasValue)
            {
                connectString += $"UseAffectedRows={UseAffectedRows};";
            }

            using MySqlConnection mySqlConnection = new MySqlConnection(connectString);
            mySqlConnection.Open();

            string guid = SecurityUtil.CreateUniqueToken();

            string insertCommandText = $"insert into tb_publisher(`Name`, `LastTime`, `Guid`) values('SSFS', 100, '{guid}')";

            using MySqlCommand insertCommand = new MySqlCommand(insertCommandText, mySqlConnection);

            insertCommand.ExecuteScalar();

            string commandText = $"update `tb_publisher` set  `Name`='{new Random().NextDouble()}', `Version`=2 WHERE `Guid`='{guid}' ;";

            using MySqlCommand mySqlCommand1 = new MySqlCommand(commandText, mySqlConnection);

            int rt1 = mySqlCommand1.ExecuteNonQuery();

            using MySqlCommand rowCountCommand1 = new MySqlCommand("select row_count()", mySqlConnection);

            long? rowCount1 = (long?)rowCountCommand1.ExecuteScalar();

            using MySqlCommand mySqlCommand2 = new MySqlCommand(commandText, mySqlConnection);

            int rt2 = mySqlCommand1.ExecuteNonQuery();

            using MySqlCommand rowCountCommand2 = new MySqlCommand("select row_count()", mySqlConnection);

            long? rowCount2 = (long?)rowCountCommand2.ExecuteScalar();

            if (UseAffectedRows.HasValue && UseAffectedRows.Value) //真正改变的行数
            {
                Assert.AreNotEqual(rt1, rt2);
                Assert.AreNotEqual(rowCount1, rowCount2);
            }
            else //found_rows 找到的行数  by default in mysql
            {
                Assert.AreEqual(rt1, rt2);
                Assert.AreEqual(rowCount1, rowCount2);
            }
        }
    }
}