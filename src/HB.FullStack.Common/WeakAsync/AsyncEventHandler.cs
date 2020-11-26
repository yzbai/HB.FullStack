using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public delegate Task AsyncEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs args);

    public delegate Task AsyncEventHandler(object sender, EventArgs args);

    public delegate Task AsyncEventHandler<TSender>(TSender sender, EventArgs args);
}
