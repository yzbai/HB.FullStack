using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

[assembly: InternalsVisibleTo("HB.Infrastructure.MySQL")]
[assembly: InternalsVisibleTo("HB.Infrastructure.SQLite")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]

namespace HB.FullStack.Database
{

    //TODO: 改造Exceptions

    internal static class DbExceptions
    {
        internal static Exception TimestampShouldBePositive(long timestamp)
        {
            DbException exception = new DbException(ErrorCodes.TimestampError, nameof(TimestampShouldBePositive), null, null);

            exception.Data["WrongTimestamp"] = timestamp;
            exception.Data["Cause"] = "Timestamp Should Be Positive";

            return exception;
        }

        //internal static Exception TimestampNotExists(DbEngineType engineType, DbModelDef modelDef, IList<string> propertyNames, [CallerMemberName] string? caller = null)
        //{
        //    DbException ex = new DbException(ErrorCodes.TimestampError, nameof(TimestampNotExists), null, null);

        //    ex.Data["EngineType"] = engineType.ToString();
        //    ex.Data["Model"] = modelDef.FullName;
        //    ex.Data["PropertyNames"] = propertyNames.ToJoinedString(",");
        //    ex.Data["Caller"] = caller;

        //    return ex;
        //}

        internal static Exception MySQLExecuterError(string? commandText, string? engineInnerCode, string? sqlState, Exception? innerException = null)
        {
            DbException exception = new DbException(ErrorCodes.DbEngineExecuterError, nameof(MySQLExecuterError), innerException, null);

            exception.Data["EngineInnerCode"] = engineInnerCode;
            exception.Data["CommandText"] = commandText;
            exception.Data["SqlState"] = sqlState;
            exception.ComeFromEngine = true;

            return exception;
        }

        internal static Exception MySQLUnKownExecuterError(string? commandText, Exception? innerException = null)
        {
            DbException exception = new DbException(ErrorCodes.DbEngineUnKownExecuterError, nameof(MySQLUnKownExecuterError), innerException, null);

            exception.Data["CommandText"] = commandText;
            exception.ComeFromEngine = true;

            return exception;
        }

        internal static Exception SQLiteExecuterError(string? commandText, string? cause, int? errorCode, int? extendedErrorCode, Exception? innerException = null)
        {
            DbException exception = new DbException(ErrorCodes.DbEngineExecuterError, nameof(SQLiteExecuterError), innerException, null);

            exception.Data["CommandText"] = commandText;
            exception.Data["Cause"] = cause;
            exception.Data["ErrorCode"] = errorCode;
            exception.Data["ExtendedErrorCode"] = extendedErrorCode;
            exception.ComeFromEngine = true;

            return exception;
        }

        internal static Exception SQLiteUnKownExecuterError(string? commandText, Exception? innerException = null)
        {
            DbException exception = new DbException(ErrorCodes.DbEngineUnKownExecuterError, nameof(SQLiteUnKownExecuterError), innerException, null);

            exception.Data["CommandText"] = commandText;
            exception.ComeFromEngine = true;

            return exception;
        }

        internal static Exception TransactionError(string cause, string? callerMemberName, int callerLineNumber)
        {
            DbException exception = new DbException(ErrorCodes.TransactionError, nameof(TransactionError), null, null);
            exception.Data["CallerMemeberName"] = callerMemberName;
            exception.Data["CallerLineNumber"] = callerLineNumber;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception TransactionConnectionIsNull(string commandText)
        {
            DbException exception = new DbException(ErrorCodes.TransactionError, nameof(TransactionConnectionIsNull), null, null);
            exception.Data["CommandText"] = commandText;
            exception.Data["Cause"] = "Connection Is Null";

            return exception;
        }

        internal static Exception TableCreateError(int version, string? dbSchemaName, string cause, Exception? innerException = null)
        {
            DbException exception = new DbException(ErrorCodes.DatabaseTableCreateError, nameof(TableCreateError), innerException, null);

            exception.Data["DatabaseVersion"] = version;
            exception.Data["DbSchemaName"] = dbSchemaName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception DbSchemaError(int version, string? dbSchemaName, string cause, Exception? innerException = null)
        {
            DbException exception = new DbException(ErrorCodes.DbConfigError, nameof(DbSchemaError), innerException, null);

            exception.Data["DatabaseVersion"] = version;
            exception.Data["DbSchemaName"] = dbSchemaName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception MigrateError(string? dbSchemaName, string cause, Exception? innerException = null)
        {
            DbException exception = new DbException(ErrorCodes.MigrateError, nameof(MigrateError), innerException, null);

            exception.Data["DbSchemaName"] = dbSchemaName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception FoundTooMuch(string? type, string? from, string? where)
        {
            DbException exception = new DbException(ErrorCodes.FoundTooMuch, nameof(FoundTooMuch), null, null);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception FoundTooMuch(string? type, string? item)
        {
            DbException exception = new DbException(ErrorCodes.FoundTooMuch, nameof(FoundTooMuch), null, null);

            exception.Data["Type"] = type;
            exception.Data["Item"] = item;

            return exception; ;
        }

        internal static Exception NotFound(string cause)
        {
            DbException exception = new DbException(ErrorCodes.NotFound, cause, null, null);

            return exception; ;
        }

        internal static Exception PropertyNotFound(string? type, string property)
        {
            DbException exception = new DbException(ErrorCodes.PropertyNotFound, nameof(PropertyNotFound), null, null);
            exception.Data["Type"] = type;
            exception.Data["Property"] = property;

            return exception;
        }

        internal static Exception UnKown(string type, string? from, string? where, Exception? innerException = null)
        {
            DbException exception = new DbException(ErrorCodes.UnKown, nameof(UnKown), innerException, null);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception UnKown(string type, string? item, Exception? innerException = null)
        {
            DbException exception = new DbException(ErrorCodes.UnKown, nameof(UnKown), innerException, null);

            exception.Data["Type"] = type;
            exception.Data["item"] = item;

            return exception;
        }

        internal static Exception ConcurrencyConflict(string type, string? item, string? cause)
        {
            DbException exception = new DbException(ErrorCodes.ConcurrencyConflict, nameof(ConcurrencyConflict), null, null);

            exception.Data["Type"] = type;
            exception.Data["item"] = item;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception NotWriteable(string type, string? database)
        {
            DbException exception = new DbException(ErrorCodes.DatabaseNotWriteable, nameof(NotWriteable), null, null);

            exception.Data["Type"] = type;
            exception.Data["Database"] = database;

            return exception;
        }

        internal static Exception SystemInfoError(string cause)
        {
            DbException exception = new DbException(ErrorCodes.SystemInfoError, cause, null, null);

            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception UseDateTimeOffsetOnly()
        {
            DbException exception = new DbException(ErrorCodes.UseDateTimeOffsetOnly, nameof(UseDateTimeOffsetOnly), null, null);

            return exception;
        }

        internal static Exception ModelHasNotSupportedPropertyType(string type, string? propertyTypeName, string propertyName)
        {
            DbException exception = new DbException(ErrorCodes.ModelHasNotSupportedPropertyType, nameof(ModelHasNotSupportedPropertyType), null, null);

            exception.Data["Type"] = type;
            exception.Data["PropertyTypeName"] = propertyTypeName;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception ModelError(string? type, string propertyName, string cause)
        {
            DbException exception = new DbException(ErrorCodes.ModelError, cause, null, null);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = cause;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception ModelError(string cause)
        {
            DbException exception = new DbException(ErrorCodes.ModelError, cause, null, null);

            return exception;
        }

        internal static Exception ModelTimestampError(string type, long timestamp, string cause)
        {
            DbException exception = new DbException(ErrorCodes.TimestampError, cause, null, null);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = "Version Error. - " + cause;
            exception.Data["Timestamp"] = timestamp;

            return exception;
        }

        internal static Exception MapperError(Exception innerException)
        {
            DbException exception = new DbException(ErrorCodes.MapperError, nameof(MapperError), innerException, null);

            return exception;
        }

        internal static Exception SqlJoinTypeMixedError()
        {
            DbException exception = new DbException(ErrorCodes.SqlError, "Sql join type mixed.", null, null);

            return exception;
        }

        internal static Exception NotSupportYet(string cause, DbEngineType engineType)
        {
            DbException exception = new DbException(ErrorCodes.NotSupported, cause, null, null);
            exception.Data["Cause"] = cause;
            exception.Data["EngineType"] = engineType.ToString();

            return exception;
        }

        internal static Exception TooManyForBatch(string cause, int count, string lastUser)
        {
            DbException exception = new DbException(ErrorCodes.BatchError, cause, null, null);
            exception.Data["Cause"] = cause;
            exception.Data["Count"] = count;
            exception.Data["LastUser"] = lastUser;

            return exception;
        }

        internal static Exception TypeConverterError(string cause, string? typeFullName)
        {
            DbException exception = new DbException(ErrorCodes.TypeConverterError, cause, null, null);
            exception.Data["Cause"] = cause;
            exception.Data["Type"] = typeFullName;

            return exception;
        }

        internal static Exception GuidShouldNotEmpty()
        {
            DbException exception = new DbException(ErrorCodes.EmptyGuid, nameof(GuidShouldNotEmpty), null, null);

            return exception;
        }

        internal static Exception UpdatePropertiesCountShouldBePositive()
        {
            DbException exception = new DbException(ErrorCodes.UpdatePropertiesCountShouldBePositive, nameof(UpdatePropertiesCountShouldBePositive), null, null);

            return exception;
        }

        internal static Exception LongIdShouldBePositive(long id)
        {
            DbException exception = new DbException(ErrorCodes.LongIdShouldBePositive, nameof(LongIdShouldBePositive), null, null);
            exception.Data["Id"] = id;

            return exception;
        }

        internal static Exception NoSuchForeignKey(string modelFullName, string foreignKeyName)
        {
            DbException exception = new DbException(ErrorCodes.NoSuchForeignKey, nameof(NoSuchForeignKey), null, null);
            exception.Data["ModelFullName"] = modelFullName;
            exception.Data["ForeignKeyName"] = foreignKeyName;

            return exception;
        }

        internal static Exception NoSuchProperty(string modelFullName, string propertyName)
        {
            DbException exception = new DbException(ErrorCodes.NoSuchProperty, nameof(NoSuchProperty), null, null);
            exception.Data["ModelFullName"] = modelFullName;
            exception.Data["PropertyName"] = propertyName;
            return exception;
        }

        internal static Exception KeyValueNotLongOrGuid(string modelFullName, string foreignKeyName, object? foreignKeyValue, string? foreignKeyType)
        {
            DbException exception = new DbException(ErrorCodes.KeyValueNotLongOrGuid, nameof(KeyValueNotLongOrGuid), null, null);
            exception.Data["ModelFullName"] = modelFullName;
            exception.Data["ForeignKeyName"] = foreignKeyName;
            exception.Data["ForeignKeyValue"] = foreignKeyValue?.ToString();
            exception.Data["ForeignKeyType"] = foreignKeyType;

            return exception;
        }

        internal static Exception NotInitializedYet()
        {
            DbException exception = new DbException(ErrorCodes.NotInitializedYet, nameof(NotInitializedYet), null, null);
            return exception;
        }

        internal static Exception UpdateVersionError<T>(int originalVersion, int updateToVersion, T item) where T : IDbModel, new()
        {
            DbException exception = new DbException(ErrorCodes.UpdateVersionError, nameof(UpdateVersionError), null, null);

            exception.Data["OriginalVersion"] = originalVersion;
            exception.Data["UpdateToVersion"] = updateToVersion;
            exception.Data["Item"] = SerializeUtil.ToJson(item);

            return exception;
        }

        internal static Exception DatabaseNotWritable(string modelName, string itemJson)
        {
            DbException ex = new DbException(ErrorCodes.DatabaseNotWriteable, nameof(DatabaseNotWritable), null, null);

            ex.Data["ModelName"] = modelName;
            ex.Data["Item"] = itemJson;

            return ex;
        }

        internal static Exception ChangedPropertyPackError(string cause, PropertyChangePack? changePack, string? modelFullName)
        {
            DbException ex = new DbException(ErrorCodes.ChangedPackError, cause, null, null);

            ex.Data["ModelFullName"] = modelFullName;
            ex.Data["PropertyNames"] = changePack?.PropertyChanges.Select(c => c.Key).ToJoinedString(",");

            return ex;
        }

        internal static Exception DuplicateKeyError(string commandText, Exception? innerException)
        {
            DbException ex = new DbException(ErrorCodes.DuplicateKeyEntry, "DuplicateKeyError", innerException, new { CommandText = commandText });
            ex.ComeFromEngine = true;
            return ex;
        }

        internal static Exception DbSchemaError(string? dbSchemaName, string cause)
        {
            DbException exception = new DbException(ErrorCodes.DbConfigError, cause, null, null);

            exception.Data["DbSchemaName"] = dbSchemaName;

            return exception;
        }

        internal static Exception SameTableNameInSameDbSchema(string? dbSchemaName, string? tableName)
        {
            DbException ex = new DbException(ErrorCodes.ModelDefError, nameof(SameTableNameInSameDbSchema), null, null);

            ex.Data["DbSchemaName"] = dbSchemaName;
            ex.Data["TableName"] = tableName;

            return ex;
        }

        internal static Exception DataTooLong(Exception innerEx)
        {
            DbException ex = new DbException(ErrorCodes.DbDataTooLong, innerEx.Message, innerEx, null);

            return ex;
        }

        internal static Exception UpdatePackCountNotEqual()
        {
            DbException ex = new DbException(ErrorCodes.DbUpdateUsingTimestampError, nameof(UpdatePackCountNotEqual), null, null);
            return ex;
        }

        internal static Exception UpdatePackEmpty()
        {
            DbException ex = new DbException(ErrorCodes.DbUpdateUsingTimestampError, nameof(UpdatePackEmpty), null, null);
            return ex;
        }

        internal static Exception UpdatePropertiesMethodWrong(string cause, IEnumerable<string> propertyNames, DbModelDef modelDef)
        {
            DbException ex = new DbException(ErrorCodes.DbUpdatePropertiesError, cause, null, null);

            ex.Data["DbSchemaName"] = modelDef.DbSchema.Name;
            ex.Data["DbModelName"] = modelDef.FullName;
            ex.Data["PropertyNames"] = propertyNames.ToJoinedString(",");

            return ex;
        }

        internal static Exception ConflictCheckError(string message)
        {
            DbException ex = new DbException(ErrorCodes.DbConflictMethodError, message, null, null);
            return ex;
        }

        internal static Exception AddError(string message)
        {
            DbException ex = new DbException(ErrorCodes.DbAddError, message, null, null);

            return ex;
        }
    }
}