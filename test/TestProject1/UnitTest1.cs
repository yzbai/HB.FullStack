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
    }

    public class TestGeneric<T>
    {
        public TestGeneric()
        {
            Console.WriteLine(nameof(T));
        }
    }
}