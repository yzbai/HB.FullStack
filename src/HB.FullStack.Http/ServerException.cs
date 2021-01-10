using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Server
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<Pending>")]
    public class ServerException : Exception
    {
        public ServerErrorCode ErrorCode { get; set; }
        public override string Message => $"ErrorCode:{ErrorCode}, Message:{base.Message}";


        public ServerException(ServerErrorCode errorCode) : base()
        {
            ErrorCode = errorCode;
        }

        public ServerException(ServerErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ServerException(ServerErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
