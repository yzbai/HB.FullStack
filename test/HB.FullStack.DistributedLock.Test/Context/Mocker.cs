using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.DistributedLock.Test
{
    public class Mocker
    {
        public static List<string> MockResourcesWithOne()
        {
            return new List<string> { "aa" };
        }
        public static List<string> MockResourcesWithThree()
        {
            return new List<string> { "aa", "bb", "cc" };
        }
    }
}
