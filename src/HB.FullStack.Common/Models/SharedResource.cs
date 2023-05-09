namespace HB.FullStack.Common.Models
{
    /// <summary>
    /// One kind of Data Transfer Objects.Mainly using on net.
    /// </summary>
    public class SharedResource : IDTO
    {
    }

    public abstract class TimestampSharedResource : SharedResource
    {
        public abstract long Timestamp { get; set; }
    }
}
