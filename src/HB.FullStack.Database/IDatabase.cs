using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using HB.FullStack.Database.Engine;

[assembly:InternalsVisibleTo("HB.FullStack.Client")]
[assembly: InternalsVisibleTo("HB.FullStack.DatabaseTests")]
[assembly: InternalsVisibleTo("HB.FullStack.DatabaseTests.MySQL")]
[assembly: InternalsVisibleTo("HB.FullStack.DatabaseTests.SQLite")]

namespace HB.FullStack.Database
{
    public interface IDatabase : IDatabaseWriter, IDatabaseReader
    {
        /// <summary>
        /// 必须加分布式锁进行。
        /// </summary>
        /// <param name="migrations"></param>
        /// <returns></returns>

        Task InitializeAsync(IEnumerable<Migration>? migrations = null);

        EngineType EngineType { get; }

        IEnumerable<string> DatabaseNames { get; }

        internal IDatabaseEngine DatabaseEngine { get; }
    }
}