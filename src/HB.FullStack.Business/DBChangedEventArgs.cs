using System;

namespace HB.FullStack.Repository
{
    public class DBChangedEventArgs:EventArgs
    {
        public DBChangeType ChangeType { get;set; }
    }
}
