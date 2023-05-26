using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.Config
{
    public static class DbSchemaExtensions
    {
        private static readonly Random _slaveConnectionRandom = new Random();

        public static void EnsureUseAffectedRowsIsFalse(this DbSchema schema)
        {
            if (schema.EngineType != DbEngineType.MySQL)
            {
                return;
            }

            schema.ConnectionString = EnsureConnectionString(schema.ConnectionString);

            if (schema.SlaveConnectionStrings != null)
            {
                for (int i = 0; i < schema.SlaveConnectionStrings.Count; ++i)
                {
                    schema.SlaveConnectionStrings[i] = EnsureConnectionString(schema.SlaveConnectionStrings[i]);
                }
            }

            [return: NotNullIfNotNull(nameof(connectionString))]
            static ConnectionString? EnsureConnectionString(ConnectionString? connectionString)
            {
                if (connectionString == null)
                {
                    return null;
                }

                DbConnectionStringBuilder connectionBuilder = new DbConnectionStringBuilder();
                connectionBuilder.ConnectionString = connectionString.ToString();

                if (connectionBuilder.TryGetValue("UseAffectedRows", out object? oldUseAffectedRows))
                {
                    if (System.Convert.ToBoolean(oldUseAffectedRows))
                    {
                        throw DbExceptions.DbSchemaError(null, $"Should Set UseAffectedRows=false.");
                    }
                }

                connectionBuilder["UseAffectedRows"] = false;

                return new ConnectionString(connectionBuilder.ConnectionString);
            }
        }

        public static void SetConnectionString(this DbSchema schema, string? connectionString, IList<string>? slaveConnectionStrings)
        {
            if (connectionString.IsNotNullOrEmpty())
            {
                schema.ConnectionString = new ConnectionString(connectionString);
            }

            if (slaveConnectionStrings != null)
            {
                schema.SlaveConnectionStrings = slaveConnectionStrings.Select(c => new ConnectionString(c)).ToList();
            }

            schema.EnsureUseAffectedRowsIsFalse();
        }

        public static ConnectionString GetSlaveConnectionString(this DbSchema dbSchema)
        {
            if (dbSchema.SlaveConnectionStrings.IsNullOrEmpty())
            {
                return dbSchema.ConnectionString.ThrowIfNull($"{dbSchema.Name} do not has master connection string.");
            }
            else
            {
                return dbSchema.SlaveConnectionStrings[_slaveConnectionRandom.Next() % dbSchema.SlaveConnectionStrings.Count];
            }

            //if (useMaster)
            //{
            //    return dbSchema.ConnectionString;
            //}

            //DbSchemaEx unit = _dbSchemaExDict[dbSchema.Name];

            //return GetSlaveConnectionString(unit);

            //static ConnectionString? GetSlaveConnectionString(DbSchemaEx dbUnit)
            //{
            //    //这里采取平均轮训的方法
            //    if (dbUnit.SlaveCount == 0)
            //    {
            //        return dbUnit.Schema.ConnectionString;
            //    }

            //    return dbUnit.Schema.SlaveConnectionStrings![dbUnit.SlaveAccessCount++ % dbUnit.SlaveCount];
            //}
        }

        public static ConnectionString GetMasterConnectionString(this DbSchema dbSchema)
        {
            return dbSchema.ConnectionString.ThrowIfNull($"{dbSchema.Name} do not has master connection string.");
        }
    }
}
