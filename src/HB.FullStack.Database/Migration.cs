

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database
{

    /// <summary>
    /// 一个Migration代表一次升级的记录
    /// 先执行SQLStatement，再执行ModifyAction
    /// OldVersion = 0 并且 NewVersion = 1 初始化数据
    /// </summary>
    public class Migration
    {
        public string DbSchema { get; set; }

        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public string? SqlStatement { get; set; }

        public Func<IDatabase, TransactionContext, Task>? ModifyFunc { get; private set; }

        public Migration(string dbSchema, int oldVersion, int newVersion, string? sql = null, Func<IDatabase, TransactionContext, Task>? func = null)
        {
            if (oldVersion < 0)
            {
                throw DatabaseExceptions.MigrateError(dbSchema, "oldVersion < 0");
            }

            if (newVersion != oldVersion + 1)
            {
                throw DatabaseExceptions.MigrateError(dbSchema, "newVersion != oldVersoin + 1 ");
            }

            DbSchema = dbSchema;
            OldVersion = oldVersion;
            NewVersion = newVersion;
            SqlStatement = sql;
            ModifyFunc = func;
        }
    }
}