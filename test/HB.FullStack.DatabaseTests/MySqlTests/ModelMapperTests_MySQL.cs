using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using HB.FullStack.Database.Convert;
using HB.FullStack.Database.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HB.FullStack.BaseTest.Models;

namespace HB.FullStack.DatabaseTests.MySQL
{
    [TestClass]
    public class ModelMapperTests_MySQL : BaseTestClass
    {
        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public void ModelMapper_ToParameter_Test(DbEngineType engineType)
        {
            PublisherModel publisherModel = Mocker.MockOnePublisherModel();

            var emit_results = publisherModel.ToDbParameters(Db.ModelDefFactory.GetDef<PublisherModel>()!, Db.ModelDefFactory, 1);

            var reflect_results = publisherModel.ToDbParametersUsingReflection(Db.ModelDefFactory.GetDef<PublisherModel>()!, 1);

            AssertEqual(emit_results, reflect_results, engineType);

            //PublisherModel2

            PublisherModel2 publisherModel2 = new PublisherModel2();

            var emit_results2 = publisherModel2.ToDbParameters(Db.ModelDefFactory.GetDef<PublisherModel2>()!, Db.ModelDefFactory, 1);

            var reflect_results2 = publisherModel2.ToDbParametersUsingReflection(Db.ModelDefFactory.GetDef<PublisherModel2>()!, 1);

            AssertEqual(emit_results2, reflect_results2, engineType);

            //PublisherModel3

            PublisherModel3 publisherModel3 = new PublisherModel3();

            var emit_results3 = publisherModel3.ToDbParameters(Db.ModelDefFactory.GetDef<PublisherModel3>()!, Db.ModelDefFactory, 1);

            var reflect_results3 = publisherModel3.ToDbParametersUsingReflection(Db.ModelDefFactory.GetDef<PublisherModel3>()!, 1);

            AssertEqual(emit_results3, reflect_results3, engineType);
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

        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public void ModelMapper_ToParameter_Performance_Test(DbEngineType engineType)
        {
            var models = Mocker.GetPublishers(10000);

            var def = Db.ModelDefFactory.GetDef<PublisherModel>();

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
    }
}