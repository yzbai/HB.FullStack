using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

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

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.NotWriteable(modelDef.ModelFullName, modelDef.DatabaseName);
            }

            //long oldTimestamp = -1;
            //string oldLastUser = "";

            try
            {
                ////Prepare
                //if (item is TimestampDbModel timestampDBModel)
                //{
                //    oldTimestamp = timestampDBModel.Timestamp;
                //    oldLastUser = timestampDBModel.LastUser;

                //    timestampDBModel.Timestamp = TimeUtil.UtcNowTicks;
                //    timestampDBModel.LastUser = lastUser;
                //}

                item.LastUser = lastUser;

                var command = DbCommandBuilder.CreateAddOrUpdateCommand(EngineType, modelDef, item, false);

                _ = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(item), ex);
            }
        }

        public async Task BatchAddOrUpdateByIdAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : TimelessDbModel, new()
        {
            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            if (!modelDef.DatabaseWriteable)
            {
                throw DatabaseExceptions.DatabaseNotWritable(modelDef.ModelFullName, SerializeUtil.ToJson(items));
            }

            foreach (var item in items)
            {
                item.LastUser = lastUser;
            }

            try
            {
                var command = DbCommandBuilder.CreateBatchAddOrUpdateCommand(EngineType, modelDef, items, transContext == null);

                _ = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName, command).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                string detail = $"Items:{SerializeUtil.ToJson(items)}";
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, detail, ex);
            }
        }
    }
}
