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

        public IList<string> AssembliesIncludeEntity { get; set; } = new List<string>();

        public IList<EntitySetting> EntitySettings { get; set; } = new List<EntitySetting>();
    }
}