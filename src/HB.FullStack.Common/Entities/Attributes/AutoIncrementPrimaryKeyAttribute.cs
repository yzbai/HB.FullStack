#nullable enable

using System;

namespace HB.FullStack.Common.Entities
{
    /// <summary>
    /// 标识字段为主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AutoIncrementPrimaryKeyAttribute : Attribute
    {
        public AutoIncrementPrimaryKeyAttribute()
        {
        }
    }
}