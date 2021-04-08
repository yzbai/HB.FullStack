using System;
using System.Runtime.CompilerServices;

using HB.FullStack.Database.Def;
using HB.FullStack.Database.SQL;

[assembly: InternalsVisibleTo("HB.Infrastructure.MySQL")]
[assembly: InternalsVisibleTo("HB.Infrastructure.SQLite")]
namespace HB.FullStack.Database
{
    /// <summary>
    /// from 1000 ~ 1999
    /// </summary>
    internal static class DatabaseErrorCodes
    {
        public static ErrorCode ExecuterError { get; set; } = new ErrorCode(1001, nameof(ExecuterError), "");
        public static ErrorCode Unkown { get; set; } = new ErrorCode(1002, nameof(Unkown), "");
        public static ErrorCode UseDateTimeOffsetOnly { get; set; } = new ErrorCode(1007, nameof(UseDateTimeOffsetOnly), "");
        public static ErrorCode EntityError { get; set; } = new ErrorCode(1009, nameof(EntityError), "");
        public static ErrorCode MapperError { get; set; } = new ErrorCode(1013, nameof(MapperError), "");
        public static ErrorCode SqlError { get; set; } = new ErrorCode(1014, nameof(SqlError), "");
        public static ErrorCode DatabaseTableCreateError { get; set; } = new ErrorCode(1016, nameof(DatabaseTableCreateError), "");
        public static ErrorCode MigrateError { get; set; } = new ErrorCode(1017, nameof(MigrateError), "");
        public static ErrorCode FoundTooMuch { get; set; } = new ErrorCode(1018, nameof(FoundTooMuch), "");
        public static ErrorCode DatabaseNotWriteable { get; set; } = new ErrorCode(1019, nameof(DatabaseNotWriteable), "");
        public static ErrorCode NotFound { get; set; } = new ErrorCode(1020, nameof(NotFound), "");
        public static ErrorCode TransactionError { get; set; } = new ErrorCode(1021, nameof(TransactionError), "");
        public static ErrorCode SystemInfoError { get; set; } = new ErrorCode(1024, nameof(SystemInfoError), "");
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
    }
}