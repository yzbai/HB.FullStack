/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HB.FullStack.Database.DbModels;

[assembly: InternalsVisibleTo("HB.FullStack.Client")]
[assembly: InternalsVisibleTo("HB.FullStack.DatabaseTests")]
[assembly: InternalsVisibleTo("HB.FullStack.DatabaseTests.MySQL")]
[assembly: InternalsVisibleTo("HB.FullStack.DatabaseTests.SQLite")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]
[assembly: InternalsVisibleTo("HB.FullStack.Repository")]

namespace HB.FullStack.Database
{
    /// <summary>
    /// 对外提供数据库操作
    /// </summary>
    public interface IDatabase : IDbWriter, IDbReader
    {
        /// <summary>
        /// 有几个DbSchema，就初始化几次
        /// 必须加分布式锁进行。返回是否有Migration被执行
        /// </summary>
        Task<bool> InitializeAsync(string dbSchema, string? connectionString, IList<string>? slaveConnectionStrings, IEnumerable<Migration>? migrations);

        IDbModelDefFactory ModelDefFactory { get; }
    }
}