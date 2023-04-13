using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Services
{
    public interface ICommonEvents
    {
        event Func<Task>? AppStart;

        event Func<Task>? AppResume;

        event Func<Task>? AppSleep;

        event Func<Task>? AppExit;
    }
}
