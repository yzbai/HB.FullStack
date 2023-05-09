
using System;
using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.DbModels
{
    /// <summary>
    /// 对TableDomain中的属性的数据库表字段描述,可以默认不添加
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "<Pending>")]
    public class DbFieldAttribute : Attribute
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

        public DbFieldAttribute([CallerLineNumber] int propertyOrder = 0)
        {
            PropertyOrder = propertyOrder;
        }
    }
}