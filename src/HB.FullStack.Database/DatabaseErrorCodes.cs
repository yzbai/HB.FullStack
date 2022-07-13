using System;
using System.Runtime.CompilerServices;

using HB.FullStack.Database.Engine;
using HB.FullStack.Database.DatabaseModels;

[assembly: InternalsVisibleTo("HB.Infrastructure.MySQL")]
[assembly: InternalsVisibleTo("HB.Infrastructure.SQLite")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]

namespace HB.FullStack.Database
{
    /// <summary>
    /// from 1000 ~ 1999
    /// </summary>
    internal static class DatabaseErrorCodes
    {
        public static ErrorCode ExecuterError { get; } = new ErrorCode( nameof(ExecuterError), "");
        public static ErrorCode Unkown { get; } = new ErrorCode( nameof(Unkown), "");
        public static ErrorCode UseDateTimeOffsetOnly { get; } = new ErrorCode( nameof(UseDateTimeOffsetOnly), "");
        public static ErrorCode ModelError { get; } = new ErrorCode( nameof(ModelError), "");
        public static ErrorCode MapperError { get; } = new ErrorCode( nameof(MapperError), "");
        public static ErrorCode SqlError { get; } = new ErrorCode( nameof(SqlError), "");
        public static ErrorCode DatabaseTableCreateError { get; } = new ErrorCode( nameof(DatabaseTableCreateError), "");
        public static ErrorCode MigrateError { get; } = new ErrorCode( nameof(MigrateError), "");
        public static ErrorCode FoundTooMuch { get; } = new ErrorCode( nameof(FoundTooMuch), "");
        public static ErrorCode DatabaseNotWriteable { get; } = new ErrorCode( nameof(DatabaseNotWriteable), "");
        public static ErrorCode ConcurrencyConflict { get; } = new ErrorCode( nameof(ConcurrencyConflict), "");
        public static ErrorCode TransactionError { get; } = new ErrorCode( nameof(TransactionError), "");
        public static ErrorCode SystemInfoError { get; } = new ErrorCode( nameof(SystemInfoError), "");
        public static ErrorCode NotSupported { get; } = new ErrorCode( nameof(NotSupported), "");
        public static ErrorCode BatchError { get; } = new ErrorCode( nameof(BatchError), "");
        public static ErrorCode TypeConverterError { get; } = new ErrorCode( nameof(TypeConverterError), "");
        public static ErrorCode EmptyGuid { get; } = new ErrorCode( nameof(EmptyGuid), "");
        public static ErrorCode UpdatePropertiesCountShouldBePositive { get; } = new ErrorCode( nameof(UpdatePropertiesCountShouldBePositive), "");
        public static ErrorCode LongIdShouldBePositive { get; } = new ErrorCode( nameof(LongIdShouldBePositive), "");
        public static ErrorCode PropertyNotFound { get; } = new ErrorCode( nameof(PropertyNotFound), "");
        public static ErrorCode NoSuchForeignKey { get; } = new ErrorCode( nameof(NoSuchForeignKey), "");

        public static ErrorCode NoSuchProperty { get; } = new ErrorCode( nameof(NoSuchProperty), "");
        public static ErrorCode KeyValueNotLongOrGuid { get; } = new ErrorCode( nameof(KeyValueNotLongOrGuid), "");

        public static ErrorCode ModelHasNotSupportedPropertyType { get; } = new ErrorCode( nameof(ModelHasNotSupportedPropertyType), "");

        public static ErrorCode ModelTimestampError { get; } = new ErrorCode( nameof(ModelTimestampError), "");
        public static ErrorCode NotInitializedYet { get; } = new ErrorCode( nameof(NotInitializedYet), "");
        public static ErrorCode UpdateVersionError { get; } = new ErrorCode(nameof(UpdateVersionError), "");
    }

    internal static class DatabaseExceptions
    {
        internal static Exception TimestampShouldBePositive(long timestamp)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.ModelTimestampError);

            exception.Data["WrongTimestamp"] = timestamp;
            exception.Data["Cause"] = "Timestamp Should Be Positive";

            return exception;
        }

        internal static Exception MySQLExecuterError(string? commandText, string? cause, string? sqlState, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.ExecuterError, innerException);

            exception.Data["Cause"] = cause;
            exception.Data["CommandText"] = commandText;
            exception.Data["SqlState"] = sqlState;

            return exception;
        }

        internal static Exception SQLiteExecuterError(string? commandText, string? cause, int? errorCode, int? extendedErrorCode, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.ExecuterError, innerException);

            exception.Data["CommandText"] = commandText;
            exception.Data["Cause"] = cause;
            exception.Data["ErrorCode"] = errorCode;
            exception.Data["ExtendedErrorCode"] = extendedErrorCode;

            return exception;
        }

        internal static Exception TransactionError(string cause, string? callerMemberName, int callerLineNumber)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.TransactionError);
            exception.Data["CallerMemeberName"] = callerMemberName;
            exception.Data["CallerLineNumber"] = callerLineNumber;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception TransactionConnectionIsNull(string commandText)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.TransactionError);
            exception.Data["CommandText"] = commandText;
            exception.Data["Cause"] = "Connection Is Null";

            return exception;
        }

        internal static Exception TableCreateError(int version, string databaseName, string cause, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.DatabaseTableCreateError, innerException);

            exception.Data["DatabaseVersion"] = version;
            exception.Data["DatabaseName"] = databaseName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception MigrateError(string databaseName, string cause, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.MigrateError, innerException);

            exception.Data["DatabaseName"] = databaseName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception FoundTooMuch(string? type, string? from, string? where)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.FoundTooMuch);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception FoundTooMuch(string? type, string item)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.FoundTooMuch);

            exception.Data["Type"] = type;
            exception.Data["Item"] = item;

            return exception; ;
        }

        internal static Exception PropertyNotFound(string? type, string property)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.PropertyNotFound);
            exception.Data["Type"] = type;
            exception.Data["Property"] = property;

            return exception;
        }

        internal static Exception UnKown(string type, string? from, string? where, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.Unkown, innerException);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception UnKown(string type, string item, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.Unkown, innerException);

            exception.Data["Type"] = type;
            exception.Data["item"] = item;

            return exception;
        }

        internal static Exception ConcurrencyConflict(string type, string item, string? cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.ConcurrencyConflict);

            exception.Data["Type"] = type;
            exception.Data["item"] = item;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception NotWriteable(string type, string database)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.DatabaseNotWriteable);

            exception.Data["Type"] = type;
            exception.Data["Database"] = database;

            return exception;
        }

        internal static Exception SystemInfoError(string cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.SystemInfoError);

            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception UseDateTimeOffsetOnly()
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.UseDateTimeOffsetOnly);

            return exception;
        }

        internal static Exception ModelHasNotSupportedPropertyType(string type, string? propertyTypeName, string propertyName)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.ModelHasNotSupportedPropertyType);

            exception.Data["Type"] = type;
            exception.Data["PropertyTypeName"] = propertyTypeName;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception ModelError(string? type, string propertyName, string cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.ModelError);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = cause;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception ModelVersionError(string type, long timestamp, string cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.ModelTimestampError);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = "Version Error. - " + cause;
            exception.Data["Timestamp"] = timestamp;

            return exception;
        }

        internal static Exception MapperError(Exception innerException)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.MapperError, innerException);

            return exception;
        }

        internal static Exception SqlJoinTypeMixedError()
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.SqlError);

            exception.Data["Cause"] = "Sql join type mixed.";

            return exception;
        }

        internal static Exception NotSupportYet(string cause, EngineType engineType)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.NotSupported);
            exception.Data["Cause"] = cause;
            exception.Data["EngineType"] = engineType.ToString();

            return exception;
        }

        internal static Exception TooManyForBatch(string cause, int count, string lastUser)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.BatchError);
            exception.Data["Cause"] = cause;
            exception.Data["Count"] = count;
            exception.Data["LastUser"] = lastUser;

            return exception;
        }

        internal static Exception TypeConverterError(string cause, string? typeFullName)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.TypeConverterError);
            exception.Data["Cause"] = cause;
            exception.Data["Type"] = typeFullName;

            return exception;
        }

        internal static Exception GuidShouldNotEmpty()
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.EmptyGuid);

            return exception;
        }

        internal static Exception UpdatePropertiesCountShouldBePositive()
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.UpdatePropertiesCountShouldBePositive);

            return exception;
        }

        internal static Exception LongIdShouldBePositive(long id)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.LongIdShouldBePositive);
            exception.Data["Id"] = id;

            return exception;
        }

        internal static Exception NoSuchForeignKey(string modelFullName, string foreignKeyName)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.NoSuchForeignKey);
            exception.Data["ModelFullName"] = modelFullName;
            exception.Data["ForeignKeyName"] = foreignKeyName;

            return exception;
        }

        internal static Exception NoSuchProperty(string modelFullName, string propertyName)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.NoSuchProperty);
            exception.Data["ModelFullName"] = modelFullName;
            exception.Data["PropertyName"] = propertyName;
            return exception;
        }

        internal static Exception KeyValueNotLongOrGuid(string modelFullName, string foreignKeyName, object? foreignKeyValue, string? foreignKeyType)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.KeyValueNotLongOrGuid);
            exception.Data["ModelFullName"] = modelFullName;
            exception.Data["ForeignKeyName"] = foreignKeyName;
            exception.Data["ForeignKeyValue"] = foreignKeyValue?.ToString();
            exception.Data["ForeignKeyType"] = foreignKeyType;

            return exception;
        }

        internal static Exception NotInitializedYet()
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.NotInitializedYet);
            return exception;
        }

        internal static Exception UpdateVersionError<T>(int originalVersion, int updateToVersion, T item) where T : DatabaseModel, new()
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.UpdateVersionError);

            exception.Data["OriginalVersion"] = originalVersion;
            exception.Data["UpdateToVersion"] = updateToVersion;
            exception.Data["Item"] = SerializeUtil.ToJson(item);

            return exception;
        }

        internal static Exception DatabaseNotWritable(string modelName, string itemJson)
        {
            DatabaseException ex = new DatabaseException(DatabaseErrorCodes.DatabaseNotWriteable);

            ex.Data["ModelName"] = modelName;
            ex.Data["Item"] = itemJson;

            return ex;
        }
    }
}