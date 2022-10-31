using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Database
{
    /// <summary>
    /// DBA
    /// 管理数据库本身，ConnectionString，Engine，Options等等
    /// </summary>
    public interface IDbManager
    {
        #region Settings

        int GetVarcharDefaultLength(DbSchema dbSchema);
        int GetMaxBatchNumber(DbSchema dbSchema);
        bool GetDefaultTrulyDelete(DbSchema dbSchema);

        #endregion

        #region ConnectionString

        ConnectionString GetConnectionString(DbSchema dbSchema, bool userMaster);

        #endregion

        #region Engine

        IDatabaseEngine GetDatabaseEngine(DbSchema dbSchema);

        IDatabaseEngine GetDatabaseEngine(EngineType engineType);

        #endregion


        Task<bool> InitializeAsync(DbSchema dbSchema, string? connectionString, IList<string>? slaveConnectionStrings, IEnumerable<Migration>? migrations);
    }
}
