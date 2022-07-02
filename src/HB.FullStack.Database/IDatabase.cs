/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("HB.FullStack.Client")]
[assembly: InternalsVisibleTo("HB.FullStack.DatabaseTests")]
[assembly: InternalsVisibleTo("HB.FullStack.DatabaseTests.MySQL")]
[assembly: InternalsVisibleTo("HB.FullStack.DatabaseTests.SQLite")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]

namespace HB.FullStack.Database
{
    public interface IDatabase : IDatabaseWriter, IDatabaseReader
    {
        /// <summary>
        /// 必须加分布式锁进行。返回是否有Migration被执行
        /// </summary>
        Task<bool> InitializeAsync(IEnumerable<Migration>? migrations = null);

        EngineType EngineType { get; }

        IEnumerable<string> DatabaseNames { get; }

        public int VarcharDefaultLength { get; }

        internal IDatabaseEngine DatabaseEngine { get; }

        internal IEntityDefFactory EntityDefFactory { get; }

        internal IDbCommandBuilder DbCommandBuilder { get; }
    }
}