using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Database.Def;

namespace HB.FullStack.Mobile.IdBarriers
{
    public class IdBarrier : AutoIncrementIdEntity
    {
        [EntityProperty(NeedIndex = true, Unique = true)]
        public long ClientId { get; set; } = -1;

        [EntityProperty(NeedIndex = true, Unique = true)]
        public long ServerId { get; set; } = -1;
    }
}
