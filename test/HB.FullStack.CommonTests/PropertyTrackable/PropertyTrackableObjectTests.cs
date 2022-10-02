using System;
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
        public void GetChangedPropertiesTest()
        {
            TestObject testObject = new TestObject();
            ActionOnTestObject(testObject);

            var changes = testObject.GetChangedProperties();

            string json = SerializeUtil.ToJson(changes);

            string rightJson = "[{\"PropertyName\":\"Name\",\"PropertyPropertyName\":null,\"OldValue\":null,\"NewValue\":\"TestName\"},{\"PropertyName\":\"InnerCls\",\"PropertyPropertyName\":null,\"OldValue\":null,\"NewValue\":{\"InnerName\":\"InnerTestName\"}},{\"PropertyName\":\"TestCollection\",\"PropertyPropertyName\":null,\"OldValue\":null,\"NewValue\":[\"x\"]}]";

            Assert.AreEqual(json, rightJson);
        }

        private static void ActionOnTestObject(TestObject testObject)
        {
            testObject.StartTrack();

            testObject.Name = "TestName";

            testObject.InnerCls = new InnerTestObject();
            testObject.InnerCls.InnerName = "InnerTestName";

            testObject.TestCollection = new ObservableCollection2<string>
            {
                "x"
            };
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
            ActionOnTestObject(testObject);

        }
    }
}