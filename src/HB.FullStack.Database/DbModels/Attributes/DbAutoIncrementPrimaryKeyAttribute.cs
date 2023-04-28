

using System;

namespace HB.FullStack.Database.DbModels
{
    /// <summary>
    /// 标识字段为主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DbAutoIncrementPrimaryKeyAttribute : DbPrimaryKeyAttribute
    {
        public DbAutoIncrementPrimaryKeyAttribute()
        {
        }
    }
}