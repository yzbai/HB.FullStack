using System;

namespace HB.FullStack.Repository
{
    public class ModelChangeEventArgs:EventArgs
    {
        public ModelChangeType ChangeType { get;set; }
    }
}
