#nullable enable

using System;

namespace HB.Framework.Common.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityPropertyIgnoreAttribute : System.Attribute
    {
    }
}