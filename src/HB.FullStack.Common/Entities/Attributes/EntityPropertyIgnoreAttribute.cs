#nullable enable

using System;

namespace HB.FullStack.Common.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityPropertyIgnoreAttribute : System.Attribute
    {
    }
}