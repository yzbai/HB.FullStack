#nullable enable

using HB.FullStack.Database;

namespace System
{
    public class DatabaseException : Exception
    {

        public DatabaseErrorCode ErrorCode { get; set; }
        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public DatabaseException(DatabaseErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public DatabaseException(DatabaseErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public DatabaseException(DatabaseErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}