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

    internal static class DatabaseExceptions
    {
        internal static Exception TimestampShouldBePositive(long timestamp)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.TimestampError, nameof(TimestampShouldBePositive), null, null);

            exception.Data["WrongTimestamp"] = timestamp;
            exception.Data["Cause"] = "Timestamp Should Be Positive";

            return exception;
        }

        internal static Exception TimestampNotExists(EngineType engineType, DbModelDef modelDef, IList<string> propertyNames, [CallerMemberName] string? caller = null)
        {
            DatabaseException ex = new DatabaseException(ErrorCodes.TimestampError, nameof(TimestampNotExists), null, null);

            ex.Data["EngineType"] = engineType.ToString();
            ex.Data["Model"] = modelDef.ModelFullName;
            ex.Data["PropertyNames"] = propertyNames.ToJoinedString(",");
            ex.Data["Caller"] = caller;

            return ex;
        }

        internal static Exception MySQLExecuterError(string? commandText, string? cause, string? sqlState, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.DbEngineExecuterError, nameof(MySQLExecuterError), innerException, null);

            exception.Data["Cause"] = cause;
            exception.Data["CommandText"] = commandText;
            exception.Data["SqlState"] = sqlState;
            exception.ComeFromEngine = true;

            return exception;
        }

        internal static Exception MySQLUnKownExecuterError(string? commandText, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.DbEngineUnKownExecuterError, nameof(MySQLUnKownExecuterError), innerException, null);

            exception.Data["CommandText"] = commandText;
            exception.ComeFromEngine = true;

            return exception;
        }

        internal static Exception SQLiteExecuterError(string? commandText, string? cause, int? errorCode, int? extendedErrorCode, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.DbEngineExecuterError, nameof(SQLiteExecuterError), innerException, null);

            exception.Data["CommandText"] = commandText;
            exception.Data["Cause"] = cause;
            exception.Data["ErrorCode"] = errorCode;
            exception.Data["ExtendedErrorCode"] = extendedErrorCode;
            exception.ComeFromEngine = true;

            return exception;
        }

        internal static Exception SQLiteUnKownExecuterError(string? commandText, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.DbEngineUnKownExecuterError, nameof(SQLiteUnKownExecuterError), innerException, null);

            exception.Data["CommandText"] = commandText;
            exception.ComeFromEngine = true;

            return exception;
        }

        internal static Exception TransactionError(string cause, string? callerMemberName, int callerLineNumber)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.TransactionError, nameof(TransactionError), null, null);
            exception.Data["CallerMemeberName"] = callerMemberName;
            exception.Data["CallerLineNumber"] = callerLineNumber;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception TransactionConnectionIsNull(string commandText)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.TransactionError, nameof(TransactionConnectionIsNull), null, null);
            exception.Data["CommandText"] = commandText;
            exception.Data["Cause"] = "Connection Is Null";

            return exception;
        }

        internal static Exception TableCreateError(int version, string? databaseName, string cause, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.DatabaseTableCreateError, nameof(TableCreateError), innerException, null);

            exception.Data["DatabaseVersion"] = version;
            exception.Data["DatabaseName"] = databaseName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception DbSettingError(int version, string? databaseName, string cause, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.DbSettingError, nameof(DbSettingError), innerException, null);

            exception.Data["DatabaseVersion"] = version;
            exception.Data["DatabaseName"] = databaseName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception MigrateError(string? databaseName, string cause, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.MigrateError, nameof(MigrateError), innerException, null);

            exception.Data["DatabaseName"] = databaseName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception FoundTooMuch(string? type, string? from, string? where)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.FoundTooMuch, nameof(FoundTooMuch), null, null);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception FoundTooMuch(string? type, string? item)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.FoundTooMuch, nameof(FoundTooMuch), null, null);

            exception.Data["Type"] = type;
            exception.Data["Item"] = item;

            return exception; ;
        }

        internal static Exception PropertyNotFound(string? type, string property)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.PropertyNotFound, nameof(PropertyNotFound), null, null);
            exception.Data["Type"] = type;
            exception.Data["Property"] = property;

            return exception;
        }

        internal static Exception UnKown(string type, string? from, string? where, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.UnKown, nameof(UnKown), innerException, null);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception UnKown(string type, string? item, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.UnKown, nameof(UnKown), innerException, null);

            exception.Data["Type"] = type;
            exception.Data["item"] = item;

            return exception;
        }

        internal static Exception ConcurrencyConflict(string type, string? item, string? cause)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.ConcurrencyConflict, nameof(ConcurrencyConflict), null, null);

            exception.Data["Type"] = type;
            exception.Data["item"] = item;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception NotWriteable(string type, string? database)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.DatabaseNotWriteable, nameof(NotWriteable), null, null);

            exception.Data["Type"] = type;
            exception.Data["Database"] = database;

            return exception;
        }

        internal static Exception SystemInfoError(string cause)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.SystemInfoError, cause, null, null);

            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception UseDateTimeOffsetOnly()
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.UseDateTimeOffsetOnly, nameof(UseDateTimeOffsetOnly), null, null);

            return exception;
        }

        internal static Exception ModelHasNotSupportedPropertyType(string type, string? propertyTypeName, string propertyName)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.ModelHasNotSupportedPropertyType, nameof(ModelHasNotSupportedPropertyType), null, null);

            exception.Data["Type"] = type;
            exception.Data["PropertyTypeName"] = propertyTypeName;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception ModelError(string? type, string propertyName, string cause)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.ModelError, cause, null, null);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = cause;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception ModelTimestampError(string type, long timestamp, string cause)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.TimestampError, cause, null, null);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = "Version Error. - " + cause;
            exception.Data["Timestamp"] = timestamp;

            return exception;
        }

        internal static Exception MapperError(Exception innerException)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.MapperError, nameof(MapperError), innerException, null);

            return exception;
        }

        internal static Exception SqlJoinTypeMixedError()
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.SqlError, "Sql join type mixed.", null, null);

            return exception;
        }

        internal static Exception NotSupportYet(string cause, EngineType engineType)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.NotSupported, cause, null, null);
            exception.Data["Cause"] = cause;
            exception.Data["EngineType"] = engineType.ToString();

            return exception;
        }

        internal static Exception TooManyForBatch(string cause, int count, string lastUser)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.BatchError, cause, null, null);
            exception.Data["Cause"] = cause;
            exception.Data["Count"] = count;
            exception.Data["LastUser"] = lastUser;

            return exception;
        }

        internal static Exception TypeConverterError(string cause, string? typeFullName)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.TypeConverterError, cause, null, null);
            exception.Data["Cause"] = cause;
            exception.Data["Type"] = typeFullName;

            return exception;
        }

        internal static Exception GuidShouldNotEmpty()
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.EmptyGuid, nameof(GuidShouldNotEmpty), null, null);

            return exception;
        }

        internal static Exception UpdatePropertiesCountShouldBePositive()
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.UpdatePropertiesCountShouldBePositive, nameof(UpdatePropertiesCountShouldBePositive), null, null);

            return exception;
        }

        internal static Exception LongIdShouldBePositive(long id)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.LongIdShouldBePositive, nameof(LongIdShouldBePositive), null, null);
            exception.Data["Id"] = id;

            return exception;
        }

        internal static Exception NoSuchForeignKey(string modelFullName, string foreignKeyName)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.NoSuchForeignKey, nameof(NoSuchForeignKey), null, null);
            exception.Data["ModelFullName"] = modelFullName;
            exception.Data["ForeignKeyName"] = foreignKeyName;

            return exception;
        }

        internal static Exception NoSuchProperty(string modelFullName, string propertyName)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.NoSuchProperty, nameof(NoSuchProperty), null, null);
            exception.Data["ModelFullName"] = modelFullName;
            exception.Data["PropertyName"] = propertyName;
            return exception;
        }

        internal static Exception KeyValueNotLongOrGuid(string modelFullName, string foreignKeyName, object? foreignKeyValue, string? foreignKeyType)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.KeyValueNotLongOrGuid, nameof(KeyValueNotLongOrGuid), null, null);
            exception.Data["ModelFullName"] = modelFullName;
            exception.Data["ForeignKeyName"] = foreignKeyName;
            exception.Data["ForeignKeyValue"] = foreignKeyValue?.ToString();
            exception.Data["ForeignKeyType"] = foreignKeyType;

            return exception;
        }

        internal static Exception NotInitializedYet()
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.NotInitializedYet, nameof(NotInitializedYet), null, null);
            return exception;
        }

        internal static Exception UpdateVersionError<T>(int originalVersion, int updateToVersion, T item) where T : DbModel, new()
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.UpdateVersionError, nameof(UpdateVersionError), null, null);

            exception.Data["OriginalVersion"] = originalVersion;
            exception.Data["UpdateToVersion"] = updateToVersion;
            exception.Data["Item"] = SerializeUtil.ToJson(item);

            return exception;
        }

        internal static Exception DatabaseNotWritable(string modelName, string itemJson)
        {
            DatabaseException ex = new DatabaseException(ErrorCodes.DatabaseNotWriteable, nameof(DatabaseNotWritable), null, null);

            ex.Data["ModelName"] = modelName;
            ex.Data["Item"] = itemJson;

            return ex;
        }

        internal static Exception ChangedPropertyPackError(string cause, ChangedPack? changedPropertyPack, string? modelFullName)
        {
            DatabaseException ex = new DatabaseException(ErrorCodes.ChangedPackError, cause, null, null);

            ex.Data["ModelFullName"] = modelFullName;
            ex.Data["PropertyNames"] = changedPropertyPack?.ChangedProperties.Select(c => c.PropertyName).ToJoinedString(",");

            return ex;
        }

        internal static Exception DuplicateKeyError(string commandText, Exception? innerException)
        {
            DatabaseException ex = new DatabaseException(ErrorCodes.DuplicateKeyEntry, "DuplicateKeyError", innerException, new { CommandText = commandText });
            ex.ComeFromEngine = true;
            return ex;
        }

        internal static Exception DbSettingError(string? dbName, string? dbKind, string cause)
        {
            DatabaseException exception = new DatabaseException(ErrorCodes.DbSettingError, cause, null, null);

            exception.Data["DbKind"] = dbKind;
            exception.Data["DbName"] = dbName;

            return exception;
        }
    }
}