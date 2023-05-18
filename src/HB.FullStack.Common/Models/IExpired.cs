namespace HB.FullStack.Common
{
    public interface IExpired
    {
        /// <summary>
        /// Utc Timestamp
        /// </summary>
        long? ExpiredAt { get; set; }
    }
}