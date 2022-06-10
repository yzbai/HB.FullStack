using System.Collections.Generic;

namespace HB.FullStack.LockTests
{
    public class Mocker
    {
        public static IList<string> MockResourcesWithThree()
        {
            return new List<string> { "aa", "bb", "cc" };
        }
    }
}