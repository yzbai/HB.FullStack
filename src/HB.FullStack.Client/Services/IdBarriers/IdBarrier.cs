using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Services.IdBarriers
{
    public class IdBarrier : TimestampAutoIncrementIdDbModel
    {
        [DbField(NeedIndex = true, Unique = true)]
        public long ClientId { get; set; } = -1;

        [DbField(NeedIndex = true, Unique = true)]
        public long ServerId { get; set; } = -1;
    }
}
