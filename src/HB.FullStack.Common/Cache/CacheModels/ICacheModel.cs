using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Cache
{

    /// <summary>
    /// 只有标记了CacheModelAttribute，才会自动启动Model的Cache功能
    /// </summary>
    public interface ICacheModel : IModel
    {
        //int Version { get; set; }
        public long Timestamp { get; set; }
    }
}
