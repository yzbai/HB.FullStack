#nullable enable

using System.Collections.Generic;

namespace HB.FullStack.Database
{
    public class DatabaseCommonSettings
    {
        public int Version { get; set; }

        public int DefaultVarcharLength { get; set; } = 200;

        public bool AutomaticCreateTable { get; set; } = true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        public IList<string> AssembliesIncludeEntity { get; set; } = new List<string>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        public IList<EntitySetting> EntitySettings { get; set; } = new List<EntitySetting>();
    }
}