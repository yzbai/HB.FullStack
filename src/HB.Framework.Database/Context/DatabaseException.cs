using System;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HB.Framework.Database
{
    public class DatabaseException : DbException
    {
        public DatabaseException()
        {
        }

        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DatabaseException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}
