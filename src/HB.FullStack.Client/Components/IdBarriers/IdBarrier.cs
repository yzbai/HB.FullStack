using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Components.IdBarriers
{
    public class IdBarrier : DbModel<long>, ITimestamp
    {
        [DbField(NeedIndex = true, Unique = true)]
        public long ClientId { get; set; } = -1;

        [DbField(NeedIndex = true, Unique = true)]
        public long ServerId { get; set; } = -1;

        [DbAutoIncrementPrimaryKey]
        public override long Id { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override bool Deleted { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public override string? LastUser { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public long Timestamp { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}