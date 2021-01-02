using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.EventBus.Abstractions
{
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public interface IEventHandler //: IDisposable
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        //string EventType { get; }

        Task HandleAsync(string jsonData, CancellationToken cancellationToken);
    }
}
