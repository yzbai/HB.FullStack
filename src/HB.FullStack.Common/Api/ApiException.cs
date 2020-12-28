using System.Collections.Generic;
using System.Net;

namespace System
{
    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<Pending>")]
    public class ApiException : FrameworkException
    {
        public HttpStatusCode HttpCode { get; }

        //public ApiException(string? message) : base(message)
        //{
        //}

        //public ApiException(string? message, Exception? innerException) : base(message, innerException)
        //{
        //}

        //public ApiException()
        //{
        //}

        public ApiException(ErrorCode errorCode, HttpStatusCode httpCode, string? message = null) : base(errorCode, message)
        {
            HttpCode = httpCode;
        }

        public ApiException(ErrorCode errorCode, HttpStatusCode httpCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
            HttpCode = httpCode;
        }

        public ApiException(ErrorCode errorCode, HttpStatusCode httpCode, string? message, IDictionary<string, IEnumerable<string>>? modelStates = null) : this(errorCode, httpCode, message)
        {
            ModelStates = modelStates;
        }

        public IDictionary<string, IEnumerable<string>>? ModelStates { get; set; }
    }
}