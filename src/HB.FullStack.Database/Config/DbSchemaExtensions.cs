using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.FullStack.Database.Config
{
    public static class DbSchemaExtensions
    {
        private static readonly Random _slaveConnectionRandom = new Random();

        public static void SetConnectionString(this DbSchema schema, string? connectionString, IList<string>? slaveConnectionStrings)
        {
            if (connectionString.IsNotNullOrEmpty())
            {
                schema.ConnectionString = new ConnectionString(connectionString.ThrowIfNullOrEmpty($"在初始化时，应该为 {schema.Name} 提供连接字符串"));
            }

            if (slaveConnectionStrings != null)
            {
                schema.SlaveConnectionStrings = slaveConnectionStrings.Select(c => new ConnectionString(c)).ToList();
            }
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
