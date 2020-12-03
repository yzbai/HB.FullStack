using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Repository
{
    public class DatabaseWriteEventArgs : EventArgs
    {
        public UtcNowTicks UtcNowTicks { get; } = TimeUtil.UtcNowTicks;

    }
}
