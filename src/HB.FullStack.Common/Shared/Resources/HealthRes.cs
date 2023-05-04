using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared
{
    public class ServerHealthRes : SharedResource
    {
        public ServerHealthy ServerHealthy { get; set; }
    }
}
