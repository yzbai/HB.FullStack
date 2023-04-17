using System;
using System.Collections.Generic;

using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.Config
{
    public interface IDbSchemaManager
    {
        DbSchema GetDbSchema(string dbSchemaName);

        void SetConnectionString(string dbSchemaName, string? connectionString, IList<string>? slaveConnectionStrings);

        ConnectionString? GetConnectionString(string dbSchemaName, bool userMaster);

        ConnectionString GetRequiredConnectionString(string dbSchemaName, bool userMaster) 
            => GetConnectionString(dbSchemaName, true).ThrowIfNull($"{dbSchemaName} Not Set ConnectionString Yet!");

        IDbEngine GetDatabaseEngine(string dbSchemaName);

        IDbEngine GetDatabaseEngine(DbEngineType engineType);
        IList<DbSchema> GetAllDbSchemas();
    }
}
