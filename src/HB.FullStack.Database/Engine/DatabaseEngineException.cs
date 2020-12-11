#nullable enable

using System;

namespace HB.FullStack.Database
{
    public class DatabaseEngineException : DatabaseException
    {
        public DatabaseEngineException()
        {
        }

        public DatabaseEngineException(string? message) : base(message)
        {
        }

        public DatabaseEngineException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public DatabaseEngineException(ErrorCode errorCode, string? whoEntityName = null, string? detail = null, Exception? innerException = null) : base(errorCode, whoEntityName, detail, innerException)
        {
        }
    }
}
