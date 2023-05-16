using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string TempTable_Insert_Id(string tempTableName, string value, DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DbEngineType.MySQL => $"insert into `{tempTableName}`(`id`) values({value});",
                DbEngineType.SQLite => $"insert into temp.{tempTableName}(\"id\") values({value});",
                _ => throw new NotSupportedException()
            };
        }

        public static string TempTable_Select_Id(string tempTableName, DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DbEngineType.MySQL => $"select `id` from `{tempTableName}`;",
                DbEngineType.SQLite => $"select id from temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Drop(string tempTableName, DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DbEngineType.MySQL => $"drop temporary table if exists `{tempTableName}`;",
                DbEngineType.SQLite => $"drop table if EXISTS temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Create_Id(string tempTableName, DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DbEngineType.MySQL => $"create temporary table `{tempTableName}` ( `id` int not null);",
                DbEngineType.SQLite => $"create temporary table temp.{tempTableName} (\"id\" integer not null);",
                _ => "",
            };
        }

        //TODO: sqlite，如果在Batch语句里加上Begin，那么会引起SQLite Error 1: 'cannot start a transaction within a transaction'.
        //为什么？
        public static string Transaction_Begin(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => "Begin;",
                //DbEngineType.SQLite => "Begin;",
                DbEngineType.SQLite => "",
                _ => throw new NotImplementedException(),
            };
        }

        public static string Transaction_Commit(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => "Commit;",
                //DbEngineType.SQLite => "Commit;",
                DbEngineType.SQLite => "",
                _ => throw new NotImplementedException(),
            };
        }

        public static string Transaction_Rollback(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => "Rollback;",
                //DbEngineType.SQLite => "Rollback;",
                DbEngineType.SQLite => "",
                _ => throw new NotImplementedException(),
            };
        }

        public static string FoundUpdateMatchedRows_Statement(DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                //found_rows()返回匹配到的
                //row_count() 返回真正被修改的

                DbEngineType.MySQL => " found_rows() ",//$"row_count()", // $" found_rows() ",
                DbEngineType.SQLite => " changes() ",//sqlite不返回真正受影响的，只返回匹配的
                _ => throw new NotImplementedException(),
            };
        }

        public static string FoundDeletedRows_Statement(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => " row_count() ",
                DbEngineType.SQLite => " changes() ",
                _ => throw new NotImplementedException(),
            };
        }

        public static string LastInsertIdStatement(DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DbEngineType.SQLite => "last_insert_rowid()",
                DbEngineType.MySQL => "last_insert_id()",
                _ => "",
            };
        }

        public static string GetOrderBySqlUtilInStatement(string quotedColName, string[] ins, DbEngineType databaseEngineType)
        {
            if (databaseEngineType == DbEngineType.MySQL)
            {
                return $" ORDER BY FIELD({quotedColName}, {ins.ToJoinedString(",")}) ";
            }
            else if (databaseEngineType == DbEngineType.SQLite)
            {
                StringBuilder orderCaseBuilder = new StringBuilder(" ORDER BY CASE ");

                orderCaseBuilder.Append(quotedColName);

                for (int i = 0; i < ins.Length; ++i)
                {
                    orderCaseBuilder.Append($" when {ins[i]} THEN {i} ");
                }

                orderCaseBuilder.Append(" END ");

                return orderCaseBuilder.ToString();
            }

            throw new NotSupportedException();
        }

        public static string OnDuplicateKeyUpdateStatement(DbEngineType engineType, DbModelPropertyDef primaryDef)
        {
            return engineType switch
            {
                DbEngineType.MySQL => "on duplicate key update",
                DbEngineType.SQLite => $"on conflict({primaryDef.DbReservedName}) do update set",
                _ => throw new NotSupportedException()
            };
        }
    }
}
