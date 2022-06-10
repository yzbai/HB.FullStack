using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.WebApi
{
    public class WebApiException : ErrorCode2Exception
    {
        public WebApiException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public WebApiException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }

        public WebApiException()
        {
        }

        public WebApiException(string message) : base(message)
        {
        }

        public WebApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
