using HB.FullStack.KVStore;
using HB.FullStack.KVStore.Entities;

using MessagePack;

namespace HB.FullStack.KVStoreTests
{
    [KVStore]
    [MessagePackObject]
    public class UserEntity : KVStoreEntity
    {
        [Key(7)]
        public string? UserName { get; set; }

        [Key(8)]
        public bool Activated { get; set; }

        [Key(9)]
        public UserType Type { get; set; }
    }
}