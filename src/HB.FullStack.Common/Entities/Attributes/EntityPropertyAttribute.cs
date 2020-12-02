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
        public bool FixedLength { get; set; }

        /// <summary>
        /// 字段长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 字段默认值，Null表示没有没有默认值
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// 字段描述、备注
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 字段是否可空
        /// </summary>
        public bool NotNull { get; set; }

        /// <summary>
        /// 字段值是否唯一
        /// </summary>
        public bool Unique { get; set; }

        public Type? Converter { get; set; }

        public EntityPropertyAttribute()
        {
        }

        public EntityPropertyAttribute(string desc)
        {
            Description = desc;
        }
    }
}