using HB.Framework.Database.Engine;
using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Database.SQL
{
    internal partial class SQLBuilder
    {
        public static string CreateAddTemplate(DatabaseEntityDef definition, DatabaseEngineType engineType)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder selectArgs = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in definition.Properties)
            {
                if (info.IsTableProperty)
                {
                    selectArgs.AppendFormat(GlobalSettings.Culture, "{0},", info.DbReservedName);

                    if (info.IsAutoIncrementPrimaryKey || info.PropertyName == "LastTime")
                    {
                        continue;
                    }

                    args.AppendFormat(GlobalSettings.Culture, "{0},", info.DbReservedName);

                    values.AppendFormat(GlobalSettings.Culture, " {0},", info.DbParameterizedName);

                }
            }

            if (selectArgs.Length > 0)
            {
                selectArgs.Remove(selectArgs.Length - 1, 1);
            }

            if (args.Length > 0)
            {
                args.Remove(args.Length - 1, 1);
            }

            if (values.Length > 0)
            {
                values.Remove(values.Length - 1, 1);
            }

            DatabaseEntityPropertyDef idProperty = definition.GetProperty("Id");

            return $"insert into {definition.DbTableReservedName}({args.ToString()}) values({values.ToString()});select {selectArgs.ToString()} from {definition.DbTableReservedName} where {idProperty.DbReservedName} = {GetLastInsertIdStatement(engineType)};";
        }

        public static string CreateUpdateTemplate(DatabaseEntityDef modelDef)
        {
            StringBuilder args = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsAutoIncrementPrimaryKey || info.PropertyName == "LastTime" || info.PropertyName == "Deleted")
                    {
                        continue;
                    }

                    args.AppendFormat(GlobalSettings.Culture, " {0}={1},", info.DbReservedName, info.DbParameterizedName);
                }
            }

            if (args.Length > 0)
            {
                args.Remove(args.Length - 1, 1);
            }

            string statement = string.Format(GlobalSettings.Culture, "UPDATE {0} SET {1}", modelDef.DbTableReservedName, args.ToString());

            return statement;
        }

        public static string CreateDeleteTemplate(DatabaseEntityDef modelDef)
        {
            DatabaseEntityPropertyDef deletedProperty = modelDef.GetProperty("Deleted");
            DatabaseEntityPropertyDef lastUserProperty = modelDef.GetProperty("LastUser");

            StringBuilder args = new StringBuilder();

            args.Append($"{deletedProperty.DbReservedName}=1,");
            args.Append($"{lastUserProperty.DbReservedName}={lastUserProperty.DbParameterizedName}");

            return $"UPDATE {modelDef.DbTableReservedName} SET {args.ToString()} ";
        }

        public static string TempTable_Insert(string tempTableName, string value, DatabaseEngineType databaseEngineType)
        {
            switch (databaseEngineType)
            {
                case DatabaseEngineType.MySQL:
                    return $"insert into `{tempTableName}`(`id`) values({value});";
                case DatabaseEngineType.SQLite:
                    return $"insert into temp.{tempTableName}(\"id\") values({value});";
                case DatabaseEngineType.MSSQLSERVER:
                default:
                    return "";
            }
        }

        public static string TempTable_Select_All(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            switch (databaseEngineType)
            {
                case DatabaseEngineType.MySQL:
                    return $"select `id` from `{tempTableName}`;";
                case DatabaseEngineType.SQLite:
                    return $"select id from temp.{tempTableName};";
                case DatabaseEngineType.MSSQLSERVER:
                default:
                    return "";
            }
        }

        public static string TempTable_Drop(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            switch (databaseEngineType)
            {
                case DatabaseEngineType.MySQL:
                    return $"drop temporary table if exists `{tempTableName}`;";
                case DatabaseEngineType.SQLite:
                    return $"drop table if EXISTS temp.{tempTableName};";
                case DatabaseEngineType.MSSQLSERVER:
                default:
                    return "";
            }
        }

        public static string TempTable_Create(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            switch (databaseEngineType)
            {
                case DatabaseEngineType.MySQL:
                    return $"create temporary table `{tempTableName}` ( `id` int not null);";
                case DatabaseEngineType.SQLite:
                    return $"create temporary table {tempTableName} (\"id\" integer not null);";
                case DatabaseEngineType.MSSQLSERVER:
                default:
                    return "";
            }
        }

        public static string FoundChanges_Statement(DatabaseEngineType databaseEngineType)
        {
            switch (databaseEngineType)
            {
                case DatabaseEngineType.MySQL:
                    return $" found_rows() ";
                case DatabaseEngineType.SQLite:
                    return $" changes() ";
                case DatabaseEngineType.MSSQLSERVER:
                default:
                    return "";
            }
        }

        public static string GetLastInsertIdStatement(DatabaseEngineType databaseEngineType)
        {
            switch (databaseEngineType)
            {
                case DatabaseEngineType.SQLite:
                    return "last_insert_rowid()";
                case DatabaseEngineType.MySQL:
                    return "last_insert_id()";
                case DatabaseEngineType.MSSQLSERVER:
                default:
                    return "";
            }
        }
    }
}
