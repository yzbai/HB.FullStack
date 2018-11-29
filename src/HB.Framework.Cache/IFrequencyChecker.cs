using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Cache
{
    public interface IFrequencyChecker
    {
        Task<bool> CheckAsync(string clientId, TimeSpan aliveTimeSpan);
    }
}
