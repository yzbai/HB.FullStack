global using HB.FullStack.BaseTest;
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
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;
using HB.FullStack.BaseTest.Data.MySqls;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MySqlConnector;

namespace HB.FullStack.DatabaseTests.MySQL
{
    [TestClass]
    public class BasicTest_MySQL : BaseTestClass
    {
        [TestMethod]
        public async Task Test_1_Batch_Add_PublisherModelAsync()
        {
            IList<PublisherModel> publishers = Mocker.GetPublishers();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<PublisherModel>().ConfigureAwait(false);

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
        public async Task Test_2_Batch_Update_PublisherModelAsync()
        {
            await Test_1_Batch_Add_PublisherModelAsync();

            TransactionContext transContext = await Trans.BeginTransactionAsync<PublisherModel>().ConfigureAwait(false);

            try
            {
                IEnumerable<PublisherModel> lst = await Db.RetrieveAllAsync<PublisherModel>(transContext).ConfigureAwait(false);

                for (int i = 0; i < lst.Count(); i += 2)
                {
                    PublisherModel model = lst.ElementAt(i);
                    //model.Guid = Guid.NewGuid().ToString();
                    model.Type = PublisherType.Online;
                    model.Name = "Name_xxx";
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
            await Test_1_Batch_Add_PublisherModelAsync();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<PublisherModel>().ConfigureAwait(false);

            try
            {
                IList<PublisherModel> lst = (await Db.RetrieveAllAsync<PublisherModel>(transactionContext, 0, 10).ConfigureAwait(false)).ToList();

                if (lst.Count != 0)
                {
                    await Db.BatchDeleteAsync(lst, "lastUsre", transactionContext).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception("没有数据");
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
            TransactionContext tContext = await Trans.BeginTransactionAsync<PublisherModel>().ConfigureAwait(false);

            try
            {
                IList<PublisherModel> lst = new List<PublisherModel>();

                for (int i = 0; i < 10; ++i)
                {
                    PublisherModel model = Mocker.MockOnePublisherModel();

                    await Db.AddAsync(model, "lastUsre", tContext).ConfigureAwait(false);

                    lst.Add(model);
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
        public async Task Test_5_Update_PublisherModelAsync()
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<PublisherModel>().ConfigureAwait(false);

            try
            {
                IList<PublisherModel> testModels = (await Db.RetrieveAllAsync<PublisherModel>(tContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testModels.Count == 0)
                {
                    throw new Exception("No Model to update");
                }

                PublisherModel model = testModels[0];

                model.Books.Add("New Book2");
                //model.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Code = "Yuzhaobai" });

                await Db.UpdateAsync(model, "lastUsre", tContext).ConfigureAwait(false);

                PublisherModel? stored = await Db.ScalarAsync<PublisherModel>(model.Id, tContext).ConfigureAwait(false);

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
            await Test_1_Batch_Add_PublisherModelAsync();

            TransactionContext tContext = await Trans.BeginTransactionAsync<PublisherModel>().ConfigureAwait(false);

            try
            {
                IList<PublisherModel> testModels = (await Db.RetrieveAllAsync<PublisherModel>(tContext).ConfigureAwait(false)).ToList();

                foreach (var model in testModels)
                {
                    await Db.DeleteAsync(model, "lastUsre", tContext).ConfigureAwait(false);
                }

                long count = await Db.CountAsync<PublisherModel>(tContext).ConfigureAwait(false);

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
            PublisherModel item = Mocker.MockOnePublisherModel();

            await Db.AddAsync(item, "xx", null).ConfigureAwait(false);

            var fetched = await Db.ScalarAsync<PublisherModel>(item.Id, null).ConfigureAwait(false);

            Assert.AreEqual(item.Timestamp, fetched!.Timestamp);

            fetched.Name = "ssssss";

            await Db.UpdateAsync(fetched, "xxx", null).ConfigureAwait(false);

            fetched = await Db.ScalarAsync<PublisherModel>(item.Id, null).ConfigureAwait(false);

            //await Db.AddOrUpdateAsync(item, "ss", null);

            fetched = await Db.ScalarAsync<PublisherModel>(item.Id, null).ConfigureAwait(false);

            //Batch

            var items = Mocker.GetPublishers();

            TransactionContext trans = await Trans.BeginTransactionAsync<PublisherModel>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(items, "xx", trans).ConfigureAwait(false);

                var results = await Db.RetrieveAsync<PublisherModel>(item => SqlStatement.In(item.Id, true, items.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await Db.BatchUpdateAsync(items, "xx", trans).ConfigureAwait(false);

                var items2 = Mocker.GetPublishers();

                await Db.BatchAddAsync(items2, "xx", trans).ConfigureAwait(false);

                results = await Db.RetrieveAsync<PublisherModel>(item => SqlStatement.In(item.Id, true, items2.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

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
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<PublisherModel>().ConfigureAwait(false);
            //TransactionContext? transactionContext = null;

            try
            {
                PublisherModel item = Mocker.MockOnePublisherModel();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                IList<PublisherModel> testModels = (await Db.RetrieveAllAsync<PublisherModel>(transactionContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testModels.Count == 0)
                {
                    throw new Exception("No Model to update");
                }

                PublisherModel model = testModels[0];

                model.Books.Add("New Book2");
                //model.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Code = "Yuzhaobai" });

                await Db.UpdateAsync(model, "lastUsre", transactionContext).ConfigureAwait(false);

                PublisherModel? stored = await Db.ScalarAsync<PublisherModel>(model.Id, transactionContext).ConfigureAwait(false);

                item = Mocker.MockOnePublisherModel();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                var fetched = await Db.ScalarAsync<PublisherModel>(item.Id, transactionContext).ConfigureAwait(false);

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
            Globals.Logger.LogDebug($"Process,{Environment.ProcessId}");

            #region

            var publisher3 = new PublisherModel3();

            await Db.AddAsync(publisher3, "sss", null).ConfigureAwait(false);

            var stored3 = await Db.ScalarAsync<PublisherModel3>(publisher3.Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher3), SerializeUtil.ToJson(stored3));

            #endregion

            #region

            var publishers2 = Mocker.GetPublishers2();

            foreach (PublisherModel2 publisher in publishers2)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            PublisherModel2? publisher2 = await Db.ScalarAsync<PublisherModel2>(publishers2[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher2), SerializeUtil.ToJson(publishers2[0]));

            #endregion

            #region

            var publishers = Mocker.GetPublishers();

            foreach (PublisherModel publisher in publishers)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            PublisherModel? publisher1 = await Db.ScalarAsync<PublisherModel>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));
            #endregion
        }

        [TestMethod]
        public async Task Test_ModelMapperPerformanceAsync()
        {
            var books = Mocker.GetBooks(500);

            var trans = await Trans.BeginTransactionAsync<Book2Model>().ConfigureAwait(false);

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

            using MySqlConnection mySqlConnection = new MySqlConnection(DbSettingManager.GetConnectionString(DbSchema_Mysql, true).ToString());

            TypeHandlerHelper.AddTypeHandlerImpl(typeof(DateTimeOffset), new DateTimeOffsetTypeHandler(), false);
            TypeHandlerHelper.AddTypeHandlerImpl(typeof(Guid), new MySqlGuidTypeHandler(), false);

            //time = 0;
            int loop = 10;

            TimeSpan time0 = TimeSpan.Zero, time1 = TimeSpan.Zero, time2 = TimeSpan.Zero, time3 = TimeSpan.Zero;
            for (int cur = 0; cur < loop; ++cur)
            {
                await mySqlConnection.OpenAsync().ConfigureAwait(false);

                using MySqlCommand command0 = new MySqlCommand("select * from tb_Book2 limit 10000", mySqlConnection);

                var reader0 = await command0.ExecuteReaderAsync().ConfigureAwait(false);

                List<Book2Model> list1 = new List<Book2Model>();
                List<Book2Model> list2 = new List<Book2Model>();
                List<Book2Model> list3 = new List<Book2Model>();

                int len = reader0.FieldCount;
                DbModelPropertyDef[] propertyDefs = new DbModelPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                DbModelDef definition = Db.ModelDefFactory.GetDef<Book2Model>()!;

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetDbPropertyDef(reader0.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }

                Func<IDbModelDefFactory, IDataReader, object> mapper1 = DbModelConvert.CreateDataReaderRowToModelDelegate(definition, reader0, 0, definition.FieldCount, false);

                //Warning: �����Dapper��С��DateTimeOffset�Ĵ洢���ᶪʧoffset��Ȼ��ת����ʱ�򣬻���ϵ���ʱ���offset
                Func<IDataReader, object> mapper2 = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(Book2Model), reader0);

                Stopwatch stopwatch1 = new Stopwatch();
                Stopwatch stopwatch2 = new Stopwatch();
                Stopwatch stopwatch3 = new Stopwatch();

                while (reader0.Read())
                {
                    stopwatch1.Start();

                    object obj1 = mapper1(Db.ModelDefFactory, reader0);

                    list1.Add((Book2Model)obj1);
                    stopwatch1.Stop();

                    stopwatch2.Start();
                    object obj2 = mapper2(reader0);

                    list2.Add((Book2Model)obj2);
                    stopwatch2.Stop();

                    stopwatch3.Start();

                    Book2Model item = new Book2Model();

                    for (int i = 0; i < len; ++i)
                    {
                        DbModelPropertyDef property = propertyDefs[i];

                        object? value = DbPropertyConvert.DbFieldValueToPropertyValue(reader0[i], property, DbEngineType.MySQL);

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
            IList<PublisherModel> publishers = Mocker.GetPublishers();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<PublisherModel>().ConfigureAwait(false);

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

            IEnumerable<PublisherModel> publisherModels = await Db.RetrieveAsync<PublisherModel>(p => p.Type == PublisherType.Big && p.LastUser == "lastUsre", null).ConfigureAwait(false);

            Assert.IsTrue(publisherModels.Any() && publisherModels.All(p => p.Type == PublisherType.Big));
        }

        //NOTICE: 在重复update时，即值不发生改变。默认useAffectedRows=false，即update返回matched的数量。 而useAffectedRows=true，则返回真正发生过改变的数量。
        //应该保持useAffectedRows=false
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