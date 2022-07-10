using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Cache.CacheModels
{
    public interface ICacheModel : IModel
    {
        //int Version { get; set; }
        public long Timestamp { get; set; }
    }
}
