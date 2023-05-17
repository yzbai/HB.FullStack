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
        public async Task AddOrUpdateByIdAsync<T>(T item, string lastUser, TransactionContext? transContext = null) where T : BaseDbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(nameof(modelDef)).ThrowIfNotWriteable();

            try
            {
                item.LastUser = lastUser;

                var command = DbCommandBuilder.CreateAddOrUpdateCommand(modelDef, item, false);

                _ = transContext != null
                    ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                throw DbExceptions.UnKown(modelDef.FullName, SerializeUtil.ToJson(item), ex);
            }
        }

        public async Task AddOrUpdateByIdAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : BaseDbModel, new()
        {
            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(nameof(modelDef)).ThrowIfNotWriteable();

            ThrowIfExceedMaxBatchNumber(items, lastUser, modelDef);

            foreach (var item in items)
            {
                item.LastUser = lastUser;
            }

            try
            {
                var command = DbCommandBuilder.CreateBatchAddOrUpdateCommand(modelDef, items);

                _ = transContext != null
                    ? await modelDef.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DbException)
            {
                string detail = $"Items:{SerializeUtil.ToJson(items)}";
                throw DbExceptions.UnKown(modelDef.FullName, detail, ex);
            }
        }
    }
}
