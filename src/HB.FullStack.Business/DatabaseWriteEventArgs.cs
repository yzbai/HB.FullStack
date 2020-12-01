using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Business
{
    public class DatabaseWriteEventArgs : EventArgs
    {
        public long UtcNowTicks { get; } = DateTimeOffset.UtcNow.Ticks;

    }
}
