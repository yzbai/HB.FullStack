using System;
using System.Runtime.CompilerServices;
using System.Text;

using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.SQL;

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
        public static ErrorCode ExecuterError { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 0, nameof(ExecuterError), "");
        public static ErrorCode Unkown { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 1, nameof(Unkown), "");
        public static ErrorCode UseDateTimeOffsetOnly { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 2, nameof(UseDateTimeOffsetOnly), "");
        public static ErrorCode EntityError { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 3, nameof(EntityError), "");
        public static ErrorCode MapperError { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 4, nameof(MapperError), "");
        public static ErrorCode SqlError { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 5, nameof(SqlError), "");
        public static ErrorCode DatabaseTableCreateError { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 6, nameof(DatabaseTableCreateError), "");
        public static ErrorCode MigrateError { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 7, nameof(MigrateError), "");
        public static ErrorCode FoundTooMuch { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 8, nameof(FoundTooMuch), "");
        public static ErrorCode DatabaseNotWriteable { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 9, nameof(DatabaseNotWriteable), "");
        public static ErrorCode NotFound { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 10, nameof(NotFound), "");
        public static ErrorCode TransactionError { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 11, nameof(TransactionError), "");
        public static ErrorCode SystemInfoError { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 12, nameof(SystemInfoError), "");
        public static ErrorCode NotSupported { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 13, nameof(NotSupported), "");
        public static ErrorCode BatchError { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 14, nameof(BatchError), "");
        public static ErrorCode TypeConverterError { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 15, nameof(TypeConverterError), "");
        public static ErrorCode EmptyGuid { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 16, nameof(EmptyGuid), "");
        public static ErrorCode UpdatePropertiesCountShouldBePositive { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 17, nameof(UpdatePropertiesCountShouldBePositive), "");
        public static ErrorCode LongIdShouldBePositive { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 18, nameof(LongIdShouldBePositive));
        public static ErrorCode PropertyNotFound { get; } = new ErrorCode(ErrorCodeStartIds.DATABASE + 19, nameof(PropertyNotFound));
    }

    internal static class Exceptions
    {
        internal static Exception VersionShouldBePositive(int wrongVersion)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.SystemInfoError);

            exception.Data["WrongVersion"] = wrongVersion;
            exception.Data["Cause"] = "Version Should Be Positive";

            return exception;
        }

        internal static Exception ExecuterError(string commandText, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.ExecuterError, innerException);

            exception.Data["CommandText"] = commandText;

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

        internal static Exception FoundTooMuch(string type, string? from, string? where)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.FoundTooMuch);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception FoundTooMuch(string type, string item)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.FoundTooMuch);

            exception.Data["Type"] = type;
            exception.Data["Item"] = item;

            return exception; ;
        }

        internal static Exception PropertyNotFound(string type, string property)
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

        internal static Exception NotFound(string type, string item, string? cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.NotFound);

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

        internal static Exception EntityHasNotSupportedPropertyType(string type, string propertyTypeName, string propertyName)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.EntityError);

            exception.Data["Type"] = type;
            exception.Data["PropertyTypeName"] = propertyTypeName;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception EntityError(string type, string propertyName, string cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.EntityError);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = cause;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception EntityVersionError(string type, int version, string cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseErrorCodes.EntityError);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = "Version Error. - " + cause;
            exception.Data["Version"] = version;

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

        internal static Exception TypeConverterError(string cause, string typeFullName)
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
    }
}