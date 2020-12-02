using System;

namespace HB.FullStack.Common.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class KVStoreEntityAttribute : Attribute
    {
        public string? InstanceName { get; set; }
    }
}