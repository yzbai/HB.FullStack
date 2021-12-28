#nullable enable

using System.Collections.Generic;

namespace HB.FullStack.Database
{
    public class DatabaseCommonSettings
    {
        /// <summary>
        /// 初始数据库从1开始
        /// </summary>
        public int Version { get; set; }

        public int DefaultVarcharLength { get; set; } = 200;

        public bool AutomaticCreateTable { get; set; } = true;

        /// <summary>
        /// 指定包含Entity的Assemblies，不再包含其他的
        /// </summary>
        public IList<string> Assemblies { get; set; } = new List<string>();

        public IList<EntitySetting> EntitySettings { get; set; } = new List<EntitySetting>();

        public int MaxBatchNumber { get; set; } = 500;

        public bool AddDropStatementWhenCreateTable { get; set; }
    }
}