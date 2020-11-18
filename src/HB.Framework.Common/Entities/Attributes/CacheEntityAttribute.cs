using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Common.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheEntityAttribute : Attribute
    {
        public bool AllowMultipleRetrieve { get; set; }

        public TimeSpan? SlidingAliveTime { get; set; }

        public CacheEntityAttribute(bool allowMultipleRetrieve = false)
        {
            AllowMultipleRetrieve = allowMultipleRetrieve;
        }
    }
}
