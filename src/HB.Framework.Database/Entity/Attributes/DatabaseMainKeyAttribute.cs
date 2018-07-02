
using System;

namespace HB.Framework.Database.Entity
{
    /// <summary>
    /// 标识字段为主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DatabaseMainKeyAttribute : DatabaseEntityPropertyAttribute
    {
        public DatabaseMainKeyAttribute(string desc)
            : base(desc)
        {

        }
    }
}
