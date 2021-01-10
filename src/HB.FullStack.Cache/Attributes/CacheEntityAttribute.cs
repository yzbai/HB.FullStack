using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheEntityAttribute : Attribute
    {
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

        public CacheEntityAttribute()
        {
        }
    }
}
