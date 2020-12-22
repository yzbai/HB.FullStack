#nullable enable

using System;
using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.Def
{
    /// <summary>
    /// 对TableDomain中的属性的数据库表字段描述
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityPropertyAttribute : Attribute
    {
        public int PropertyOrder { get; set; }

        public bool FixedLength { get; set; }

        /// <summary>
        /// 字段长度
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// 字段是否可空
        /// </summary>
        public bool NotNull { get; set; }

        /// <summary>
        /// 字段值是否唯一
        /// </summary>
        public bool Unique { get; set; }

        public Type? Converter { get; set; }

        public EntityPropertyAttribute([CallerLineNumber] int propertyOrder = 0)
        {
            PropertyOrder = propertyOrder;
        }
    }
}