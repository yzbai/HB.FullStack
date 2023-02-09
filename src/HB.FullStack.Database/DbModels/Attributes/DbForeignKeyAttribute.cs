

using System;

namespace HB.FullStack.Database.DbModels
{
    /// <summary>
    /// 标识字段为外键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DbForeignKeyAttribute : Attribute
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="baseType">引用表的类类型</param>
        public DbForeignKeyAttribute(Type baseType, bool isUnique)
        {
            BaseType = baseType;
            IsUnique = isUnique;
        }

        public Type BaseType { get; }
        public bool IsUnique { get; }
    }
}