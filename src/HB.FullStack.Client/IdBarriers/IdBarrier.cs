using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.XamarinForms.IdBarriers
{
    public class IdBarrier : AutoIncrementIdModel
    {
        [ModelProperty(NeedIndex = true, Unique = true)]
        public long ClientId { get; set; } = -1;

        [ModelProperty(NeedIndex = true, Unique = true)]
        public long ServerId { get; set; } = -1;
    }
}
