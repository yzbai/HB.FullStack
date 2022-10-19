using System;

namespace HB.FullStack.KVStore
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class KVStoreAttribute : Attribute
    {
        public string? InstanceName { get; set; }
    }
}