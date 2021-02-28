using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class RepositoryException: Exception
    {

        public RepositoryErrorCode ErrorCode { get; set; }

        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public RepositoryException(RepositoryErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public RepositoryException(RepositoryErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public RepositoryException(RepositoryErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
