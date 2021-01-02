using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.DistributedLock.Test
{
    public static class Mocker
    {
        public static IList<string> MockResourcesWithOne()
        {
            return new List<string> { "aa" };
        }
        public static IList<string> MockResourcesWithThree()
        {
            return new List<string> { "aa", "bb", "cc" };
        }
    }
}
