using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Common.IdGen
{
    public interface IDistributedIdGen
    {
        public static IDistributedIdGen IdGen { get; set; } = null!;

        long GetId();
    }
}
