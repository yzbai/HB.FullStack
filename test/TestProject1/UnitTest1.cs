using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;

using HB.FullStack.Database;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test_ConnectionString()
        {
        }

        [TestMethod]
        public void TestMethod1()
        {
            var item = new TestGeneric<UnitTest1>();

            var types = item.GetType().GenericTypeArguments;

        }

        /// <summary>
        /// 验证协变
        /// </summary>
        [TestMethod]
        public void TestXieBian()
        {

            IList<A> list = new List<A>();

            Assert.IsTrue(list is IEnumerable);

        }

        /// <summary>
        /// 验证协变
        /// </summary>
        [TestMethod]
        public void TestXieBian2()
        {

            IList<A> list = new List<A>();

            Assert.IsTrue(list is IEnumerable<BaseA>);
        }

        /// <summary>
        /// 验证协变
        /// </summary>
        [TestMethod]
        public void TestXieBian3()
        {

            IList<B> list = new List<B>();

            Assert.IsTrue(list is IEnumerable<object> e && e.Count() == 0);

            IEnumerable er = list;

        }

        [TestMethod]
        public void TestConverter()
        {
            var guidConvertor = TypeDescriptor.GetConverter(typeof(Guid));

            Guid guid = Guid.NewGuid();

            string? str = guidConvertor.ConvertToString(guid);

            Guid? guid2 = (Guid?)guidConvertor.ConvertFromString(str!);

            Assert.AreEqual(guid, guid2!.Value);
        }

    }

    public class BaseA
    {

    }

    public class A : BaseA
    {
        public sealed override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class AA : A
    {
        //Can not override GetHashCode
    }

    public class AAA : AA
    {
        //Can not override GetHashCode
    }

    public class B
    {

    }

    public class TestGeneric<T>
    {
        public TestGeneric()
        {
            Console.WriteLine(nameof(T));
        }
    }
}