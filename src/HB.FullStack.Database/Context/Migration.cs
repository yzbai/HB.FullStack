

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Database
{

    /// <summary>
    /// 先执行SQLStatement，再执行ModifyAction
    /// </summary>
    public class Migration
    {
        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public string? SqlStatement { get; set; }

        public string DatabaseName { get; set; }

        public Func<IDatabase, TransactionContext, Task>? ModifyFunc { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="oldVersion"></param>
        /// <param name="newVersion"></param>
        /// <param name="sql"></param>
        
        public Migration(string databaseName, int oldVersion, int newVersion)
        {
            if (databaseName.IsNullOrEmpty())
            {
                throw DatabaseExceptions.MigrateError(databaseName, "databaseName is Empty");
            }

            if (oldVersion < 0)
            {
                throw DatabaseExceptions.MigrateError(databaseName, "oldVersion < 0");
            }

            if (newVersion != oldVersion + 1)
            {
                throw DatabaseExceptions.MigrateError(databaseName, "newVersion != oldVersoin + 1 ");
            }

            DatabaseName = databaseName;
            OldVersion = oldVersion;
            NewVersion = newVersion;
        }

        
        public Migration(string targetSchema, int oldVersion, int newVersion, string sql) : this(targetSchema, oldVersion, newVersion)
        {
            SqlStatement = sql;
        }

        public Migration(string targetSchema, int oldVersion, int newVersion, Func<IDatabase, TransactionContext, Task> func) : this(targetSchema, oldVersion, newVersion)
        {
            ModifyFunc = func;
        }
    }
}