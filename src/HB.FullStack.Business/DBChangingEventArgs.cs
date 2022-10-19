using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Repository
{

    public class DBChangingEventArgs : EventArgs
    {
        public DBChangeType ChangeType { get; set; }
    }
}
