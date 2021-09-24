using HB.FullStack.Identity.Entities;
using HB.FullStack.Repository;
using HB.FullStack.Database;
using HB.FullStack.KVStore;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Identity
{
    internal class LoginControlRepo : KVStoreEntityRepository<LoginControl>
    {
        public LoginControlRepo(IKVStore kvStore) : base(kvStore)
        {
        }


    }
}
