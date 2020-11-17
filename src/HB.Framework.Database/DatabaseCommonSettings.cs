#nullable enable

using System.Collections.Generic;

namespace HB.Framework.Database
{
    public class DatabaseCommonSettings
    {
        public int Version { get; set; }

        public int DefaultVarcharLength { get; set; } = 200;

        public bool AutomaticCreateTable { get; set; } = true;

        public IList<string> AssembliesIncludeEntity { get; private set; } = new List<string>();

        public IList<EntityInfo> Entities { get; private set; } = new List<EntityInfo>();
    }
}
