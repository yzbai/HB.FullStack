using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        /// <summary>
        /// AddOrUpdate,即override,不检查Timestamp
        /// </summary>
        public async Task AddOrUpdateByIdAsync<T>(T item, string lastUser, TransactionContext? transContext = null) where T : TimelessDbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(nameof(modelDef)).ThrowIfNotWriteable();
            ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
            //TruncateLastUser(ref lastUser);

            try
            {
                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                item.LastUser = lastUser;

                var command = DbCommandBuilder.CreateAddOrUpdateCommand(modelDef, item, false);

                _ = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(connectionString, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(item), ex);
            }
        }

        public async Task AddOrUpdateByIdAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : TimelessDbModel, new()
        {
            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!.ThrowIfNotWriteable();
            ConnectionString connectionString = _dbSchemaManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);

            ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);
            //TruncateLastUser(ref lastUser);

            foreach (var item in items)
            {
                item.LastUser = lastUser;
            }

            try
            {
                IDbEngine engine = _dbSchemaManager.GetDatabaseEngine(modelDef.EngineType);

                var command = DbCommandBuilder.CreateBatchAddOrUpdateCommand(modelDef, items, transContext == null);

                _ = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(connectionString, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                string detail = $"Items:{SerializeUtil.ToJson(items)}";
                throw DbExceptions.UnKown(modelDef.ModelFullName, detail, ex);
            }
        }
    }
}
