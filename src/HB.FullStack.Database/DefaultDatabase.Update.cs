using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Extensions;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        public async Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser);

            long oldTimestamp = -1;
            string oldLastUser = "";

            try
            {
                //Prepare
                if (item is TimestampDbModel timestampDBModel)
                {
                    oldTimestamp = timestampDBModel.Timestamp;
                    oldLastUser = timestampDBModel.LastUser;

                    timestampDBModel.Timestamp = TimeUtil.Timestamp;
                    timestampDBModel.LastUser = lastUser;
                }

                EngineCommand command = DbCommandBuilder.CreateUpdateCommand(EngineType, modelDef, item, oldTimestamp);

                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    //TODO: 这里返回0，一般是因为version不匹配，单也有可能是Id不存在，或者Deleted=1.
                    //可以改造SqlHelper中的update语句为如下，进行一般改造，排除上述可能。
                    //在原始的update语句，比如：update tb_userdirectorypermission set LastUser='TTTgdTTTEEST' where Id = uuid_to_bin('08da5b35-b123-2d4f-876c-6ee360db28c1') and Deleted = 0 and Version='0';
                    //后面select found_rows(), count(1) as 'exits' from tb_userdirectorypermission where Id = uuid_to_bin('08da5b35-b123-2d4f-876c-6ee360db28c1') and Deleted = 0;
                    //然后使用Reader读取，通过两个值进行判断。
                    //如果found_rows=1，正常返回
                    //如果found_rows=0, exists = 1, version冲突
                    //如果found_rows=0, exists = 0, 已删除

                    //不存在和Version冲突，统称为冲突，所以不用改，反正后续业务为了解决冲突也会重新select什么的，到时候可以判定是已经删掉了还是version冲突

                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(item), "");
                }

                throw DatabaseExceptions.FoundTooMuch(modelDef.ModelFullName, SerializeUtil.ToJson(item));
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ComeFromEngine)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreItem(item, oldTimestamp, oldLastUser);
                }

                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(item), ex);
            }

            static void RestoreItem(T item, long oldTimestamp, string oldLastUser)
            {
                if (item is TimestampDbModel timestampDBModel)
                {
                    timestampDBModel.Timestamp = oldTimestamp;
                    timestampDBModel.LastUser = oldLastUser;
                }
            }
        }

        /// <summary>
        /// 批量更改，反应Version变化
        /// </summary>
        public async Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            if (_databaseEngine.DatabaseSettings.MaxBatchNumber < items.Count())
            {
                throw DatabaseExceptions.TooManyForBatch("BatchUpdate超过批量操作的最大数目", items.Count(), lastUser);
            }

            ThrowIf.NotValid(items, nameof(items));

            if (!items.Any())
            {
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                var command = DbCommandBuilder.CreateBatchUpdateCommand(EngineType, modelDef, items, oldTimestamps, transContext == null);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transContext?.Transaction,
                    modelDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(items), "BatchUpdate");
                    }

                    count++;
                }

                if (count != items.Count())
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(items), "BatchUpdate");
                }
            }
            catch (DatabaseException ex)
            {
                if (transContext != null || ex.ComeFromEngine)
                {
                    RestoreBatchItems(items, oldTimestamps, oldLastUsers, modelDef);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (transContext != null)
                {
                    RestoreBatchItems(items, oldTimestamps, oldLastUsers, modelDef);
                }

                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(items), ex);
            }
        }

        public async Task UpdatePropertiesAsync<T>(
            object id,
            IList<(string propertyName, object? propertyValue)> propertyNameValues,
            long timestamp,
            string lastUser,
            TransactionContext? transContext) where T : TimestampDbModel, new()
        {
            if (propertyNameValues.Count <= 0)
            {
                return;
            }

            if (id is long longId && longId <= 0)
            {
                throw DatabaseExceptions.LongIdShouldBePositive(longId);
            }

            if (id is Guid guid && guid.IsEmpty())
            {
                throw DatabaseExceptions.GuidShouldNotEmpty();
            }

            if (timestamp <= 0)
            {
                throw DatabaseExceptions.TimestampShouldBePositive(timestamp);
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser);

            try
            {
                EngineCommand command = DbCommandBuilder.CreateUpdatePropertiesCommand(
                    engineType: EngineType,
                    modelDef: modelDef,
                    id: id,
                    propertyNames: propertyNameValues.Select(t => t.propertyName).ToList(),
                    propertyValues: propertyNameValues.Select(t => t.propertyValue).ToList(),
                    oldTimestamp: timestamp,
                    newTimestamp: TimeUtil.Timestamp,
                    lastUser: lastUser);

                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);

                if (rows == 1)
                {
                    return;
                }
                else if (rows == 0)
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, $"使用Timestamp版本的乐观锁，出现冲突。id:{id}, lastUser:{lastUser}, timestamp:{timestamp}, propertyValues:{SerializeUtil.ToJson(propertyNameValues)}", "");
                }
                else
                {
                    throw DatabaseExceptions.FoundTooMuch(modelDef.ModelFullName, $"id:{id}, timestamp:{timestamp}, propertyValues:{SerializeUtil.ToJson(propertyNameValues)}");
                }
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, $"id:{id}, timestamp:{timestamp}, propertyValues:{SerializeUtil.ToJson(propertyNameValues)}", ex);
            }
        }

        public async Task UpdatePropertiesAsync<T>(
            object id,
            IList<(string propertyName, object? oldValue, object? newValue)> propertyNameOldNewValues,
            string lastUser,
            TransactionContext? transContext)
            where T : DbModel, new()
        {
            if (propertyNameOldNewValues.Count <= 0)
            {
                throw new ArgumentException("数量为空", nameof(propertyNameOldNewValues));
            }

            if (id is long longId && longId <= 0)
            {
                throw DatabaseExceptions.LongIdShouldBePositive(longId);
            }

            if (id is Guid guid && guid.IsEmpty())
            {
                throw DatabaseExceptions.GuidShouldNotEmpty();
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser);

            try
            {
                EngineCommand command = DbCommandBuilder.CreateUpdatePropertiesUsingOldNewCompareCommand(
                    EngineType,
                    modelDef,
                    id,
                    propertyNameOldNewValues.Select(t => t.propertyName).ToList(),
                    propertyNameOldNewValues.Select(t => t.oldValue).ToList(),
                    propertyNameOldNewValues.Select(t => t.newValue).ToList(),
                    TimeUtil.Timestamp,
                    lastUser);

                int matchedRows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, modelDef.DatabaseName!, command).ConfigureAwait(false);

                if (matchedRows == 1)
                {
                    return;
                }
                else if (matchedRows == 0)
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, $"使用新旧值对比的乐观锁出现冲突。id:{id}, lastUser:{lastUser}, propertyOldNewValues:{SerializeUtil.ToJson(propertyNameOldNewValues)}", "");
                }
                else
                {
                    throw DatabaseExceptions.FoundTooMuch(modelDef.ModelFullName, $"id:{id}, lastUser:{lastUser}, propertyOldNewValues:{SerializeUtil.ToJson(propertyNameOldNewValues)}");
                }
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, $"id:{id}, lastUser:{lastUser}, propertyOldNewValues:{SerializeUtil.ToJson(propertyNameOldNewValues)}", ex);
            }
        }
        public async Task UpdatePropertiesAsync<T>(ChangedPack changedPropertyPack, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            List<(string propertyName, object? oldValue, object? newValue)> lst = ConvertChangedPackToList(changedPropertyPack, modelDef);

            await UpdatePropertiesAsync<T>(changedPropertyPack.Id!, lst, lastUser, transContext).ConfigureAwait(false);
        }

        public async Task BatchUpdatePropertiesAsync<T>(
            IList<(object id, IList<string> propertyNames, IList<object?> propertyValues, long timestamp)> modelChanges,
            string lastUser,
            TransactionContext? transactionContext) where T : TimestampDbModel, new()
        {
            if (modelChanges.Count == 0)
            {
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser);

            var updateChanges = new List<(object id, IList<string> propertyNames, IList<object?> propertyValues, long? oldTimestamp, long? newTimestamp)>(modelChanges.Count);
            long newTimestamp = TimeUtil.Timestamp;

            foreach ((object id, IList<string> propertyNames, IList<object?> propertyValues, long timestamp) in modelChanges)
            {
                if (id is long longId)
                {
                    if (longId <= 0) throw DatabaseExceptions.LongIdShouldBePositive(longId);
                }
                else if (id is Guid guid)
                {
                    if (guid.IsEmpty()) throw DatabaseExceptions.GuidShouldNotEmpty();
                }

                if (timestamp < 638008780206018439)
                {
                    throw DatabaseExceptions.TimestampShouldBePositive(timestamp);
                }

                updateChanges.Add((id, propertyNames, propertyValues, timestamp, newTimestamp));
            }

            try
            {
                var command = DbCommandBuilder.CreateBatchUpdatePropertiesCommand(EngineType, modelDef, updateChanges, lastUser, transactionContext == null);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transactionContext?.Transaction,
                    modelDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(updateChanges), "BatchUpdatePropertiesAsync");
                    }

                    count++;
                }

                if (count != updateChanges.Count)
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(updateChanges), "BatchUpdatePropertiesAsync");
                }
            }
            catch (Exception ex)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(updateChanges), ex);
            }
        }

        public async Task BatchUpdatePropertiesAsync<T>(
            IList<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues)> modelChanges,
            string lastUser,
            TransactionContext? transactionContext = null) where T : DbModel, new()
        {
            if (modelChanges.Count == 0)
            {
                return;
            }

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser);

            var updateChanges = new List<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues)>(modelChanges.Count);

            foreach (var (id, propertyNames, oldPropertyValues, newPropertyValues) in modelChanges)
            {
                if (id is long longId)
                {
                    if (longId <= 0) throw DatabaseExceptions.LongIdShouldBePositive(longId);
                }
                else if (id is Guid guid)
                {
                    if (guid.IsEmpty()) throw DatabaseExceptions.GuidShouldNotEmpty();
                }

                updateChanges.Add((id, propertyNames, oldPropertyValues, newPropertyValues));
            }

            try
            {
                var command = DbCommandBuilder.CreateBatchUpdatePropertiesUsingOldNewCompareCommand(EngineType, modelDef, updateChanges, TimeUtil.Timestamp, lastUser, transactionContext == null);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(
                    transactionContext?.Transaction,
                    modelDef.DatabaseName!,
                    command,
                    true).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(updateChanges), "BatchUpdatePropertiesAsync");
                    }

                    count++;
                }

                if (count != updateChanges.Count)
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(updateChanges), "BatchUpdatePropertiesAsync");
                }
            }
            catch (Exception ex)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(updateChanges), ex);
            }
        }

        public Task BatchUpdatePropertiesAsync<T>(IEnumerable<ChangedPack> changedPacks, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            var lst = ConvertChangedPackToList(changedPacks, modelDef);

            return BatchUpdatePropertiesAsync<T>(lst, lastUser, transContext);
        }

        private static List<(string propertyName, object? oldValue, object? newValue)> ConvertChangedPackToList(ChangedPack changedPropertyPack, DbModelDef modelDef)
        {
            if (changedPropertyPack == null || changedPropertyPack.Id == null || changedPropertyPack.ChangedProperties.IsNullOrEmpty())
            {
                throw DatabaseExceptions.ChangedPropertyPackError("ChangedProperties为空或者Id为null", changedPropertyPack);
            }

            List<(string propertyName, object? oldValue, object? newValue)> lst = new List<(string propertyName, object? oldValue, object? newValue)>();

            foreach (ChangedProperty cp in changedPropertyPack.ChangedProperties)
            {
                DbModelPropertyDef? propertyDef = modelDef.GetPropertyDef(cp.PropertyName);

                if (propertyDef == null)
                {
                    throw DatabaseExceptions.ChangedPropertyPackError("包含不属于当前DbModel的属性", changedPropertyPack);
                }

                lst.Add((
                    cp.PropertyName,
                    SerializeUtil.FromJsonElement(propertyDef.Type, cp.OldValue),
                    SerializeUtil.FromJsonElement(propertyDef.Type, cp.NewValue)));
            }

            return lst;
        }

        private static IList<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues)> ConvertChangedPackToList(IEnumerable<ChangedPack> changedPropertyPacks, DbModelDef modelDef)
        {
            var lst = new List<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues)>(changedPropertyPacks.Count());

            foreach (var changedPropertyPack in changedPropertyPacks)
            {
                if (changedPropertyPack == null || changedPropertyPack.Id == null || changedPropertyPack.ChangedProperties.IsNullOrEmpty())
                {
                    throw DatabaseExceptions.ChangedPropertyPackError("ChangedProperties为空或者Id为null", changedPropertyPack);
                }

                List<string> propertyNames = new List<string>();
                List<object?> oldPropertyValues = new List<object?>();
                List<object?> newPropertyValues = new List<object?>();

                foreach (ChangedProperty cp in changedPropertyPack.ChangedProperties)
                {
                    DbModelPropertyDef? propertyDef = modelDef.GetPropertyDef(cp.PropertyName);

                    if (propertyDef == null)
                    {
                        throw DatabaseExceptions.ChangedPropertyPackError("包含不属于当前DbModel的属性", changedPropertyPack);
                    }

                    propertyNames.Add(cp.PropertyName);
                    oldPropertyValues.Add(SerializeUtil.FromJsonElement(propertyDef.Type, cp.OldValue));
                    newPropertyValues.Add(SerializeUtil.FromJsonElement(propertyDef.Type, cp.NewValue));
                }

                lst.Add((changedPropertyPack.Id!, propertyNames, oldPropertyValues, newPropertyValues));
            }

            return lst;
        }
    }
}
