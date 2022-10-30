

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Database
{

    /// <summary>
    /// 一个Migration代表一次升级的记录
    /// 先执行SQLStatement，再执行ModifyAction
    /// </summary>
    public class Migration
    {
        public string? DbName { get; set; }
        public string? DbKind { get; set; }
        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public string? SqlStatement { get; set; }

        public Func<IDatabase, TransactionContext, Task>? ModifyFunc { get; private set; }

        public Migration(string? dbName, string? dbKind, int oldVersion, int newVersion, string? sql = null, Func<IDatabase, TransactionContext, Task>? func = null)
        {
            if (dbName.IsNullOrEmpty() && dbKind.IsNullOrEmpty())
            {
                throw DatabaseExceptions.MigrateError(dbName, "DbName 和 DbKind不能同时为空");
            }

            if (oldVersion < 0)
            {
                throw DatabaseExceptions.MigrateError(dbName ?? dbKind, "oldVersion < 0");
            }

            if (newVersion != oldVersion + 1)
            {
                throw DatabaseExceptions.MigrateError(dbName ?? dbKind, "newVersion != oldVersoin + 1 ");
            }

            DbName = dbName;
            DbKind = dbKind;
            OldVersion = oldVersion;
            NewVersion = newVersion;
            SqlStatement = sql;
            ModifyFunc = func;
        }
    }
}