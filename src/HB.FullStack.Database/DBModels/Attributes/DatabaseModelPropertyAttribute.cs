

using System;
using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.DBModels
{
    /// <summary>
    /// 对TableDomain中的属性的数据库表字段描述
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "<Pending>")]
    public class DatabaseModelPropertyAttribute : Attribute
    {
        public int PropertyOrder { get; internal set; }

        public bool FixedLength { get; set; }

        /// <summary>
        /// 字段长度
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// 字段是否可空
        /// </summary>
        public bool NotNull { get; set; }

        public bool NeedIndex { get; set; }

        /// <summary>
        /// 字段值是否唯一
        /// </summary>
        public bool Unique { get; set; }

        public Type? Converter { get; set; }

        public DatabaseModelPropertyAttribute([CallerLineNumber] int propertyOrder = 0)
        {
            PropertyOrder = propertyOrder;
        }
    }
}