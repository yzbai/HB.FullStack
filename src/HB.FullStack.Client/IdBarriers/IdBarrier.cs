using HB.FullStack.Database.DbModels;

namespace HB.FullStack.XamarinForms.IdBarriers
{
    public class IdBarrier : TimestampAutoIncrementIdDbModel
    {
        [DBModelProperty(NeedIndex = true, Unique = true)]
        public long ClientId { get; set; } = -1;

        [DBModelProperty(NeedIndex = true, Unique = true)]
        public long ServerId { get; set; } = -1;
    }
}
