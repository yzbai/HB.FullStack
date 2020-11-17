using System;

namespace HB.Framework.Common.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class KVStoreEntityAttribute : Attribute
    {
        public string? InstanceName { get; set; }
    }
}