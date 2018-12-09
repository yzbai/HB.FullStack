using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Framework.EventBus.Abstractions
{
    /// <summary>
    /// 以Topic为中心的时间总线
    /// 需要保证事件的不丢失，每一个event都有追溯，都有因果
    /// </summary>
    public interface IEventBus
    {

        void Publish(EventMessage eventMessage);
    }
}
