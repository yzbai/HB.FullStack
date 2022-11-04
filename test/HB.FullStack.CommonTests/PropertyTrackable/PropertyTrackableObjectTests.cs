using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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
            string rightJson = "[{\"PropertyName\":\"Name\",\"PropertyPropertyName\":null,\"OldValue\":null,\"NewValue\":\"TestName\"},{\"PropertyName\":\"InnerRecord\",\"PropertyPropertyName\":null,\"OldValue\":{\"InnerName\":null},\"NewValue\":{\"InnerName\":\"InnerTestRecord\"}},{\"PropertyName\":\"Immutables\",\"PropertyPropertyName\":null,\"OldValue\":[\"x\"],\"NewValue\":[\"x\",\"y\"]}]";

            TestObject testObject = new TestObject();
            ActionOnTestObject1(testObject);

            var changes = testObject.GetChangedProperties();

            string json = SerializeUtil.ToJson(changes);

            Assert.AreEqual(json, rightJson);
        }

        [TestMethod()]
        public void GetChangedPropertiesTest2()
        {
            string rightJson = "[{\"PropertyName\":\"Name\",\"PropertyPropertyName\":null,\"OldValue\":null,\"NewValue\":\"TestName\"},{\"PropertyName\":\"InnerRecord\",\"PropertyPropertyName\":null,\"OldValue\":null,\"NewValue\":{\"InnerName\":\"InnerTestRecord\"}},{\"PropertyName\":\"Immutables\",\"PropertyPropertyName\":null,\"OldValue\":null,\"NewValue\":[\"x\",\"y\"]}]";

            TestObject testObject = new TestObject();
            ActionOnTestObject2(testObject);

            var changes = testObject.GetChangedProperties();

            string json = SerializeUtil.ToJson(changes);


            Assert.AreEqual(json, rightJson);
        }

        private static void ActionOnTestObject1(TestObject testObject)
        {
            testObject.InnerRecord = new InnerTestRecord(null);
            testObject.Immutables = ImmutableList.Create("x");

            testObject.StartTrack();

            testObject.Name = "TestName";

            testObject.InnerRecord = testObject.InnerRecord with { InnerName = "InnerTestRecord" };
            testObject.Immutables = testObject.Immutables.Add("y");
            
        }

        private static void ActionOnTestObject2(TestObject testObject)
        {

            testObject.StartTrack();

            testObject.Name = "TestName";

            testObject.InnerRecord = new InnerTestRecord(null);

            testObject.InnerRecord = testObject.InnerRecord with { InnerName = "InnerTestRecord" };

            testObject.Immutables = ImmutableList.Create("x");
            testObject.Immutables = testObject.Immutables.Add("y");
        }

        [TestMethod]
        public void TestAttributeForward()
        {
            var attr = typeof(TestObject).GetProperty(nameof(TestObject.ForwordAttributeName))?.GetCustomAttribute<AddtionalPropertyAttribute>();

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