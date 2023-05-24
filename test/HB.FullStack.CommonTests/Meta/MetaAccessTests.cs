using System;
using System.Collections.Generic;
using System.Reflection;

using HB.FullStack.Common.Meta;
using HB.FullStack.Common.PropertyTrackable;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CommonTests.Meta
{
    public class MetaAccessObj
    {
        [AddtionalProperty]
        public string? Name { get; set; } = "TestName";

        [AddtionalProperty]
        public IList<string> Keys { get; set; } = new List<string> { "1", "2" };
    }

    [TestClass()]
    public class MetaAccessTests
    {
        [TestMethod()]
        public void CreateGetPropertyValuesDelegateTest()
        {
            IList<PropertyInfo> properties = typeof(MetaAccessObj).GetPropertyInfosByAttribute<AddtionalPropertyAttribute>();

            Func<object, object?[]> func = MetaAccess.CreateGetPropertyValuesDelegate(typeof(MetaAccessObj), properties);

            MetaAccessObj obj = new MetaAccessObj();

            object?[] values = func(obj);

            Assert.IsTrue(values[0] is string str && str == "TestName");

            Assert.IsTrue(values[1] is IList<string> list && list.Count == 2 && list[0] == "1" && list[1] == "2");
        }

        [TestMethod()]
        public void GetPropertyValuesByAttributeTest()
        {
            MetaAccessObj obj = new MetaAccessObj();

            PropertyNameValue[] values = MetaAccess.GetPropertyValuesByAttribute<AddtionalPropertyAttribute>(obj);

            Assert.IsTrue(values[0].Name == nameof(MetaAccessObj.Name) && values[0].Value!.ToString() == "TestName");

            Assert.IsTrue(values[1].Name == nameof(MetaAccessObj.Keys) && values[1].Value is IList<string> list && list.Count == 2 && list[0] == "1" && list[1] == "2");
        }

        [TestMethod()]
        public void CreateGetPropertyValuesDelegate2Test()
        {
            IList<PropertyInfo> properties = typeof(MetaAccessObj).GetPropertyInfosByAttribute<AddtionalPropertyAttribute>();

            Func<object, PropertyNameValue[]> func = MetaAccess.CreateGetPropertyValuesDelegate2(typeof(MetaAccessObj), properties);

            MetaAccessObj obj = new MetaAccessObj();

            PropertyNameValue[] values = func(obj);

            Assert.IsTrue(values[0].Name == nameof(MetaAccessObj.Name) && values[0].Value!.ToString() == "TestName");

            Assert.IsTrue(values[1].Name == nameof(MetaAccessObj.Keys) && values[1].Value is IList<string> list && list.Count == 2 && list[0] == "1" && list[1] == "2");
        }
    }
}