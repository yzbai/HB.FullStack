using HB.FullStack.KVStore;
using HB.FullStack.KVStore.Entities;

using MessagePack;

namespace HB.FullStack.KVStoreTests
{
    [KVStore]
    public class UserEntity : KVStoreEntity
    {
        public string? UserName { get; set; }

        public bool Activated { get; set; }

        public UserType Type { get; set; }
    }
}