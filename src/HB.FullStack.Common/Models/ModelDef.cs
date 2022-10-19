using System;

namespace HB.FullStack.Common.Models
{
    public abstract class ModelDef
    {
        public ModelKind Kind { get; set; }


        /// <summary>
        /// 实体名
        /// </summary>
        public string ModelFullName { get; set; } = null!;

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type ModelType { get; set; } = null!;


        public abstract ModelPropertyDef? GetPropertyDef(string propertyName);
    }
}
