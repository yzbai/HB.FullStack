using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        DatabaseEngineType EngineType { get; }

    }
}