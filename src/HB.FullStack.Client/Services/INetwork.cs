using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Services
{
    //TODO: 这里只考虑网络本身的状态，没有考虑服务器是否在线的问题

    public interface INetwork
    {
        void Initialize();
        bool NetworkIsReady { get; }

        event Func<Task>? NetworkResumed;

        event Func<Task>? NetworkFailed;
    }
}
