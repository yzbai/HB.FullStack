using System.Reflection;

namespace HB.FullStack.Utils.Tests
{
    [TestClass]
    public class PropertyDelegateCreatorTests
    {
        private class TestClass
        {
            public string Name { get; set; } = "DefaultName";
            public int Age { get; set; } = 25;
            public static string StaticProperty { get; set; } = "StaticValue";
            public static string StaticProperty2 { get; set; } = "StaticValue2";
            public int? NullableProperty { get; set; }
            public List<int> ListProperty { get; set; } = new List<int> { 1, 2, 3 };

            public string[] Strings { get; set; } = ["string1", "string2"];

            public TestInnerClass InnerClass { get; set; } = new TestInnerClass();

            public List<TestInnerClass> Inners { get; set; } = [
                new TestInnerClass { InnerName="11" },
                new TestInnerClass{ InnerName = "22"}];
        }

        private class TestInnerClass
        {
            public string InnerName { get; set; } = "InnerName";

            public int InnerAge { get; set; } = 26;

            public List<int> ListProperty { get; set; } = new List<int> { 1, 2, 3 };

        }

        private class TestClassWithPrivateSetter
        {
            public string ReadOnlyProperty { get; private set; } = "ReadOnlyValue";
        }

        [TestMethod]
        public void CreateGetDelegate_ShouldReturnCorrectValue()
        {
            // Arrange
            var property = typeof(TestClass).GetProperty(nameof(TestClass.Name))!;
            var instance = new TestClass { Name = "TestName" };

            // Act
            var getDelegate = PropertyDelegateCreator.CreateGetDelegate(property);
            var result = getDelegate(instance);

            // Assert
            Assert.AreEqual("TestName", result);
        }

        [TestMethod]
        public void CreateSetDelegate_ShouldSetCorrectValue()
        {
            // Arrange
            var property = typeof(TestClass).GetProperty(nameof(TestClass.Name))!;
            var instance = new TestClass();

            // Act
            var setDelegate = PropertyDelegateCreator.CreateSetDelegate(property);
            setDelegate(instance, "NewName");

            // Assert
            Assert.AreEqual("NewName", instance.Name);
        }

        [TestMethod]
        public void CreateBatchGetDelegate_ShouldReturnCorrectValues()
        {
            // Arrange
            var properties = new List<PropertyInfo>
            {
                typeof(TestClass).GetProperty(nameof(TestClass.Name))!,
                typeof(TestClass).GetProperty(nameof(TestClass.Age))!
            };
            var instance = new TestClass { Name = "BatchName", Age = 30 };

            // Act
            var batchGetDelegate = PropertyDelegateCreator.CreateBatchGetDelegate(properties, typeof(TestClass));
            var result = batchGetDelegate(instance);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("BatchName", result[0]);
            Assert.AreEqual(30, result[1]);
        }

        [TestMethod]
        public void CreateBatchGetDelegate_ShouldReturnCorrectValues_StaticProperties()
        {
            // Arrange
            var properties = new List<PropertyInfo>
            {
                typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty))!,
                typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty2))!,

            };

            // Act
            var batchGetDelegate = PropertyDelegateCreator.CreateBatchGetDelegate(properties, typeof(TestClass));
            var result = batchGetDelegate(null);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(TestClass.StaticProperty, result[0]);
            Assert.AreEqual(TestClass.StaticProperty2, result[1]);
        }

        [TestMethod]
        public void CreateBatchGetDelegate_ShouldReturnCorrectValues_MixStaticProperties()
        {
            // Arrange
            var properties = new List<PropertyInfo>
            {
                typeof(TestClass).GetProperty(nameof(TestClass.Name))!,
                typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty))!,

            };
            var instance = new TestClass { Name = "BatchName", Age = 30 };

            // Act
            var batchGetDelegate = PropertyDelegateCreator.CreateBatchGetDelegate(properties, typeof(TestClass));
            var result = batchGetDelegate(instance);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("BatchName", result[0]);
            Assert.AreEqual(TestClass.StaticProperty, result[1]);
        }

        [TestMethod]
        public void CreateBatchGetDelegate2_ShouldReturnCorrectPropertyNameValues()
        {
            // Arrange
            var properties = new List<PropertyInfo>
            {
                typeof(TestClass).GetProperty(nameof(TestClass.Name))!,
                typeof(TestClass).GetProperty(nameof(TestClass.Age))!,
            };
            var instance = new TestClass { Name = "BatchName2", Age = 35 };

            // Act
            var batchGetDelegate2 = PropertyDelegateCreator.CreateBatchGetDelegate2(properties, typeof(TestClass));
            var result = batchGetDelegate2(instance);

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("Name", result[0].Name);
            Assert.AreEqual("BatchName2", result[0].Value);
            Assert.AreEqual("Age", result[1].Name);
            Assert.AreEqual(35, result[1].Value);
        }

        [TestMethod]
        public void CreateGetQueryStringDelegate_ShouldReturnCorrectQueryStrings()
        {
            var correctString = "Name=DefaultName&Age=25&ListProperty=1&ListProperty=2&ListProperty=3&Strings=string1&Strings=string2&InnerClass.InnerName=InnerName&InnerClass.InnerAge=26&InnerClass.ListProperty=1&InnerClass.ListProperty=2&InnerClass.ListProperty=3&Inners[0].InnerName=11&Inners[0].InnerAge=26&Inners[0].ListProperty=1&Inners[0].ListProperty=2&Inners[0].ListProperty=3&Inners[1].InnerName=22&Inners[1].InnerAge=26&Inners[1].ListProperty=1&Inners[1].ListProperty=2&Inners[1].ListProperty=3";

            // Arrange
            var properties = new List<PropertyInfo>
            {
                typeof(TestClass).GetProperty(nameof(TestClass.Name))!,
                typeof(TestClass).GetProperty(nameof(TestClass.Age))!,
                typeof(TestClass).GetProperty(nameof(TestClass.ListProperty))!,
                typeof(TestClass).GetProperty(nameof(TestClass.Strings))!,
                typeof(TestClass).GetProperty(nameof(TestClass.InnerClass))!,
                typeof(TestClass).GetProperty(nameof(TestClass.Inners))!,
            };

            var instance = new TestClass();

            // Act
            var queryStringDelegate = PropertyDelegateCreator.CreateGetQueryStringDelegate(properties, typeof(TestClass));
            var result = string.Join('&', queryStringDelegate(instance));

            // Assert
            Assert.AreEqual(correctString, result);
        }

        [TestMethod]
        public void CreateGetQueryStringDelegate_ShouldThrowIfContainsStaticProperty()
        {
            var properties = new List<PropertyInfo>
            {
                typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty))!
            };
            var instance = new TestClass { Name = "QueryName", Age = 40 };

            // Act
            Assert.ThrowsException<ArgumentException>(
                () => PropertyDelegateCreator.CreateGetQueryStringDelegate(properties, typeof(TestClass)));
        }

        [TestMethod]
        public void CreateGetDelegate_ShouldHandleStaticProperties()
        {
            // Arrange
            var property = typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty))!;

            // Act
            var getDelegate = PropertyDelegateCreator.CreateGetDelegate(property);
            var result = getDelegate(null);

            // Assert
            Assert.AreEqual("StaticValue", result);
        }

        [TestMethod]
        public void CreateSetDelegate_ShouldHandleStaticProperties()
        {
            // Arrange
            var property = typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty))!;

            // Act
            var setDelegate = PropertyDelegateCreator.CreateSetDelegate(property);
            setDelegate(null, "NewStaticValue");

            // Assert
            Assert.AreEqual("NewStaticValue", TestClass.StaticProperty);
        }

        [TestMethod]
        public void CreateGetDelegate_ShouldThrowForNullProperty()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => PropertyDelegateCreator.CreateGetDelegate(null));
        }

        [TestMethod]
        public void CreateGetDelegate_ShouldThrowForNullInputToDelegate()
        {
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Name));
            var getDelegate = PropertyDelegateCreator.CreateGetDelegate(propertyInfo);

            var rtException = Assert.ThrowsException<ArgumentNullException>(() => getDelegate(null!));

            Assert.AreEqual(rtException.ParamName, "inputObject");
        }

        [TestMethod]
        public void CreateSetDelegate_ShouldThrowForNullInputToDelegate()
        {
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Name));
            var setDelegate = PropertyDelegateCreator.CreateSetDelegate(propertyInfo);

            var rtException = Assert.ThrowsException<ArgumentNullException>(() => setDelegate(null!, "TestName"));

            Assert.AreEqual(rtException.ParamName, "inputObject");
        }

        [TestMethod]
        public void CreateSetDelegate_ShouldThrowForNullProperty()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => PropertyDelegateCreator.CreateSetDelegate(null!));
        }

        [TestMethod]
        public void CreateSetDelegate_ShouldHandlePrivateSetters()
        {
            // Arrange
            var property = typeof(TestClassWithPrivateSetter).GetProperty(
                nameof(TestClassWithPrivateSetter.ReadOnlyProperty))!;
            var instance = new TestClassWithPrivateSetter();

            // Act
            var setDelegate = PropertyDelegateCreator.CreateSetDelegate(property);
            setDelegate(instance, "NewValue");

            // Assert
            Assert.AreEqual(instance.ReadOnlyProperty, "NewValue");

        }

        [TestMethod]
        public void CreateBatchGetDelegate_ShouldHandleEmptyPropertyList()
        {
            // Arrange
            var properties = new List<PropertyInfo>();
            var instance = new TestClass();

            // Act

            Assert.ThrowsException<ArgumentNullException>(() => PropertyDelegateCreator.CreateBatchGetDelegate(properties, typeof(TestClass)));
        }

        [TestMethod]
        public void CreateBatchGetDelegate_ShouldThrowForNullInstance()
        {
            // Arrange
            var properties = new List<PropertyInfo>
            {
                typeof(TestClass).GetProperty(nameof(TestClass.Name))!
            };

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var batchGetDelegate = PropertyDelegateCreator.CreateBatchGetDelegate(properties, typeof(TestClass));
                batchGetDelegate(null!);
            });
        }

        [TestMethod]
        public void CreateBatchGetDelegate2_ShouldHandleNullableProperties()
        {
            // Arrange
            var properties = new List<PropertyInfo>
            {
                typeof(TestClass).GetProperty(nameof(TestClass.NullableProperty))!
            };
            var instance = new TestClass { NullableProperty = null };

            // Act
            var batchGetDelegate2 = PropertyDelegateCreator.CreateBatchGetDelegate2(properties, typeof(TestClass));
            var result = batchGetDelegate2(instance);

            // Assert
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("NullableProperty", result[0].Name);
            Assert.IsNull(result[0].Value);
        }

        [TestMethod]
        public void CreateGetQueryStringDelegate_ShouldHandleEmptyPropertyList()
        {
            // Arrange
            var properties = new List<PropertyInfo>();
            var instance = new TestClass();

            // Act
            var queryStringDelegate = PropertyDelegateCreator.CreateGetQueryStringDelegate(properties, typeof(TestClass));
            var result = queryStringDelegate(instance);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CreateGetQueryStringDelegate_ShouldHandleComplexProperties()
        {
            // Arrange
            var properties = new List<PropertyInfo>
            {
                typeof(TestClass).GetProperty(nameof(TestClass.ListProperty))!
            };
            var instance = new TestClass();

            // Act & Assert
            Assert.ThrowsException<NotSupportedException>(() =>
            {
                var queryStringDelegate = PropertyDelegateCreator.CreateGetQueryStringDelegate(properties, typeof(TestClass));
                queryStringDelegate(instance);
            });
        }

        [TestMethod]
        public void CreateGetDelegate_ShouldHandleValueTypes()
        {
            // Arrange
            var property = typeof(TestClass).GetProperty(nameof(TestClass.Age))!;
            var instance = new TestClass { Age = 42 };

            // Act
            var getDelegate = PropertyDelegateCreator.CreateGetDelegate(property);
            var result = getDelegate(instance);

            // Assert
            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void CreateSetDelegate_ShouldHandleValueTypes()
        {
            // Arrange
            var property = typeof(TestClass).GetProperty(nameof(TestClass.Age))!;
            var instance = new TestClass();

            // Act
            var setDelegate = PropertyDelegateCreator.CreateSetDelegate(property);
            setDelegate(instance, 99);

            // Assert
            Assert.AreEqual(99, instance.Age);
        }

        [TestMethod]
        public void CreateBatchGetDelegate_ShouldHandleMixedPropertyTypes()
        {
            // Arrange
            var properties = new List<PropertyInfo>
            {
                typeof(TestClass).GetProperty(nameof(TestClass.Name))!,
                typeof(TestClass).GetProperty(nameof(TestClass.Age))!,
                typeof(TestClass).GetProperty(nameof(TestClass.NullableProperty))!
            };
            var instance = new TestClass { Name = "MixedName", Age = 50, NullableProperty = null };

            // Act
            var batchGetDelegate = PropertyDelegateCreator.CreateBatchGetDelegate(properties, typeof(TestClass));
            var result = batchGetDelegate(instance);

            // Assert
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("MixedName", result[0]);
            Assert.AreEqual(50, result[1]);
            Assert.IsNull(result[2]);
        }
    }
}
