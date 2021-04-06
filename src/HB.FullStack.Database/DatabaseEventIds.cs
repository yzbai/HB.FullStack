using System;

using HB.FullStack.Database.Def;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    /// <summary>
    /// from 1000 ~ 1999
    /// </summary>
    internal static class EventCodes
    {
        /// <summary>
        /// 
        /// </summary>
        public static EventCode ExecuterError = new EventCode(1001, nameof(ExecuterError), "");
        public static EventCode Unkown = new EventCode(1002, nameof(Unkown), "");
        public static EventCode UseDateTimeOffsetOnly = new EventCode(1007, nameof(UseDateTimeOffsetOnly), "");
        public static EventCode EntityError = new EventCode(1009, nameof(EntityError), "");
        public static EventCode MapperError = new EventCode(1013, nameof(MapperError), "");
        public static EventCode SqlError = new EventCode(1014, nameof(SqlError), "");
        public static EventCode DatabaseTableCreateError = new EventCode(1016, nameof(DatabaseTableCreateError), "");
        public static EventCode MigrateError = new EventCode(1017, nameof(MigrateError), "");
        public static EventCode FoundTooMuch = new EventCode(1018, nameof(FoundTooMuch), "");
        public static EventCode DatabaseNotWriteable = new EventCode(1019, nameof(DatabaseNotWriteable), "");
        public static EventCode NotFound = new EventCode(1020, nameof(NotFound), "");
        public static EventCode TransactionError = new EventCode(1021, nameof(TransactionError), "");
        public static EventCode SystemInfoError = new EventCode(1024, nameof(SystemInfoError), "");
    }

    public static class Exceptions
    {
        public static Exception VersionShouldBePositive(int wrongVersion)
        {
            DatabaseException exception = new DatabaseException(EventCodes.SystemInfoError);

            exception.Data["WrongVersion"] = wrongVersion;
            exception.Data["Cause"] = "Version Should Be Positive";

            return exception;
        }

        public static Exception ExecuterError(string commandText, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(EventCodes.ExecuterError, innerException);
            
            exception.Data["CommandText"] = commandText;

            return exception;
        }

        internal static Exception TransactionError(string cause, string? callerMemberName, int callerLineNumber)
        {
            DatabaseException exception = new DatabaseException(EventCodes.TransactionError);
            exception.Data["CallerMemeberName"] = callerMemberName;
            exception.Data["CallerLineNumber"] = callerLineNumber;
            exception.Data["Cause"] = cause;

            return exception;
        }

        public static Exception TransactionConnectionIsNull(string commandText)
        {
            DatabaseException exception = new DatabaseException(EventCodes.TransactionError);
            exception.Data["CommandText"] = commandText;
            exception.Data["Cause"] = "Connection Is Null";

            return exception;
        }

        internal static Exception TableCreateError(int version, string databaseName, string cause, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(EventCodes.DatabaseTableCreateError, cause, innerException);

            exception.Data["DatabaseVersion"] = version;
            exception.Data["DatabaseName"] = databaseName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception MigrateError(string databaseName, string cause, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(EventCodes.MigrateError, cause, innerException);

            exception.Data["DatabaseName"] = databaseName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception FoundTooMuch(string type, string? from, string? where)
        {
            DatabaseException exception = new DatabaseException(EventCodes.FoundTooMuch);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception FoundTooMuch(string type, string item)
        {
            DatabaseException exception = new DatabaseException(EventCodes.FoundTooMuch);

            exception.Data["Type"] = type;
            exception.Data["Item"] = item;

            return exception; ;
        }

        internal static Exception UnKown(string type, string? from, string? where, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(EventCodes.Unkown, innerException);

            exception.Data["Type"] = type;
            exception.Data["From"] = from;
            exception.Data["Where"] = where;

            return exception;
        }

        internal static Exception UnKown(string type, string item, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(EventCodes.Unkown, innerException);

            exception.Data["Type"] = type;
            exception.Data["item"] = item;

            return exception;
        }

        internal static Exception NotFound(string type, string item, string? cause)
        {
            DatabaseException exception = new DatabaseException(EventCodes.NotFound);

            exception.Data["Type"] = type;
            exception.Data["item"] = item;
            exception.Data["Cause"] = cause;

            return exception;
        }
        
        

        internal static Exception NotWriteable(string type, string database)
        {
            DatabaseException exception = new DatabaseException(EventCodes.DatabaseNotWriteable);

            exception.Data["Type"] = type;
            exception.Data["Database"] = database;

            return exception;
        }

        internal static Exception SystemInfoError(string cause)
        {
            DatabaseException exception = new DatabaseException(EventCodes.SystemInfoError);

            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception UseDateTimeOffsetOnly()
        {
            DatabaseException exception = new DatabaseException(EventCodes.UseDateTimeOffsetOnly);


            return exception;
        }

        internal static Exception EntityHasNotSupportedPropertyType(string type, string propertyTypeName, string propertyName)
        {
            DatabaseException exception = new DatabaseException(EventCodes.EntityError);

            exception.Data["Type"] = type;
            exception.Data["PropertyTypeName"] = propertyTypeName;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception EntityError(string type, string propertyName, string cause)
        {
            DatabaseException exception = new DatabaseException(EventCodes.EntityError);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = cause;
            exception.Data["ProeprtyName"] = propertyName;

            return exception;
        }

        internal static Exception EntityVersionError(string type, int version, string cause)
        {
            DatabaseException exception = new DatabaseException(EventCodes.EntityError);

            exception.Data["Type"] = type;
            exception.Data["Cause"] = "Version Error. - " + cause;
            exception.Data["Version"] = version;

            return exception;
        }

        internal static Exception MapperError(Exception innerException)
        {
            DatabaseException exception = new DatabaseException(EventCodes.MapperError, innerException);

            return exception;
        }

        internal static Exception SqlJoinTypeMixedError()
        {
            DatabaseException exception = new DatabaseException(EventCodes.SqlError);

            exception.Data["Cause"] = "Sql join type mixed.";

            return exception;
        }

        
    }
}