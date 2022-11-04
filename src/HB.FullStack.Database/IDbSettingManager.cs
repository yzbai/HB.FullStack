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
    public interface IDbSettingManager
    {
        #region Settings

        DbSetting GetDbSetting(string dbSchema);

        int GetVarcharDefaultLength(DbSchema dbSchema);

        int GetMaxBatchNumber(DbSchema dbSchema);

        bool GetDefaultTrulyDelete(DbSchema dbSchema);

        #endregion

        #region ConnectionString

        /// <summary>
        /// 如果从Options中得不到ConnectionStrings，那么就接受这些设置.保证Options中的是最后的。
        /// </summary>
        void SetConnectionStringIfNeed(string dbSchema, string? connectionString, IList<string>? slaveConnectionStrings);

        ConnectionString GetConnectionString(DbSchema dbSchema, bool userMaster);

        #endregion

        #region Engine

        IDatabaseEngine GetDatabaseEngine(DbSchema dbSchema);

        IDatabaseEngine GetDatabaseEngine(EngineType engineType);

        #endregion
    }
}
