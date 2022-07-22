using System.Collections;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
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