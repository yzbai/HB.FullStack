using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Mapper;
using HB.FullStack.DatabaseTests.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class EntityMapperTests : BaseTestClass
    {
        [TestMethod]
        [DataRow(EngineType.MySQL)]
        [DataRow(EngineType.SQLite)]
        public void EntityMapper_ToParameter_Test(EngineType engineType)
        {
            PublisherEntity publisherEntity = Mocker.MockOnePublisherEntity();

            publisherEntity.Version = 0;

            var emit_results = publisherEntity.EntityToParameters(Db.EntityDefFactory.GetDef<PublisherEntity>()!, engineType, Db.EntityDefFactory, 1);

            var reflect_results = publisherEntity.EntityToParametersUsingReflection(Db.EntityDefFactory.GetDef<PublisherEntity>()!, engineType, 1);

            AssertEqual(emit_results, reflect_results, engineType);

            //PublisherEntity2

            PublisherEntity2 publisherEntity2 = new PublisherEntity2();
            publisherEntity2.Version = 0;

            var emit_results2 = publisherEntity2.EntityToParameters(Db.EntityDefFactory.GetDef<PublisherEntity2>()!, engineType, Db.EntityDefFactory, 1);

            var reflect_results2 = publisherEntity2.EntityToParametersUsingReflection(Db.EntityDefFactory.GetDef<PublisherEntity2>()!, engineType, 1);

            AssertEqual(emit_results2, reflect_results2, engineType);

            //PublisherEntity3

            PublisherEntity3 publisherEntity3 = new PublisherEntity3();
            publisherEntity3.Version = 0;

            var emit_results3 = publisherEntity3.EntityToParameters(Db.EntityDefFactory.GetDef<PublisherEntity3>()!, engineType, Db.EntityDefFactory, 1);

            var reflect_results3 = publisherEntity3.EntityToParametersUsingReflection(Db.EntityDefFactory.GetDef<PublisherEntity3>()!, engineType, 1);

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
        public void EntityMapper_ToParameter_Performance_Test(EngineType engineType)
        {
            var entities = Mocker.GetPublishers(10000);

            Parallel.ForEach(entities, e => e.Version = 0);

            var def = Db.EntityDefFactory.GetDef<PublisherEntity>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Restart();
            foreach (var entity in entities)
            {
                _ = entity.EntityToParameters(def!, engineType, Db.EntityDefFactory);
            }
            stopwatch.Stop();

            Console.WriteLine($"Emit: {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            foreach (var entity in entities)
            {
                _ = entity.EntityToParametersUsingReflection(def!, engineType);
            }
            stopwatch.Stop();

            Console.WriteLine($"Reflection: {stopwatch.ElapsedMilliseconds}");
        }
    }
}