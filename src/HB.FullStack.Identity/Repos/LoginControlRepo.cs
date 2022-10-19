using HB.FullStack.Identity.Models;
using HB.FullStack.Repository;
using HB.FullStack.Database;
using HB.FullStack.KVStore;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Identity
{
    public class LoginControlRepo : KVStoreModelRepository<LoginControl>
    {
        public LoginControlRepo(IKVStore kvStore) : base(kvStore)
        {
        }
    }
}