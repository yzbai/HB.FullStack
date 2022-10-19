
using System.Collections.Generic;

namespace HB.FullStack.Database
{
    public class DbCommonSettings
    {
        /// <summary>
        /// 初始数据库从1开始
        /// </summary>
        public int Version { get; set; }

        public int DefaultVarcharLength { get; set; } = 200;

        public bool AutomaticCreateTable { get; set; } = true;

        /// <summary>
        /// 指定包含Model的Assemblies，不再包含其他的
        /// </summary>
        public IList<string> Assemblies { get; set; } = new List<string>();

        public IList<DbModelSetting> DbModelSettings { get; set; } = new List<DbModelSetting>();

        public int MaxBatchNumber { get; set; } = 500;

        public bool AddDropStatementWhenCreateTable { get; set; }

        /// <summary>
        /// 默认的是否真正删除
        /// </summary>
        public bool DefaultTrulyDelete { get; set; } = false;
    }
}