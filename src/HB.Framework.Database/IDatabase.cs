using HB.Framework.Common.Entities;
using HB.Framework.Database.Engine;
using HB.Framework.Database.Entities;
using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HB.Framework.Database
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