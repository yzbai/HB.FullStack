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
        public static EventCode DatabaseTransactionConnectionIsNull = new EventCode(1000, nameof(DatabaseTransactionConnectionIsNull), "");
        public static EventCode DatabaseExecuterError = new EventCode(1001, nameof(DatabaseExecuterError), "");
        public static EventCode DatabaseError = new EventCode(1002, nameof(DatabaseError), "");
        public static EventCode DatabaseNameNotFoundInSystemInfoTable = new EventCode(1003, nameof(DatabaseNameNotFoundInSystemInfoTable), "");
        public static EventCode VersionNotFoundInSystemInfoTable = new EventCode(1004, nameof(VersionNotFoundInSystemInfoTable), "");
        public static EventCode MigrateOldVersionErrorMessage = new EventCode(1005, nameof(MigrateOldVersionErrorMessage), "");
        public static EventCode MigrateVersionStepErrorMessage = new EventCode(1006, nameof(MigrateVersionStepErrorMessage), "");
        public static EventCode UseDateTimeOffsetOnly = new EventCode(1007, nameof(UseDateTimeOffsetOnly), "");
        public static EventCode DatabaseUnSupported = new EventCode(1008, nameof(DatabaseUnSupported), "");
        public static EventCode NotADatabaseEntity = new EventCode(1009, nameof(NotADatabaseEntity), "");
        public static EventCode DatabaseDefError = new EventCode(1010, nameof(DatabaseDefError), "");
        public static EventCode DatabaseVersionNotSet = new EventCode(1011, nameof(DatabaseVersionNotSet), "");
        public static EventCode LackPropertyDef = new EventCode(1012, nameof(LackPropertyDef), "");
        public static EventCode EmitEntityMapperError = new EventCode(1013, nameof(EmitEntityMapperError), "");
        public static EventCode SqlJoinTypeMixedErrorMessage = new EventCode(1014, nameof(SqlJoinTypeMixedErrorMessage), "");
        public static EventCode VersionShouldBePositive = new EventCode(1015, nameof(VersionShouldBePositive), "");
        public static EventCode DatabaseTableCreateError = new EventCode(1016, nameof(DatabaseTableCreateError), "");
        public static EventCode DatabaseMigrateError = new EventCode(1017, nameof(DatabaseMigrateError), "");
        public static EventCode DatabaseFoundTooMuch = new EventCode(1018, nameof(DatabaseFoundTooMuch), "");
        public static EventCode DatabaseNotWriteable = new EventCode(1019, nameof(DatabaseNotWriteable), "");
        public static EventCode DatabaseNotFound = new EventCode(1020, nameof(DatabaseNotFound), "");
        public static EventCode DatabaseTransactionError = new EventCode(1021, nameof(DatabaseTransactionError), "");
        public static EventCode DatabaseTransactionAlreadyFinished = new EventCode(1022, nameof(DatabaseTransactionAlreadyFinished), "");
        public static EventCode DatabaseInitLockError = new EventCode(1023, nameof(DatabaseInitLockError), "");
    }

    public static class Exceptions
    {
        public static Exception VersionShouldBePositive(int wrongVersion)
        {
            DatabaseException exception = new DatabaseException(EventCodes.VersionShouldBePositive);

            exception.Data["WrongVersion"] = wrongVersion;

            return exception;
        }

        public static Exception DatabaseExecuterError(string commandText, Exception innerException)
        {
            throw new NotImplementedException();
        }

        internal static Exception DatabaseTransactionAlreadyFinished(string? callerMemberName, int callerLineNumber)
        {
            DatabaseException exception = new DatabaseException(EventCodes.DatabaseTransactionAlreadyFinished);
            exception.Data["CallerMemeberName"] = callerMemberName;
            exception.Data["CallerLineNumber"] = callerLineNumber;

            return exception;
        }

        internal static Exception DatabaseTableCreateError(int version, string databaseName, string cause, Exception? innerException = null)
        {
            DatabaseException exception = new DatabaseException(EventCodes.DatabaseTableCreateError, cause, innerException);

            exception.Data["DatabaseVersion"] = version;
            exception.Data["DatabaseName"] = databaseName;
            exception.Data["Cause"] = cause;

            return exception;
        }

        internal static Exception DatabaseMigrateError(string databaseName, string cause, Exception? innerException = null)
        {
            return new NotImplementedException();
        }

        internal static Exception DatabaseFoundTooMuch<T>(string type, FromExpression<T>? from, WhereExpression<T>? Where) where T : DatabaseEntity, new()
        {
            return new NotImplementedException();
        }

        internal static Exception UnKown<TFrom, TWhere>(string type, FromExpression<TFrom>? from, WhereExpression<TWhere>? where, Exception? innnerException = null)
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
        {
            return new NotImplementedException();
        }

        internal static Exception UnKown(string type, string item, Exception? innerException = null)
        {
            throw new NotImplementedException();
        }

        internal static Exception NotFound(string type, string item)
        {
            throw new NotImplementedException();
        }

        internal static Exception DatabaseFoundTooMuch(string entityFullName, string item)
        {
            throw new NotImplementedException();
        }

        internal static Exception NotWriteable(string type, string database)
        {
            throw new NotImplementedException();
        }

        internal static Exception NotFound(string entityFullName, string item, string cause)
        {
            throw new NotImplementedException();
        }

        internal static Exception SystemInfoError(string cause)
        {
            throw new NotImplementedException();
        }

        internal static Exception UseDateTimeOffsetOnly()
        {
            throw new NotImplementedException();
        }

        internal static Exception NotSupported(string type, Type propertyType, string property)
        {
            throw new NotImplementedException();
        }

        internal static Exception EntityError(string type, string cause)
        {
            throw new NotImplementedException();
        }

        internal static Exception EntityError(string type, string propertyName, string cause)
        {
            throw new NotImplementedException();
        }

        internal static Exception EntityVersionError(string type, int version, string cause)
        {
            throw new NotImplementedException();
        }

        internal static Exception MapperError(Exception innerException)
        {
            throw new NotImplementedException();
        }

        internal static Exception SqlJoinTypeMixedError()
        {
            throw new NotImplementedException();
        }

        public static Exception TransactionConnectionIsNull(string commandText)
        {
            throw new NotImplementedException();
        }
    }
}