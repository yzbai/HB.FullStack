using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Framework.Database
{
    public class EntitySchema
    {
        //public string Assembly { get; set; }
        public string EntityTypeFullName { get; set; }
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string Description { get; set; }
        public bool Writeable { get; set; }
    }

    public interface IDatabaseSettings
    {
        int Version { get; }

        int DefaultVarcharLength { get; }

        IList<EntitySchema> Entities { get;}

    }
}
