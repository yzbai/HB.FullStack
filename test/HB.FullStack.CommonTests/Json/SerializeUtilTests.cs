using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HB.FullStack.CommonTests.Json
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

            Assert.IsTrue(SerializeUtil.TryFromJsonWithCollectionCheck(emptyCollectionStr, out TestModel? model));
            Assert.IsNull(model);

            SerializeUtil.TryFromJsonWithCollectionCheck(emptyCollectionStr, out IEnumerable<TestModel>? models);
            Assert.IsTrue(models != null && !models.Any());

            Assert.IsTrue(SerializeUtil.TryFromJsonWithCollectionCheck(emptyStr, out TestModel? model1));
            Assert.IsNull(model1);

            Assert.IsTrue(SerializeUtil.TryFromJsonWithCollectionCheck(emptyStr, out IEnumerable<TestModel>? models1));
            Assert.IsTrue(models1 != null && !models1.Any());

            Assert.IsTrue(SerializeUtil.TryFromJsonWithCollectionCheck(nullStr, out TestModel? model2));
            Assert.IsNull(model2);

            Assert.IsTrue(SerializeUtil.TryFromJsonWithCollectionCheck(nullStr, out IEnumerable<TestModel>? models2));
            Assert.IsTrue(models2 != null && !models2.Any());
        }

        [TestMethod()]
        public void TryFromJsonWithCollectionCheckTest1()
        {
            TestModel testModel1 = new TestModel { Name = "123", Values = new List<string> { "1", "2" } };
            TestModel testModel2 = new TestModel { Name = "123", Values = new List<string> { "1", "2" } };
            TestModel testModel3 = new TestModel { Name = "123", Values = new List<string> { "1", "2" } };

            IEnumerable<TestModel> oneList = new List<TestModel> { testModel1 };
            IEnumerable<TestModel> moreLst = new List<TestModel> { testModel1, testModel2, testModel3 };

            string itemJson = SerializeUtil.ToJson(testModel1);
            string oneCollectionJson = SerializeUtil.ToJson(oneList);
            string moreCollectionJson = SerializeUtil.ToJson(moreLst);

            //item - item
            if (SerializeUtil.TryFromJsonWithCollectionCheck(itemJson, out TestModel? model))
            {
                Assert.AreEqual(SerializeUtil.ToJson(model), SerializeUtil.ToJson(testModel1));
            }
            else
            {
                Assert.Fail("item - item failed");
            }

            //item - collection
            if (SerializeUtil.TryFromJsonWithCollectionCheck(itemJson, out IEnumerable<TestModel>? models))
            {
                Assert.AreEqual(SerializeUtil.ToJson(models), SerializeUtil.ToJson(oneList));
            }
            else
            {
                Assert.Fail("item - collection failed");
            }

            //collection - collection
            if (SerializeUtil.TryFromJsonWithCollectionCheck(moreCollectionJson, out IEnumerable<TestModel>? models2))
            {
                Assert.AreEqual(SerializeUtil.ToJson(models2), SerializeUtil.ToJson(moreLst));
            }
            else
            {
                Assert.Fail("collection - collection failed");
            }

            //oneCollection - item
            if (SerializeUtil.TryFromJsonWithCollectionCheck(oneCollectionJson, out TestModel? model1))
            {
                Assert.AreEqual(SerializeUtil.ToJson(model1), SerializeUtil.ToJson(testModel1));
            }
            else
            {
                Assert.Fail("oneCollection - item failed");
            }

            //moreCollection - item
            if (SerializeUtil.TryFromJsonWithCollectionCheck(moreCollectionJson, out TestModel? _))
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