#nullable enable

using System.Runtime.Serialization;

using HB.FullStack.Database;

namespace System
{
    public class DatabaseException : ErrorCode2Exception
    {
        public DatabaseException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public DatabaseException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public DatabaseException()
        {
        }

        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}