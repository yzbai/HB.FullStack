using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Mapper;
using HB.FullStack.DatabaseTests.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class ModelMapperTests : BaseTestClass
    {
        [TestMethod]
        [DataRow(EngineType.MySQL)]
        [DataRow(EngineType.SQLite)]
        public void ModelMapper_ToParameter_Test(EngineType engineType)
        {
            PublisherModel publisherModel = Mocker.MockOnePublisherModel();



            var emit_results = publisherModel.ModelToParameters(Db.ModelDefFactory.GetDef<PublisherModel>()!, engineType, Db.ModelDefFactory, 1);

            var reflect_results = publisherModel.ModelToParametersUsingReflection(Db.ModelDefFactory.GetDef<PublisherModel>()!, engineType, 1);

            AssertEqual(emit_results, reflect_results, engineType);

            //PublisherModel2

            PublisherModel2 publisherModel2 = new PublisherModel2();


            var emit_results2 = publisherModel2.ModelToParameters(Db.ModelDefFactory.GetDef<PublisherModel2>()!, engineType, Db.ModelDefFactory, 1);

            var reflect_results2 = publisherModel2.ModelToParametersUsingReflection(Db.ModelDefFactory.GetDef<PublisherModel2>()!, engineType, 1);

            AssertEqual(emit_results2, reflect_results2, engineType);

            //PublisherModel3

            PublisherModel3 publisherModel3 = new PublisherModel3();


            var emit_results3 = publisherModel3.ModelToParameters(Db.ModelDefFactory.GetDef<PublisherModel3>()!, engineType, Db.ModelDefFactory, 1);

            var reflect_results3 = publisherModel3.ModelToParametersUsingReflection(Db.ModelDefFactory.GetDef<PublisherModel3>()!, engineType, 1);

            AssertEqual(emit_results3, reflect_results3, engineType);
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

        [TestMethod]
        [DataRow(EngineType.MySQL)]
        [DataRow(EngineType.SQLite)]
        public void ModelMapper_ToParameter_Performance_Test(EngineType engineType)
        {
            var models = Mocker.GetPublishers(10000);



            var def = Db.ModelDefFactory.GetDef<PublisherModel>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Restart();
            foreach (var model in models)
            {
                _ = model.ModelToParameters(def!, engineType, Db.ModelDefFactory);
            }
            stopwatch.Stop();

            Console.WriteLine($"Emit: {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            foreach (var model in models)
            {
                _ = model.ModelToParametersUsingReflection(def!, engineType);
            }
            stopwatch.Stop();

            Console.WriteLine($"Reflection: {stopwatch.ElapsedMilliseconds}");
        }
    }
}