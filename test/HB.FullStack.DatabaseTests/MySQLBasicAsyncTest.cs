using ClassLibrary1;

using HB.FullStack.Database;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.SQL;
using HB.FullStack.DatabaseTests.Data;

using Microsoft.Extensions.DependencyInjection;

using MySqlConnector;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace HB.FullStack.DatabaseTests
{
    //[TestCaseOrderer("HB.FullStack.Database.Test.TestCaseOrdererByTestName", "HB.FullStack.Database.Test")]
    public class MySQLBasicAsyncTest : IClassFixture<ServiceFixture>
    {
        private readonly IDatabase _mysql;
        private readonly ITransaction _mysqlTransaction;
        private readonly ITestOutputHelper _output;
        private readonly IDatabaseEntityDefFactory _defFactory;
        private readonly IDatabaseEntityMapper _mapper;
        private readonly string _mysqlConnectionString;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="testOutputHelper"></param>
        /// <param name="serviceFixture"></param>
        /// <exception cref="DatabaseException">Ignore.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        public MySQLBasicAsyncTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            _output = testOutputHelper;

            _mysql = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();
            _mysqlTransaction = serviceFixture.ServiceProvider.GetRequiredService<ITransaction>();

            _mysql.InitializeAsync().Wait();

            _defFactory = serviceFixture.ServiceProvider.GetRequiredService<IDatabaseEntityDefFactory>();

            _mapper = serviceFixture.ServiceProvider.GetRequiredService<IDatabaseEntityMapper>();

            _mysqlConnectionString = serviceFixture.Configuration["MySQL:Connections:0:ConnectionString"];
        }

        /// <summary>
        /// Test_1_Batch_Add_PublisherEntityAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
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
        [Fact]


        public async Task Test_2_Batch_Update_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
            TransactionContext transContext = await transaction.BeginTransactionAsync<PublisherEntity>();

            try
            {
                IEnumerable<PublisherEntity> lst = await database.RetrieveAllAsync<PublisherEntity>(transContext);

                for (int i = 0; i < lst.Count(); i += 2)
                {
                    PublisherEntity entity = lst.ElementAt(i);
                    //entity.Guid = Guid.NewGuid().ToString();
                    entity.Type = PublisherType.Online;
                    entity.Name = "ÖÐsfasfafÎÄÃû×Ö";
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
        [Fact]


        public async Task Test_3_Batch_Delete_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
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
        [Fact]


        public async Task Test_4_Add_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
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
        [Fact]


        public async Task Test_5_Update_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
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
        [Fact]


        public async Task Test_6_Delete_PublisherEntityAsync()
        {
            IDatabase database = _mysql;
            ITransaction transaction = _mysqlTransaction;
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

        [Fact]


        public async Task Test_8_LastTimeTestAsync()
        {
            IDatabase database = _mysql;

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

            ITransaction transaction = _mysqlTransaction;

            TransactionContext trans = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

            try
            {
                await database.BatchAddAsync<PublisherEntity>(items, "xx", trans).ConfigureAwait(false);


                var results = await database.RetrieveAsync<PublisherEntity>(item => SQLUtil.In(item.Guid, true, items.Select(item => item.Guid).ToArray()), trans).ConfigureAwait(false);

                await database.BatchUpdateAsync<PublisherEntity>(items, "xx", trans);

                var items2 = Mocker.GetPublishers();

                //await database.BatchAddOrUpdateAsync<PublisherEntity>(items2, "xx", trans);

                results = await database.RetrieveAsync<PublisherEntity>(item => SQLUtil.In(item.Guid, true, items2.Select(item => item.Guid).ToArray()), trans);

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

        [Fact]


        public async Task Test_9_UpdateLastTimeTestAsync()
        {
            IDatabase database = _mysql;

            ITransaction transaction = _mysqlTransaction;

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
            IDatabase database = _mysql;

            var publishers = Mocker.GetPublishers();


            foreach (PublisherEntity publisher in publishers)
            {
                await database.AddAsync(publisher, "", null).ConfigureAwait(false);
            }


            PublisherEntity? publisher1 = await database.ScalarAsync<PublisherEntity>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.Equal(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));

        }

        [Theory]
        [InlineData(1)]
        public async Task Test_EntityMapperPerformanceAsync(int index)
        {
            index++;
            //IDatabase database = _mysql;

            //var books = Mocker.GetBooks(50000);

            //var trans = await _mysqlTransaction.BeginTransactionAsync<BookEntity>().ConfigureAwait(false);

            //try
            //{
            //    await database.BatchAddAsync(books, "x", trans).ConfigureAwait(false);
            //    await _mysqlTransaction.CommitAsync(trans).ConfigureAwait(false);
            //}
            //catch
            //{
            //    await _mysqlTransaction.RollbackAsync(trans).ConfigureAwait(false);
            //}


            Stopwatch stopwatch = new Stopwatch();

            MySqlConnection mySqlConnection = new MySqlConnection(_mysqlConnectionString);

            long time;

            int loop = 10;

            //Reflection

            time = 0;
            for (int cur = 0; cur < loop; ++cur)
            {
                await mySqlConnection.OpenAsync();


                MySqlCommand command = new MySqlCommand("select * from tb_book", mySqlConnection);

                var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                List<BookEntity> list = new List<BookEntity>();
                stopwatch.Start();
                while (await reader.ReadAsync())
                {
                    BookEntity entity = new BookEntity();
                    _mapper.ToObject<BookEntity>(reader, entity);

                    list.Add(entity);
                }

                stopwatch.Stop();

                time += stopwatch.ElapsedMilliseconds;


                await reader.DisposeAsync().ConfigureAwait(false);
                command.Dispose();

                await mySqlConnection.CloseAsync();
            }
            _output.WriteLine((time / 10.0).ToString());

            //Emit


            time = 0;
            for (int cur = 0; cur < loop; ++cur)
            {

                await mySqlConnection.OpenAsync().ConfigureAwait(false);

                MySqlCommand command2 = new MySqlCommand("select * from tb_book", mySqlConnection);

                var reader2 = await command2.ExecuteReaderAsync().ConfigureAwait(false);

                var def = _defFactory.GetDef<BookEntity>();
                var fun = EntityMapperHelper.CreateEntityMapperDelegate(def, reader2);
                List<BookEntity> list2 = new List<BookEntity>();
                stopwatch.Restart();
                while (await reader2.ReadAsync())
                {
                    object obj = fun(reader2, def);

                    list2.Add((BookEntity)obj);
                }

                stopwatch.Stop();

                time += stopwatch.ElapsedMilliseconds;

                await reader2.DisposeAsync().ConfigureAwait(false);
                command2.Dispose();

                await mySqlConnection.CloseAsync();
            }
            _output.WriteLine((time / 10.0).ToString());


            //Dapper

            time = 0;
            for (int cur = 0; cur < loop; ++cur)
            {
                await mySqlConnection.OpenAsync().ConfigureAwait(false);

                MySqlCommand command3 = new MySqlCommand("select * from tb_book", mySqlConnection);

                var reader3 = await command3.ExecuteReaderAsync().ConfigureAwait(false);

                Func<IDataReader, object> fun3 = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(BookEntity), reader3);

                List<BookEntity> list3 = new List<BookEntity>();
                stopwatch.Restart();
                while (await reader3.ReadAsync())
                {
                    object obj = fun3(reader3);

                    list3.Add((BookEntity)obj);
                }

                stopwatch.Stop();

                time += stopwatch.ElapsedMilliseconds;


                command3.Dispose();

                await mySqlConnection.CloseAsync();
            }

            _output.WriteLine((time / 10.0).ToString());


            //coding

            time = 0;
            for (int cur = 0; cur < loop; ++cur)
            {

                await mySqlConnection.OpenAsync();


                MySqlCommand command0 = new MySqlCommand("select * from tb_book", mySqlConnection);

                var reader0 = await command0.ExecuteReaderAsync().ConfigureAwait(false);

                List<BookEntity> list0 = new List<BookEntity>();
                int len = reader0.FieldCount;
                DatabaseEntityPropertyDef[] propertyDefs = new DatabaseEntityPropertyDef[len];

                DatabaseEntityDef definition = _defFactory.GetDef<BookEntity>();

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetProperty(reader0.GetName(i))!;
                }

                stopwatch.Restart();

                while (await reader0.ReadAsync())
                {
                    BookEntity item = new BookEntity();

                    for (int i = 0; i < len; ++i)
                    {
                        DatabaseEntityPropertyDef property = propertyDefs[i];

                        object? value = property.TypeConverter == null ?
                            ValueConverterUtil.DbValueToTypeValue(reader0[i], property.PropertyInfo.PropertyType) :
                            property.TypeConverter.DbValueToTypeValue(reader0[i]);

                        if (value != null)
                        {
                            property.PropertyInfo.SetValue(item, value);
                        }
                    }

                    list0.Add(item);
                }

                stopwatch.Stop();

                time += stopwatch.ElapsedMilliseconds;

                //

                await reader0.DisposeAsync().ConfigureAwait(false);
                command0.Dispose();

                await mySqlConnection.CloseAsync();

            }

            _output.WriteLine((time / 10.0).ToString());

        }

        //[Fact]


        //public async Task Test_10_AddOrUpdate_VersionTestAsync()
        //{
        //    IDatabase database = _mysql;

        //    ITransaction transaction = _mysqlTransaction;

        //    TransactionContext transactionContext = await transaction.BeginTransactionAsync<PublisherEntity>().ConfigureAwait(false);

        //    try
        //    {

        //        PublisherEntity item = Mocker.MockOne();

        //        Assert.True(item.Version == -1);

        //        //await database.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);

        //        Assert.True(item.Version == 0);

        //        //await database.AddOrUpdateAsync(item, "sfas", transactionContext).ConfigureAwait(false);

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
