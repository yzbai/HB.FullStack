#nullable enable

using System.Runtime.Serialization;

using HB.FullStack.Database;

namespace System
{
    public class DatabaseException : ErrorCodeException
    {
        public DatabaseException(ErrorCode eventCode) : base(eventCode)
        {
        }

        public DatabaseException(ErrorCode eventCode, Exception? innerException) : base(eventCode, innerException)
        {
        }

    }
}