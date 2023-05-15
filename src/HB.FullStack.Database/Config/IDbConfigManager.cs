using System;
using System.Collections.Generic;

using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.Config
{
    public interface IDbConfigManager
    {
        IList<string> DbModelAssemblies { get; }

        DbSchema DefaultDbSchema { get; }

        DbSchema GetDbSchema(string dbSchemaName);

        void SetConnectionString(string dbSchemaName, string? connectionString, IList<string>? slaveConnectionStrings);

        //ConnectionString? GetConnectionString(DbSchema dbSchema, bool userMaster);

        //ConnectionString GetRequiredConnectionString(DbSchema dbSchema, bool userMaster)
        //    => GetConnectionString(dbSchema, true).ThrowIfNull($"{dbSchema.Name} Not Set ConnectionString Yet!");

        //IDbEngine GetDatabaseEngine(DbSchema dbSchema);

        //IDbEngine GetDatabaseEngine(DbEngineType engineType);

        IList<DbSchema> GetAllDbSchemas();
    }
}
