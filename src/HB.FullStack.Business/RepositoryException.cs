using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class RepositoryException : ErrorCodeException
    {
        public RepositoryException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public RepositoryException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}
