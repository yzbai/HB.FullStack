using HB.FullStack.KVStore;
using HB.FullStack.KVStore.KVStoreModels;

using MessagePack;

namespace HB.FullStack.KVStoreTests
{
    [KVStore]
    public class UserModel : KVStoreModel
    {
        public string? UserName { get; set; }

        public bool Activated { get; set; }

        public UserType Type { get; set; }
    }
}