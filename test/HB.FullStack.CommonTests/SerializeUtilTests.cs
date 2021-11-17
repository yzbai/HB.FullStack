using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HB.FullStack.CommonTests
{
    [TestClass]
    public class SerializeUtilTests
    {
        [TestMethod]
        public void TryFromJsonWithCollectionCheckTest()
        {
            string? nullStr = null;
            string emptyStr = string.Empty;
            string emptyCollectionStr = "[]";

            Assert.IsTrue(SerializeUtil.TryFromJsonWithCollectionCheck(emptyCollectionStr, out TestEntity? entity));
            Assert.IsNull(entity);

            SerializeUtil.TryFromJsonWithCollectionCheck(emptyCollectionStr, out IEnumerable<TestEntity>? entities);
            Assert.IsTrue(entities!=null && !entities.Any());


            Assert.IsTrue(SerializeUtil.TryFromJsonWithCollectionCheck(emptyStr, out TestEntity? entity1));
            Assert.IsNull(entity1);

            Assert.IsTrue(SerializeUtil.TryFromJsonWithCollectionCheck(emptyStr, out IEnumerable<TestEntity>? entities1));
            Assert.IsTrue(entities1 != null && !entities1.Any());

            Assert.IsTrue(SerializeUtil.TryFromJsonWithCollectionCheck(nullStr, out TestEntity? entity2));
            Assert.IsNull(entity2);

            Assert.IsTrue(SerializeUtil.TryFromJsonWithCollectionCheck(nullStr, out IEnumerable<TestEntity>? entities2));
            Assert.IsTrue(entities2 != null && !entities2.Any());
        }

        [TestMethod()]
        public void TryFromJsonWithCollectionCheckTest1()
        {
            TestEntity testEntity1 = new TestEntity { Name = "123", Values = new List<string> { "1", "2" } };
            TestEntity testEntity2 = new TestEntity { Name = "123", Values = new List<string> { "1", "2" } };
            TestEntity testEntity3 = new TestEntity { Name = "123", Values = new List<string> { "1", "2" } };

            IEnumerable<TestEntity> oneList = new List<TestEntity> { testEntity1 };
            IEnumerable<TestEntity> moreLst = new List<TestEntity> { testEntity1, testEntity2, testEntity3 };

            string itemJson = SerializeUtil.ToJson(testEntity1);
            string oneCollectionJson = SerializeUtil.ToJson(oneList);
            string moreCollectionJson = SerializeUtil.ToJson(moreLst);

            //item - item
            if (SerializeUtil.TryFromJsonWithCollectionCheck(itemJson, out TestEntity? entity))
            {
                Assert.AreEqual(SerializeUtil.ToJson(entity), SerializeUtil.ToJson(testEntity1));
            }
            else
            {
                Assert.Fail("item - item failed");
            }

            //item - collection
            if (SerializeUtil.TryFromJsonWithCollectionCheck(itemJson, out IEnumerable<TestEntity>? entities))
            {
                Assert.AreEqual(SerializeUtil.ToJson(entities), SerializeUtil.ToJson(oneList));
            }
            else
            {
                Assert.Fail("item - collection failed");
            }

            //collection - collection
            if (SerializeUtil.TryFromJsonWithCollectionCheck(moreCollectionJson, out IEnumerable<TestEntity>? entities2))
            {
                Assert.AreEqual(SerializeUtil.ToJson(entities2), SerializeUtil.ToJson(moreLst));
            }
            else
            {
                Assert.Fail("collection - collection failed");
            }

            //oneCollection - item
            if (SerializeUtil.TryFromJsonWithCollectionCheck(oneCollectionJson, out TestEntity? entity1))
            {
                Assert.AreEqual(SerializeUtil.ToJson(entity1), SerializeUtil.ToJson(testEntity1));
            }
            else
            {
                Assert.Fail("oneCollection - item failed");
            }

            //moreCollection - item
            if (SerializeUtil.TryFromJsonWithCollectionCheck(moreCollectionJson, out TestEntity? entity2))
            {
                Assert.Fail("moreCollection - item failed");
            }
        }

        [TestMethod]
        public void ToJsonTest()
        {
            var student = StudentMocker.MockOneStudent();

            string json = SerializeUtil.ToJson(student);
            string newtonJson = Newtonsoft.Json.JsonConvert.SerializeObject(student, new Newtonsoft.Json.Converters.StringEnumConverter());

            Assert.AreEqual(json, newtonJson);
        }

        [TestMethod]
        public void ToJsonTest_ChineseSymbol()
        {
            object jsonObject = new { chinese_symbol = @"~·@#￥%……&*（）—-+=｛｝【】；：“”‘’《》，。？、" };
            string json = SerializeUtil.ToJson(jsonObject);
            string newtonJson = Newtonsoft.Json.JsonConvert.SerializeObject(value: jsonObject);

            Assert.AreEqual(expected: json, actual: newtonJson);
        }

        [TestMethod]
        public void FromJsonTest_Number()
        {
            string json = "{\"Number\": \"123\", \"Price\": \"12.123456789\"}";


            NumberTestCls? obj = SerializeUtil.FromJson<NumberTestCls>(json);

            NumberTestCls? newtonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<NumberTestCls>(json);

            Assert.IsTrue(obj!.Number == newtonObj?.Number && obj.Price == newtonObj.Price);
        }

        [TestMethod]
        public void Collection_Test()
        {
            IList<Student> students = new List<Student> { StudentMocker.MockOneStudent(), StudentMocker.MockOneStudent() };

            string json = SerializeUtil.ToJson(students);
            string newtonJson = Newtonsoft.Json.JsonConvert.SerializeObject(students, new Newtonsoft.Json.Converters.StringEnumConverter());

            Console.WriteLine(json);
            Console.WriteLine(newtonJson);

            Assert.AreEqual(json, newtonJson);
            _ = SerializeUtil.FromJson<IList<Student>>(json);


        }
    }
}