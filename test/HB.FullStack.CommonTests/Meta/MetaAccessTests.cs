using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Meta;
using HB.FullStack.Common.PropertyTrackable;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.Common.Meta.Tests
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

            PropertyValue[] values = MetaAccess.GetPropertyValuesByAttribute<AddtionalPropertyAttribute>(obj);

            Assert.IsTrue(values[0].PropertyName == nameof(MetaAccessObj.Name) && values[0].Value!.ToString() == "TestName");

            Assert.IsTrue(values[1].PropertyName == nameof(MetaAccessObj.Keys) && values[1].Value is IList<string> list && list.Count == 2 && list[0] == "1" && list[1] == "2");
        }

        [TestMethod()]
        public void CreateGetPropertyValuesDelegate2Test()
        {
            IList<PropertyInfo> properties = typeof(MetaAccessObj).GetPropertyInfosByAttribute<AddtionalPropertyAttribute>();

            Func<object, PropertyValue[]> func = MetaAccess.CreateGetPropertyValuesDelegate2(typeof(MetaAccessObj), properties);

            MetaAccessObj obj = new MetaAccessObj();

            PropertyValue[] values = func(obj);

            Assert.IsTrue(values[0].PropertyName == nameof(MetaAccessObj.Name) && values[0].Value!.ToString() == "TestName");

            Assert.IsTrue(values[1].PropertyName == nameof(MetaAccessObj.Keys) && values[1].Value is IList<string> list && list.Count == 2 && list[0] == "1" && list[1] == "2");
        }
    }
}