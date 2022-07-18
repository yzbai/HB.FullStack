namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Console.WriteLine("Test Results!");

            Assert.IsTrue(true);

            throw new ArgumentException("thisis");
        }
    }
}