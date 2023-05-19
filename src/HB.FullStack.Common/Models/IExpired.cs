namespace HB.FullStack.Common
{
    public interface IExpired
    {
        /// <summary>
        /// Utc Timestamp
        /// null : expired at once
        /// </summary>
        long? ExpiredAt { get; set; }
    }
}