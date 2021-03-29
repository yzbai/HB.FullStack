using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.Def;

namespace HB.FullStack.Server.UserActivityTrace
{
    public class UserActivity : IdGenEntity
    {
        public long UserId { get; set; }   

    }
}
