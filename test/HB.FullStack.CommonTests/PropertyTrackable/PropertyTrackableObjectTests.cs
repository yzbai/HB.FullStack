using System;
using System.Collections.Immutable;
using System.Reflection;

using HB.FullStack.Common.PropertyTrackable;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CommonTests.PropertyTrackable
{
    [TestClass()]
    public class PropertyTrackableObjectTests
    {
        [TestMethod()]
        public void GetChangedPropertiesTest1()
        {
            string rightJson = "[{\"PropertyName\":\"Name\",\"OldValue\":null,\"NewValue\":\"TestName\"},{\"PropertyName\":\"Age\",\"OldValue\":0,\"NewValue\":1000},{\"PropertyName\":\"TestRecord\",\"OldValue\":{\"InnerName\":null},\"NewValue\":{\"InnerName\":\"InnerTestRecord\"}},{\"PropertyName\":\"ImmutableList\",\"OldValue\":[\"x\"],\"NewValue\":[\"x\",\"y\"]},{\"PropertyName\":\"ImmutableArray\",\"OldValue\":[\"y\"],\"NewValue\":[\"y\",\"ydfd\"]},{\"PropertyName\":\"ObservableInner\",\"OldValue\":null,\"NewValue\":{\"InnerName\":\"sdfs\"}},{\"PropertyName\":\"ObservableInner\",\"OldValue\":{\"InnerName\":\"sdfs\"},\"NewValue\":{\"InnerName\":\"sfasfs\"}}]";
            string rightJson2 = "[{\"PropertyName\":\"Name\",\"OldValue\":null,\"NewValue\":\"TestName\"},{\"PropertyName\":\"Age\",\"OldValue\":0,\"NewValue\":1000},{\"PropertyName\":\"TestRecord\",\"OldValue\":{\"InnerName\":null},\"NewValue\":{\"InnerName\":\"InnerTestRecord\"}},{\"PropertyName\":\"ImmutableList\",\"OldValue\":[\"x\"],\"NewValue\":[\"x\",\"y\"]},{\"PropertyName\":\"ImmutableArray\",\"OldValue\":[\"y\"],\"NewValue\":[\"y\",\"ydfd\"]},{\"PropertyName\":\"ObservableInner\",\"OldValue\":null,\"NewValue\":{\"InnerName\":\"sfasfs\"}}]";

            TestObject testObject = new TestObject();
            ActionOnTestObject1(testObject);

            var changes = testObject.GetPropertyChangePack();
            var changes2 = testObject.GetPropertyChangePack();

            string json = SerializeUtil.ToJson(changes);
            string json2 = SerializeUtil.ToJson(changes2);

            Assert.AreEqual(json, rightJson);
            Assert.AreEqual(json2, rightJson2);
        }

        private static void ActionOnTestObject1(TestObject testObject)
        {
            testObject.TestRecord = new TestRecord(null);
            testObject.ImmutableList = ImmutableList.Create("x");
            testObject.ImmutableArray = ImmutableArray.Create("y");
            testObject.StartTrack();

            //Modify

            testObject.Name = "TestName";
            testObject.Age = 1000;
            testObject.TestRecord = testObject.TestRecord with { InnerName = "InnerTestRecord" };
            testObject.ImmutableList = testObject.ImmutableList.Add("y");
            testObject.ImmutableArray = testObject.ImmutableArray.Value.Add("ydfd");
            testObject.ObservableInner = new ObservableInner { InnerName = "sdfs" };
            testObject.ObservableInner.InnerName = "sfasfs";
        }

        [TestMethod]
        public void TestAttributeForward()
        {
            var attr = typeof(TestObject).GetProperty(nameof(TestObject.Id))?.GetCustomAttribute<AddtionalPropertyAttribute>(true);

            Assert.IsNotNull(attr);
        }

        [TestMethod]
        public void TestCppAddtionalProperties()
        {
            TestObject testObject = new TestObject();
            ActionOnTestObject1(testObject);

            //TODO:continue;
        }
    }
}