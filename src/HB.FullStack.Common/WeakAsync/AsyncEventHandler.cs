using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System
{
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public delegate Task AsyncEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs args);

    public delegate Task AsyncEventHandler(object sender, EventArgs args);

    public delegate Task AsyncEventHandler<TSender>(TSender sender, EventArgs args);
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
}
