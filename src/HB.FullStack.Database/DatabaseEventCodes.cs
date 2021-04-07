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
    internal static class DatabaseEventCodes
    {
        public static EventCode ExecuterError { get; set; } = new EventCode(1001, nameof(ExecuterError), "");
        public static EventCode Unkown { get; set; } = new EventCode(1002, nameof(Unkown), "");
        public static EventCode UseDateTimeOffsetOnly { get; set; } = new EventCode(1007, nameof(UseDateTimeOffsetOnly), "");
        public static EventCode EntityError { get; set; } = new EventCode(1009, nameof(EntityError), "");
        public static EventCode MapperError { get; set; } = new EventCode(1013, nameof(MapperError), "");
        public static EventCode SqlError { get; set; } = new EventCode(1014, nameof(SqlError), "");
        public static EventCode DatabaseTableCreateError { get; set; } = new EventCode(1016, nameof(DatabaseTableCreateError), "");
        public static EventCode MigrateError { get; set; } = new EventCode(1017, nameof(MigrateError), "");
        public static EventCode FoundTooMuch { get; set; } = new EventCode(1018, nameof(FoundTooMuch), "");
        public static EventCode DatabaseNotWriteable { get; set; } = new EventCode(1019, nameof(DatabaseNotWriteable), "");
        public static EventCode NotFound { get; set; } = new EventCode(1020, nameof(NotFound), "");
        public static EventCode TransactionError { get; set; } = new EventCode(1021, nameof(TransactionError), "");
        public static EventCode SystemInfoError { get; set; } = new EventCode(1024, nameof(SystemInfoError), "");
    }

    internal static class Exceptions
    {
        internal static Exception VersionShouldBePositive(int wrongVersion)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.SystemInfoError);

            exception.Data["WrongVersion"] = wrongVersion;
            exception.Data["Cause"] = "Version Should Be Positive";

            return exception;
        }

        internal static Exception ExecuterError(string commandText, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.ExecuterError, innerException);

            exception.Data["CommandText"] = commandText;

            return exception;
        }

        internal static Exception TransactionError(string cause, string? callerMemberName, int callerLineNumber)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.TransactionError);
            exception.Data["CallerMemeberName"] = callerMemberName;
            exception.Data["CallerLineNumber"] = callerLineNumber;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception TransactionConnectionIsNull(string commandText)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.TransactionError);
            exception.Data["CommandText"] = commandText;
            exception.Data["Cause"] = "Connection Is Null";

            return exception;
        }

        internal static Exception TableCreateError(int version, string databaseName, string cause, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.DatabaseTableCreateError, innerException);

            exception.Data["DatabaseVersion"] = version;
            exception.Data["DatabaseName"] = databaseName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception MigrateError(string databaseName, string cause, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.MigrateError, innerException);

            exception.Data["DatabaseName"] = databaseName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception FoundTooMuch(string type, string? from, string? where)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.FoundTooMuch);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception FoundTooMuch(string type, string item)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.FoundTooMuch);

            exception.Data["Type"] = type;
            exception.Data["Item"] = item;

            return exception; ;
        }

        internal static Exception UnKown(string type, string? from, string? where, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.Unkown, innerException);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception UnKown(string type, string item, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.Unkown, innerException);

            exception.Data["Type"] = type;
            exception.Data["item"] = item;

            return exception;
        }

        internal static Exception NotFound(string type, string item, string? cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.NotFound);

            exception.Data["Type"] = type;
            exception.Data["item"] = item;
            exception.Data["Cause"] = cause;

            return exception;
        }



        internal static Exception NotWriteable(string type, string database)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.DatabaseNotWriteable);

            exception.Data["Type"] = type;
            exception.Data["Database"] = database;

            return exception;
        }

        internal static Exception SystemInfoError(string cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.SystemInfoError);

            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception UseDateTimeOffsetOnly()
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.UseDateTimeOffsetOnly);


            return exception;
        }

        internal static Exception EntityHasNotSupportedPropertyType(string type, string propertyTypeName, string propertyName)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.EntityError);

            exception.Data["Type"] = type;
            exception.Data["PropertyTypeName"] = propertyTypeName;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception EntityError(string type, string propertyName, string cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.EntityError);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = cause;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception EntityVersionError(string type, int version, string cause)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.EntityError);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = "Version Error. - " + cause;
            exception.Data["Version"] = version;

            return exception;
        }

        internal static Exception MapperError(Exception innerException)
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.MapperError, innerException);

            return exception;
        }

        internal static Exception SqlJoinTypeMixedError()
        {
            DatabaseException exception = new DatabaseException(DatabaseEventCodes.SqlError);

            exception.Data["Cause"] = "Sql join type mixed.";

            return exception;
        }
    }
}