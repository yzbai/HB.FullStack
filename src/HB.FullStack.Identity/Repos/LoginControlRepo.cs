using HB.FullStack.Server.Identity.Models;
using HB.FullStack.Repository;
using HB.FullStack.Database;
using HB.FullStack.KVStore;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Server.Identity
{
    public class LoginControlRepo : KVStoreModelRepository<LoginControl>
    {
        public LoginControlRepo(IKVStore kvStore) : base(kvStore)
        {
        }
    }
}