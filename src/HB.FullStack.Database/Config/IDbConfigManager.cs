using System;
using System.Collections.Generic;

using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.Config
{
    public interface IDbConfigManager
    {
        IList<string> DbModelAssemblies { get; }

        DbSchema DefaultDbSchema { get; }

        IList<DbSchema> AllDbSchemas { get; }
        IEnumerable<DbInitContext>? InitContexts { get; }

        IDbEngine GetDbEngine(DbEngineType engineType);

        DbSchema GetDbSchema(string dbSchemaName);
    }
}
