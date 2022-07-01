using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.ClientEntity
{
    public class ClientEntityDef
    {
        public TimeSpan ExpiryTime { get; set; }
        
        //TODO: 需要细化, 或者由业务决定
        public bool NeedLogined { get; set; }
        public bool AllowOfflineRead { get; set; }
        public bool AllowOfflineWrite { get; set; }
    }
}
