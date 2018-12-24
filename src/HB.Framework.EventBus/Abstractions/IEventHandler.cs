using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.EventBus.Abstractions
{
    public interface IEventHandler //: IDisposable
    {
        string EventType { get; }

        void Handle(string jsonData);
    }
}
