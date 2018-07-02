
using System;

namespace HB.Framework.Database.Entity
{
    /// <summary>
    /// 标识字段为外键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DatabaseForeignKeyAttribute : DatabaseEntityPropertyAttribute
    {
        /// <summary>
        /// 引用的表类
        /// </summary>
        private Type baseType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="baseType">引用表的类类型</param>
        public DatabaseForeignKeyAttribute(string desc, Type baseType)
            : base(desc)
        {
            this.baseType = baseType;
        }
    }
}
