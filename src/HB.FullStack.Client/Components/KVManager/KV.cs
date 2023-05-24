using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Components.KVManager
{
    [DbModel(ConflictCheckMethods.Ignore)]
    public class KV : DbModel<string>, IExpired
    {
        public override string Id { get; set; } = null!;

        public string? Value { get; set; }

        public long? ExpiredAt { get; set; }

        public override bool Deleted { get; set; }

        public override string? LastUser { get; set; }
    }
}