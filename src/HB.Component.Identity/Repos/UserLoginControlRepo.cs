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
    internal class UserLoginControlRepo : KVStoreRepository<UserLoginControl>
    {
        public UserLoginControlRepo(IKVStore kvStore) : base(kvStore)
        {
        }


    }
}
