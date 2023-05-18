/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Threading.Tasks;

using HB.FullStack.Cache;

namespace HB.FullStack.Database.Config
{

    /// <summary>
    /// 一个Migration代表一次升级的记录
    /// 先执行SQLStatement，再执行ModifyAction
    /// OldVersion = 0 并且 NewVersion = 1 为初始化数据
    /// </summary>
    public class Migration
    {
        public string DbSchemaName { get; set; }
        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public string? SqlStatement { get; set; }

        public Func<IDatabase, TransactionContext, Task>? ModifyFunc { get; private set; }

        //TODO: 架构合理性考虑,是否开放事件出来
        //TODO: clear the cache
        //清理比如xxx开头的CacheItem,要求Cache有统一开头，且不能与KVStore冲突。所以KVStore最好与cache是不同的实例
        public Func<Task>? CacheCleanTask { get; set; }

        public Migration(string dbSchemaName, int oldVersion, int newVersion, string? sql,
            Func<IDatabase, TransactionContext, Task>? func, Func<Task>? cacheCleanTask)
        {
            if (oldVersion < 0)
            {
                throw DbExceptions.MigrateError(dbSchemaName, "oldVersion < 0");
            }

            if (newVersion != oldVersion + 1)
            {
                throw DbExceptions.MigrateError(dbSchemaName, "newVersion != oldVersoin + 1 ");
            }

            DbSchemaName = dbSchemaName;
            OldVersion = oldVersion;
            NewVersion = newVersion;
            SqlStatement = sql;
            ModifyFunc = func;
            CacheCleanTask = cacheCleanTask;
        }
    }
}