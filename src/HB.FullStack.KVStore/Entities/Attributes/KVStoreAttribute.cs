using System;

namespace HB.FullStack.KVStore
{
    [AttributeUsage(AttributeTargets.Class)]
    public class KVStoreAttribute : Attribute
    {
        public string? InstanceName { get; set; }
    }
}