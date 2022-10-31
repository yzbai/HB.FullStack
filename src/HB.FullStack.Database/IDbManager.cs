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

        int GetVarcharDefaultLength(DbModelDef modelDef);
        int GetMaxBatchNumber(DbModelDef modelDef);
        bool GetDefaultTrulyDelete(DbModelDef modelDef);

        #endregion

        #region ConnectionString

        ConnectionString GetConnectionStringByDbName(string dbName, bool userMaster);
        ConnectionString GetConnectionString(DbModelDef modelDef, bool userMaster);

        #endregion

        #region Engine

        IDatabaseEngine GetDatabaseEngineByDbName(string databaseName);

        IDatabaseEngine GetDatabaseEngine(DbModelDef modelDef);

        #endregion


        Task<bool> InitializeByDbNameAsync(string dbName, IEnumerable<Migration>? migrations);

        Task<bool> InitializeByDbKindAsync(string dbKind, IEnumerable<Migration>? migrations);
    }
}
