using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.EventBus.Abstractions
{
    public interface IEventHandler //: IDisposable
    {
        //string EventType { get; }

        Task HandleAsync(string jsonData, CancellationToken cancellationToken);
    }
}
