using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Common.IdGen
{
    public interface IDistributedLongIdGen
    {
        long GetId();
    }
}
