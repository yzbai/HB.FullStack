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
            string rightJson = "{\"PropertyChanges\":{\"Name\":{\"PropertyName\":\"Name\",\"OldValue\":null,\"NewValue\":\"TestName\"},\"Age\":{\"PropertyName\":\"Age\",\"OldValue\":0,\"NewValue\":1000},\"TestRecord\":{\"PropertyName\":\"TestRecord\",\"OldValue\":{\"InnerName\":null},\"NewValue\":{\"InnerName\":\"InnerTestRecord\"}},\"ImmutableList\":{\"PropertyName\":\"ImmutableList\",\"OldValue\":[\"x\"],\"NewValue\":[\"x\",\"y\"]},\"ImmutableArray\":{\"PropertyName\":\"ImmutableArray\",\"OldValue\":[\"y\"],\"NewValue\":[\"y\",\"ydfd\"]},\"ObservableInner\":{\"PropertyName\":\"ObservableInner\",\"OldValue\":null,\"NewValue\":{\"InnerName\":\"sfasfs\"}}},\"AddtionalProperties\":{\"Id\":\"This is a Id\"}}";
            string rightJson2 = "{\"PropertyChanges\":{\"Name\":{\"PropertyName\":\"Name\",\"OldValue\":null,\"NewValue\":\"TestName\"},\"Age\":{\"PropertyName\":\"Age\",\"OldValue\":0,\"NewValue\":1000},\"TestRecord\":{\"PropertyName\":\"TestRecord\",\"OldValue\":{\"InnerName\":null},\"NewValue\":{\"InnerName\":\"InnerTestRecord\"}},\"ImmutableList\":{\"PropertyName\":\"ImmutableList\",\"OldValue\":[\"x\"],\"NewValue\":[\"x\",\"y\"]},\"ImmutableArray\":{\"PropertyName\":\"ImmutableArray\",\"OldValue\":[\"y\"],\"NewValue\":[\"y\",\"ydfd\"]},\"ObservableInner\":{\"PropertyName\":\"ObservableInner\",\"OldValue\":null,\"NewValue\":{\"InnerName\":\"sfasfs\"}}},\"AddtionalProperties\":{\"Id\":\"This is a Id\"}}";

            PropertyTrackableTestObject testObject = new PropertyTrackableTestObject();
            ActionOnTestObject1(testObject);

            var changes = testObject.GetPropertyChangePack();
            var changes2 = testObject.GetPropertyChangePack();

            string json = SerializeUtil.ToJson(changes);
            string json2 = SerializeUtil.ToJson(changes2);

            Assert.AreEqual(json, rightJson);
            Assert.AreEqual(json2, rightJson2);
        }

        private static void ActionOnTestObject1(PropertyTrackableTestObject testObject)
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
            var attr = typeof(PropertyTrackableTestObject).GetProperty(nameof(PropertyTrackableTestObject.Id))?.GetCustomAttribute<AddtionalPropertyAttribute>(true);

            Assert.IsNotNull(attr);
        }

        [TestMethod]
        public void TestCppAddtionalProperties()
        {
            PropertyTrackableTestObject testObject = new PropertyTrackableTestObject();
            ActionOnTestObject1(testObject);

            //TODO:continue;
        }
    }
}