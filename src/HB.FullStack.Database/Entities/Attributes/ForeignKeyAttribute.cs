﻿#nullable enable

using System;

namespace HB.FullStack.Database.Entities
{
    /// <summary>
    /// 标识字段为外键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ForeignKeyAttribute : Attribute
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="baseType">引用表的类类型</param>
        public ForeignKeyAttribute(Type baseType, bool unique)
        {
            BaseType = baseType;
            IsUnique = unique;
        }

        public Type BaseType { get; }
        public bool IsUnique { get; }
    }
}