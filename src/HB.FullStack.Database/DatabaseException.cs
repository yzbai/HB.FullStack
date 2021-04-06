#nullable enable

using System.Runtime.Serialization;

using HB.FullStack.Database;

namespace System
{
    public class DatabaseException : EventCodeException
    {
        public DatabaseException(EventCode eventCode) : base(eventCode)
        {
        }

        public DatabaseException(EventCode eventCode, Exception? innerException) : base(eventCode, innerException)
        {
        }

        public DatabaseException(EventCode eventCode, string? message) : base(eventCode, message)
        {
        }

        public DatabaseException(EventCode eventCode, string? message, Exception? innerException) : base(eventCode, message, innerException)
        {
        }
    }
}