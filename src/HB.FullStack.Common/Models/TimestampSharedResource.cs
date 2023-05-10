namespace HB.FullStack.Common.Models
{
    public abstract class TimestampSharedResource : SharedResource, ITimestamp
    {
        public abstract long Timestamp { get; set; }
    }
}
