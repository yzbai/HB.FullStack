using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Database.Config
{
    public class DbFieldSchema
    {

        //public int? PropertyOrder { get; set; }

        public string FieldName { get; set; } = null!;

        public bool? FixedLength { get; set; }

        /// <summary>
        /// 字段长度
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// 字段是否可空
        /// </summary>
        public bool? NotNull { get; set; }

        public bool? NeedIndex { get; set; }

        /// <summary>
        /// 字段值是否唯一
        /// </summary>
        public bool? Unique { get; set; }

    }
}
