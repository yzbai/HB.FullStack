#nullable enable

using HB.Framework.Database.Engine;
using HB.Framework.Database.Entities;
using System;
using System.Text;

namespace HB.Framework.Database.SQL
{
    internal partial class SQLBuilder
    {
        public static string CreateAddTemplate(DatabaseEntityDef modelDef, DatabaseEngineType engineType)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder selectArgs = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    selectArgs.AppendFormat(GlobalSettings.Culture, "{0},", info.DbReservedName);

                    if (info.IsAutoIncrementPrimaryKey)
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

            DatabaseEntityPropertyDef idProperty = modelDef.GetProperty("Id")!;

            return $"insert into {modelDef.DbTableReservedName}({args}) values({values});select {selectArgs} from {modelDef.DbTableReservedName} where {idProperty.DbReservedName} = {GetLastInsertIdStatement(engineType)};";
        }

        public static string CreateAddOrUpdateTemplate(DatabaseEntityDef modelDef, DatabaseEngineType engineType)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder selectArgs = new StringBuilder();
            StringBuilder values = new StringBuilder();
            StringBuilder exceptGuidAndFixedVersionUpdatePairs = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    selectArgs.AppendFormat(GlobalSettings.Culture, "{0},", info.DbReservedName);

                    if (info.IsAutoIncrementPrimaryKey)
                    {
                        continue;
                    }

                    args.AppendFormat(GlobalSettings.Culture, "{0},", info.DbReservedName);

                    values.AppendFormat(GlobalSettings.Culture, " {0},", info.DbParameterizedName);

                }
            }

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsAutoIncrementPrimaryKey || info.PropertyInfo.Name == "Version" || info.PropertyInfo.Name == "Guid" || info.PropertyInfo.Name == "Deleted")
                    {
                        continue;
                    }

                    exceptGuidAndFixedVersionUpdatePairs.Append($" {info.DbReservedName}={info.DbParameterizedName},");
                }
            }

            DatabaseEntityPropertyDef versionPropertyDef = modelDef.GetProperty("Version")!;

            exceptGuidAndFixedVersionUpdatePairs.Append($" {versionPropertyDef.DbReservedName}={versionPropertyDef.DbReservedName}+1,");

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

            if (exceptGuidAndFixedVersionUpdatePairs.Length > 0)
            {
                exceptGuidAndFixedVersionUpdatePairs.Remove(exceptGuidAndFixedVersionUpdatePairs.Length - 1, 1);
            }

            DatabaseEntityPropertyDef guidProperty = modelDef.GetProperty("Guid")!;

            return $"insert into {modelDef.DbTableReservedName}({args}) values({values}) {OnDuplicateKeyUpdateStatement(engineType)} {exceptGuidAndFixedVersionUpdatePairs};select {selectArgs} from {modelDef.DbTableReservedName} where {guidProperty.DbReservedName} = {guidProperty.DbParameterizedName};";
        }

        public static string CreateUpdateTemplate(DatabaseEntityDef modelDef)
        {
            StringBuilder args = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsAutoIncrementPrimaryKey || info.PropertyInfo.Name == "Deleted" || info.PropertyInfo.Name == "Guid")
                    {
                        continue;
                    }

                    args.Append($" {info.DbReservedName}={info.DbParameterizedName},");
                }
            }

            if (args.Length > 0)
            {
                args.Remove(args.Length - 1, 1);
            }

            return $"UPDATE {modelDef.DbTableReservedName} SET {args}";
        }

        public static string CreateDeleteTemplate(DatabaseEntityDef modelDef)
        {
            DatabaseEntityPropertyDef deletedProperty = modelDef.GetProperty("Deleted")!;
            DatabaseEntityPropertyDef lastUserProperty = modelDef.GetProperty("LastUser")!;
            DatabaseEntityPropertyDef lastTimeProperty = modelDef.GetProperty("LastTime")!;

            StringBuilder args = new StringBuilder();

            args.Append($"{deletedProperty.DbReservedName}=1,");
            args.Append($"{lastUserProperty.DbReservedName}={lastUserProperty.DbParameterizedName},");
            args.Append($"{lastTimeProperty.DbReservedName}={lastTimeProperty.DbParameterizedName}");

            return $"UPDATE {modelDef.DbTableReservedName} SET {args} ";
        }

        public static string TempTable_Insert(string tempTableName, string value, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"insert into `{tempTableName}`(`id`) values({value});",
                DatabaseEngineType.SQLite => $"insert into temp.{tempTableName}(\"id\") values({value});",
                _ => "",
            };
        }

        public static string TempTable_Insert_Select(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"insert into `{tempTableName}`(`id`,`version`) ",
                DatabaseEngineType.SQLite => $"insert into temp.{tempTableName}(\"id\",\"version\") ",
                _ => "",
            };
        }

        public static string TempTable_Select_All(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"select `id` from `{tempTableName}`;",
                DatabaseEngineType.SQLite => $"select id from temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Select_IdAndVersion(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"select `id`,`version` from `{tempTableName}`;",
                DatabaseEngineType.SQLite => $"select id, version from temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Drop(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"drop temporary table if exists `{tempTableName}`;",
                DatabaseEngineType.SQLite => $"drop table if EXISTS temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Create(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"create temporary table `{tempTableName}` ( `id` int not null);",
                DatabaseEngineType.SQLite => $"create temporary table temp.{tempTableName} (\"id\" integer not null);",
                _ => "",
            };
        }

        public static string TempTable_Create_IdAndVersion(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"create temporary table `{tempTableName}` ( `id` bigint not null, `version` int not null);",
                DatabaseEngineType.SQLite => $"create temporary table temp.{tempTableName} (\"id\" integer not null,\"version\" integer not null);",
                _ => "",
            };
        }

        public static string FoundChanges_Statement(DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                //found_rows()返回的查询语句的结果
                //row_count表示修改时找到的条数
                //默认下UserAffectRows=false，两者相同。当UserAffectedRow=true时，row_count()只会返回真正修改的行数，找到但值相同没有修改的不算
                DatabaseEngineType.MySQL => $"row_count()", // $" found_rows() ",
                DatabaseEngineType.SQLite => $" changes() ",
                _ => "",
            };
        }

        public static string GetLastInsertIdStatement(DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.SQLite => "last_insert_rowid()",
                DatabaseEngineType.MySQL => "last_insert_id()",
                _ => "",
            };
        }

        public static string OnDuplicateKeyUpdateStatement(DatabaseEngineType engineType)
        {
            return engineType switch
            {
                DatabaseEngineType.MySQL => "on duplicate key update",
                DatabaseEngineType.SQLite => "on conflict(`Guid`) do update set",
                _ => ""
            };
        }
    }
}
