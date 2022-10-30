using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Extensions;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    //TODO: 添加对newTimestamp的检查
    partial class DefaultDatabase
    {
        public async Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            ThrowIf.NotValid(item, nameof(item));

            DbModelDef modelDef = DefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

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

                EngineCommand command = DbCommandBuilder.CreateUpdateCommand(modelDef, item, oldTimestamp);

                var engine = DbManager.GetDatabaseEngine(modelDef);

                long rows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(DbManager.GetConnectionString(modelDef, true), command).ConfigureAwait(false);

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

        public async Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            if (items.IsNullOrEmpty())
            {
                return;
            }

            if (items.Count() == 1)
            {
                await UpdateAsync(items.First(), lastUser, transContext).ConfigureAwait(false);
                return;
            }

            ThrowIf.NotValid(items, nameof(items));

            DbModelDef modelDef = DefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ThrowIfTooMuchItems(items, lastUser, modelDef);
            ThrowIfNotWriteable(modelDef);
            TruncateLastUser(ref lastUser);

            List<long> oldTimestamps = new List<long>();
            List<string?> oldLastUsers = new List<string?>();

            try
            {
                PrepareBatchItems(items, lastUser, oldTimestamps, oldLastUsers, modelDef);

                var command = DbCommandBuilder.CreateBatchUpdateCommand(modelDef, items, oldTimestamps, transContext == null);

                var engine = DbManager.GetDatabaseEngine(modelDef);

                using var reader = transContext != null
                    ? await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(DbManager.GetConnectionString(modelDef, true), command).ConfigureAwait(false);

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
            TransactionContext? transContext,
            long? newTimestamp = null) where T : TimestampDbModel, new()
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

            DbModelDef modelDef = DefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser);

            try
            {
                EngineCommand command = DbCommandBuilder.CreateUpdatePropertiesCommand(
                    modelDef: modelDef,
                    id: id,
                    propertyNames: propertyNameValues.Select(t => t.propertyName).ToList(),
                    propertyValues: propertyNameValues.Select(t => t.propertyValue).ToList(),
                    oldTimestamp: timestamp,
                    newTimestamp: newTimestamp ?? TimeUtil.Timestamp,
                    lastUser: lastUser);

                var engine = DbManager.GetDatabaseEngine(modelDef);

                long rows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(DbManager.GetConnectionString(modelDef, true), command).ConfigureAwait(false);

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
            TransactionContext? transContext,
            long? newTimestamp = null) where T : DbModel, new()
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

            DbModelDef modelDef = DefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ThrowIfNotWriteable(modelDef);

            TruncateLastUser(ref lastUser);

            try
            {
                EngineCommand command = DbCommandBuilder.CreateUpdatePropertiesUsingOldNewCompareCommand(
                    modelDef,
                    id,
                    propertyNameOldNewValues.Select(t => t.propertyName).ToList(),
                    propertyNameOldNewValues.Select(t => t.oldValue).ToList(),
                    propertyNameOldNewValues.Select(t => t.newValue).ToList(),
                    newTimestamp ?? TimeUtil.Timestamp,
                    lastUser);

                var engine = DbManager.GetDatabaseEngine(modelDef);

                int matchedRows = transContext != null
                    ? await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandNonQueryAsync(DbManager.GetConnectionString(modelDef, true), command).ConfigureAwait(false); ;

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

        public async Task UpdatePropertiesAsync<T>(
            ChangedPack changedPack,
            string lastUser,
            TransactionContext? transContext) where T : DbModel, new()
        {
            DbModelDef modelDef = DefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            //这里不包含Timestamp改动
            List<(string propertyName, object? oldValue, object? newValue)> lst = ConvertChangedPackToList(changedPack, modelDef, out long? newTimestamp);

            await UpdatePropertiesAsync<T>(changedPack.Id!, lst, lastUser, transContext, newTimestamp).ConfigureAwait(false);
        }

        public async Task BatchUpdatePropertiesAsync<T>(
            IList<(object id, IList<(string propertyName, object? propertyValue)> properties, long oldTimestamp, long? newTimestamp)> modelChanges,
            string lastUser,
            TransactionContext? transactionContext) where T : TimestampDbModel, new()
        {

            if (modelChanges.IsNullOrEmpty())
            {
                return;
            }

            if (modelChanges.Count == 1)
            {
                var (id, properties, oldTimestamp, newTimestamp) = modelChanges[0];

                await UpdatePropertiesAsync<T>(
                    id,
                    properties,
                    oldTimestamp,
                    lastUser,
                    transactionContext,
                    newTimestamp).ConfigureAwait(false);

                return;
            }

            DbModelDef modelDef = DefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);

            ThrowIfNotWriteable(modelDef);
            TruncateLastUser(ref lastUser);

            var updateChanges = ConvertToCommandTuple(modelChanges);

            try
            {
                var command = DbCommandBuilder.CreateBatchUpdatePropertiesCommand(
                    modelDef,
                    updateChanges,
                    lastUser,
                    transactionContext == null);

                var engine = DbManager.GetDatabaseEngine(modelDef);

                using var reader = transactionContext != null
                    ? await engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(DbManager.GetConnectionString(modelDef, true), command).ConfigureAwait(false);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(modelChanges), "BatchUpdatePropertiesAsync");
                    }

                    count++;
                }

                if (count != modelChanges.Count)
                {
                    throw DatabaseExceptions.ConcurrencyConflict(modelDef.ModelFullName, SerializeUtil.ToJson(modelChanges), "BatchUpdatePropertiesAsync");
                }
            }
            catch (Exception ex)
            {
                throw DatabaseExceptions.UnKown(modelDef.ModelFullName, SerializeUtil.ToJson(modelChanges), ex);
            }
        }

        public async Task BatchUpdatePropertiesAsync<T>(
            IList<(object id, IList<(string propertyNames, object? oldPropertyValues, object? newPropertyValues)> properties, long? newTimestamp)> modelChanges,
            string lastUser,
            TransactionContext? transactionContext = null) where T : DbModel, new()
        {
            if (modelChanges.IsNullOrEmpty())
            {
                return;
            }

            if (modelChanges.Count == 1)
            {
                (object id, IList<(string propertyNames, object? oldPropertyValues, object? newPropertyValues)> properties, long? newTimestamp) = modelChanges[0];

                await UpdatePropertiesAsync<T>(id, properties, lastUser, transactionContext, newTimestamp);

                return;
            }

            DbModelDef modelDef = DefFactory.GetDef<T>()!;

            ThrowIfNotWriteable(modelDef);
            TruncateLastUser(ref lastUser);

            List<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues, long newTimestamp)> updateChanges = ConvertToDbModelUpdateProperties(modelChanges);

            try
            {
                var command = DbCommandBuilder.CreateBatchUpdatePropertiesUsingOldNewCompareCommand(
                    modelDef,
                    updateChanges,
                    lastUser,
                    transactionContext == null);

                var engine = DbManager.GetDatabaseEngine(modelDef);

                using var reader = transactionContext != null
                    ? await engine.ExecuteCommandReaderAsync(transactionContext.Transaction, command).ConfigureAwait(false)
                    : await engine.ExecuteCommandReaderAsync(DbManager.GetConnectionString(modelDef, true), command).ConfigureAwait(false); ;

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
            if (changedPacks.IsNullOrEmpty())
            {
                return Task.CompletedTask;
            }

            if (changedPacks.Count() == 1)
            {
                return UpdatePropertiesAsync<T>(changedPacks.First(), lastUser, transContext);
            }

            DbModelDef modelDef = DefFactory.GetDef<T>()!;

            var lst = ConvertChangedPackToList(changedPacks, modelDef);

            return BatchUpdatePropertiesAsync<T>(lst, lastUser, transContext);
        }

        private static List<(string propertyName, object? oldValue, object? newValue)> ConvertChangedPackToList(ChangedPack changedPropertyPack, DbModelDef modelDef, out long? newTimestamp)
        {
            if (changedPropertyPack == null || changedPropertyPack.Id == null || changedPropertyPack.ChangedProperties.IsNullOrEmpty())
            {
                throw DatabaseExceptions.ChangedPropertyPackError("ChangedProperties为空或者Id为null", changedPropertyPack, modelDef.ModelFullName);
            }

            List<(string propertyName, object? oldValue, object? newValue)> lst = new List<(string propertyName, object? oldValue, object? newValue)>();

            newTimestamp = null;

            foreach (ChangedProperty cp in changedPropertyPack.ChangedProperties)
            {
                if (cp.PropertyName == nameof(ITimestampModel.Timestamp))
                {
                    newTimestamp = (long?)cp.NewValue;
                    continue;
                }

                lst.Add((cp.PropertyName, cp.OldValue, cp.NewValue));
            }

            return lst;
        }

        private static IList<(object id, IList<(string propertyName, object? oldPropertyValue, object? newPropertyValue)> properties, long? newTimestamp)> ConvertChangedPackToList(
            IEnumerable<ChangedPack> changedPropertyPacks, DbModelDef modelDef)
        {
            var lst = new List<(object id, IList<(string propertyName, object? oldPropertyValue, object? newPropertyValue)> properties, long? newTimestamp)>(changedPropertyPacks.Count());

            foreach (var changedPropertyPack in changedPropertyPacks)
            {
                if (changedPropertyPack == null || changedPropertyPack.Id == null || changedPropertyPack.ChangedProperties.IsNullOrEmpty())
                {
                    throw DatabaseExceptions.ChangedPropertyPackError("ChangedProperties为空或者Id为null", changedPropertyPack, modelDef.ModelFullName);
                }

                List<(string propertyName, object? oldPropertyValue, object? newPropertyValue)> properties = new List<(string propertyName, object? oldPropertyValue, object? newPropertyValue)>();

                long? curNewTimestamp = null;

                foreach (ChangedProperty cp in changedPropertyPack.ChangedProperties)
                {
                    if (cp.PropertyName == nameof(ITimestampModel.Timestamp))
                    {
                        curNewTimestamp = (long?)cp.NewValue;
                        continue;
                    }

                    properties.Add((cp.PropertyName, cp.OldValue, cp.NewValue));
                }

                lst.Add((changedPropertyPack.Id!, properties, curNewTimestamp));
            }

            return lst;
        }

        private static List<(object id, IList<string> propertyNames, IList<object?> propertyValues, long? oldTimestamp, long? newTimestamp)> ConvertToCommandTuple(
            IList<(object id, IList<(string propertyName, object? propertyValue)> properties, long oldTimestamp, long? newTimestamp)> modelChanges)
        {
            var updateChanges = new List<(object id, IList<string> propertyNames, IList<object?> propertyValues, long? oldTimestamp, long? newTimestamp)>(modelChanges.Count);

            foreach ((object id, IList<(string propertyName, object? propertyValue)> properties, long oldTimestamp, long? newTimestamp) in modelChanges)
            {
                if (oldTimestamp < 638008780206018439 || newTimestamp < 638008780206018439)
                {
                    throw DatabaseExceptions.TimestampShouldBePositive(oldTimestamp);
                }

                List<string> propertyNames = new List<string>(properties.Count);
                List<object?> propertyValues = new List<object?>(properties.Count);

                foreach ((string propertyName, object? propertyValue) in properties)
                {
                    propertyNames.Add(propertyName);
                    propertyValues.Add(propertyValue);
                }

                updateChanges.Add((id, propertyNames, propertyValues, oldTimestamp, newTimestamp));
            }

            return updateChanges;
        }

        private static List<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues, long newTimestamp)> ConvertToDbModelUpdateProperties(
            IList<(object id, IList<(string propertyName, object? oldPropertyValue, object? newPropertyValue)> properties, long? newTimestamp)> modelChanges)
        {
            var updateChanges = new List<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues, long newTimestamp)>(modelChanges.Count);

            long defaultTimestamp = TimeUtil.Timestamp;

            foreach ((object id, IList<(string propertyNames, object? oldPropertyValues, object? newPropertyValues)> properties, long? newTimestamp) in modelChanges)
            {
                if (newTimestamp.HasValue && newTimestamp.Value < 638008780206018439)
                {
                    throw DatabaseExceptions.TimestampShouldBePositive(newTimestamp.Value);
                }

                List<string> propertyNames = new List<string>(properties.Count);
                List<object?> oldPropertyValues = new List<object?>(properties.Count);
                List<object?> newPropertyValues = new List<object?>(properties.Count);

                foreach ((string propertyName, object? oldPropertyValue, object? newPropertyValue) in properties)
                {
                    propertyNames.Add(propertyName);
                    oldPropertyValues.Add(oldPropertyValue);
                    newPropertyValues.Add(newPropertyValue);
                }

                updateChanges.Add((id, propertyNames, oldPropertyValues, newPropertyValues, newTimestamp ?? defaultTimestamp));
            }

            return updateChanges;
        }
    }
}
