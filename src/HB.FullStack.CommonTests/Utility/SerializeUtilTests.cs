using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Tests
{

    public class TestEntity
    {
        public string? Name { get; set; }

        public IEnumerable<string> Values { get; init; } = new List<string>();
    }

    [TestClass()]
    public class SerializeUtilTests
    {
        [TestMethod()]
        public void TryFromJsonWithCollectionCheckTest()
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
    }
}