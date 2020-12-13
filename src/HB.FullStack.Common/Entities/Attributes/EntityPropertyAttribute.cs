#nullable enable

using System;

namespace HB.FullStack.Common.Entities
{
    /// <summary>
    /// 对TableDomain中的属性的数据库表字段描述
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityPropertyAttribute : Attribute
    {
        public const int MediumLength = 2048;

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

        public object? TypeConverter { get; set; }

        public EntityPropertyAttribute()
        {
        }
    }
}