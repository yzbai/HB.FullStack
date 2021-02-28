using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Common.IdGen
{
    public interface IDistributedIdGen
    {
        long GetId();
    }

    public static class StaticIdGen
    {
        public static IDistributedIdGen IdGen { get; set; } = null!;

        public static long GetId()
        {
            return IdGen.GetId();
        }
    }
}
