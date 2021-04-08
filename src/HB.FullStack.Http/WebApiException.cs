using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.WebApi
{
    public class WebApiException : ErrorCodeException
    {
        public WebApiException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public WebApiException(ErrorCode errorCode, Exception? innerException) : base(errorCode, innerException)
        {
        }
    }
}
