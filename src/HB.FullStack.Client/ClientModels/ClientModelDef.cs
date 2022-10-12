using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.ClientModels
{
    public class ClientModelDef
    {
        public TimeSpan ExpiryTime { get; set; }

        public bool AllowOfflineRead { get; set; }
        public bool AllowOfflineAdd { get; set; }
        public bool AllowOfflineUpdate { get; set; }
        public bool AllowOfflineDelete { get; set; }
    }
}
