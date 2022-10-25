using System;

namespace HB.FullStack.Repository
{
    public class DBChangeEventArgs:EventArgs
    {
        public DBChangeType ChangeType { get;set; }
    }
}
