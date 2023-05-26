using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.BaseTest.DapperMapper;
using HB.FullStack.Database.Convert;

using HB.FullStack.Database.DbModels;

using HB.FullStack.Database.Engine;

using HB.FullStack.Database;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class ConvertTests : DatabaseTestClass
    {
        private async Task Test_Mapper_ToModel_Performance_Core<T>() where T : IDbModel
        {
            var modelDef = Db.ModelDefFactory.GetDef<T>()!;
            var books = await AddAndRetrieve<T>(50);

            //SetUp Dapper
            TypeHandlerHelper.AddTypeHandlerImpl(typeof(DateTimeOffset), new DateTimeOffsetTypeHandler(), false);
            TypeHandlerHelper.AddTypeHandlerImpl(typeof(Guid), new MySqlGuidTypeHandler(), false);

            //time = 0;
            int loop = 10;

            TimeSpan time0 = TimeSpan.Zero, time1 = TimeSpan.Zero, time2 = TimeSpan.Zero, time3 = TimeSpan.Zero;
            for (int cur = 0; cur < loop; ++cur)
            {
                using var reader = await modelDef.Engine.ExecuteCommandReaderAsync(
                    modelDef.MasterConnectionString,
                    new DbEngineCommand("select * from {modelDef.DbTableReservedName} limit 5000")).ConfigureAwait(false);

                List<T> list1 = new List<T>();
                List<T> list2 = new List<T>();
                List<T> list3 = new List<T>();

                int len = reader.FieldCount;
                DbModelPropertyDef[] propertyDefs = new DbModelPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = modelDef.GetDbPropertyDef(reader.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }

                Func<IDbModelDefFactory, IDataReader, object> fullStack_mapper = DbModelConvert.CreateDataReaderRowToModelDelegate(
                    modelDef, reader, 0, modelDef.FieldCount, false);

                Func<IDataReader, object> dapper_mapper = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(T), reader);

                Func<IDataReader, object> reflection_mapper = (r) =>
                {
                    T item = Activator.CreateInstance<T>();

                    for (int i = 0; i < len; ++i)
                    {
                        DbModelPropertyDef property = propertyDefs[i];

                        object? value = DbPropertyConvert.DbFieldValueToPropertyValue(r[i], property, DbEngineType.MySQL);

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
                    list1.Add((T)obj1);
                    stopwatch1.Stop();

                    stopwatch2.Start();
                    object obj2 = dapper_mapper(reader);
                    list2.Add((T)obj2);
                    stopwatch2.Stop();

                    stopwatch3.Start();
                    object obj3 = reflection_mapper(reader);
                    list3.Add((T)obj3);
                    stopwatch3.Stop();
                }

                time1 += stopwatch1.Elapsed;
                time2 += stopwatch2.Elapsed;
                time3 += stopwatch3.Elapsed;
            }

            Console.WriteLine("FullStack_Emit : " + (time1.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("Dapper : " + (time2.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("FullStack_Reflection : " + (time3.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public async Task Test_Mapper_ToModel_Performance()
        {
            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Long_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private void Test_Mapper_ToParameter_Core<T>() where T : IDbModel
        {
            var modelDef = Db.ModelDefFactory.GetDef<T>()!;
            T model = Mocker.MockOne<T>();

            var emit_results = model.ToDbParameters(modelDef, Db.ModelDefFactory, null, 0);

            var reflect_results = model.ToDbParametersUsingReflection(modelDef, null, 0);

            AssertEqual(emit_results, reflect_results, modelDef.EngineType);
        }

        [TestMethod]
        public void Test_Mapper_ToParameter()
        {
            Test_Mapper_ToParameter_Core<MySql_Timestamp_Guid_BookModel>();
            Test_Mapper_ToParameter_Core<MySql_Timestamp_Long_BookModel>();
            Test_Mapper_ToParameter_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Guid_BookModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Long_BookModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Guid_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Long_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Guid_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Long_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            Test_Mapper_ToParameter_Core<MySql_Timestamp_Guid_PublisherModel>();
            Test_Mapper_ToParameter_Core<MySql_Timestamp_Long_PublisherModel>();
            Test_Mapper_ToParameter_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Guid_PublisherModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Long_PublisherModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Long_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Guid_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Long_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private void Test_Mapper_ToParameter_Performance_Core<T>() where T : IDbModel
        {
            var modelDef = Db.ModelDefFactory.GetDef<T>()!;
            var models = Mocker.Mock<T>(1000000);

            Stopwatch stopwatch = new Stopwatch();

            int i = 0;
            stopwatch.Restart();
            foreach (var model in models)
            {
                _ = model.ToDbParameters(modelDef, Db.ModelDefFactory, null, i++);
            }

            stopwatch.Stop();

            Console.WriteLine($"Emit: {stopwatch.ElapsedMilliseconds}");

            i = 0;
            stopwatch.Restart();
            foreach (var model in models)
            {
                _ = model.ToDbParametersUsingReflection(modelDef, null, i++);
            }
            stopwatch.Stop();

            Console.WriteLine($"Reflection: {stopwatch.ElapsedMilliseconds}");
        }

        [TestMethod]
        public void Test_Mapper_ToParameter_Performance()
        {
            Test_Mapper_ToParameter_Performance_Core<MySql_Timestamp_Guid_BookModel>();
            Test_Mapper_ToParameter_Performance_Core<MySql_Timestamp_Guid_PublisherModel>();

            Test_Mapper_ToParameter_Performance_Core<Sqlite_Timestamp_Guid_BookModel>();
            Test_Mapper_ToParameter_Performance_Core<Sqlite_Timestamp_Guid_PublisherModel>();
        }

        private static void AssertEqual(IEnumerable<KeyValuePair<string, object>> emit_results, IEnumerable<KeyValuePair<string, object>> results, DbEngineType engineType)
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
    }
}
