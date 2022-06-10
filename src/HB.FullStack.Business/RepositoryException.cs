using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class RepositoryException : ErrorCode2Exception
    {
        public RepositoryException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public RepositoryException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public RepositoryException()
        {
        }

        public RepositoryException(string message) : base(message)
        {
        }

        public RepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
