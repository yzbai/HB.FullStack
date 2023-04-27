using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Repository;
using HB.FullStack.Server.Identity.Models;

namespace HB.FullStack.Server.Identity.Repos
{
    public class UserProfileRepo : ModelRepository<UserProfile>
    {
        protected override Task InvalidateCacheItemsOnChanged(object sender, DBChangeEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
