namespace System
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CacheModelAttribute : Attribute
    {
        /// <summary>
        /// null表示使用默认的Cache
        /// </summary>
        public string? CacheInstanceName { get; set; }


        /// <summary>
        /// 在没有超多最多时间范围内，每次续命多久
        /// </summary>
        public long SlidingSeconds { get; set; } = -1;

        /// <summary>
        /// 最多活多长时间
        /// </summary>
        public long MaxAliveSeconds { get; set; } = -1;

        //public bool IsBatchEnabled { get; set; }

    }
}
