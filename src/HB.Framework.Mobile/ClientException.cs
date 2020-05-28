using System;

namespace HB.Framework.Client
{
    public class ClientException : FrameworkException
    {
        public override FrameworkExceptionType ExceptionType { get => FrameworkExceptionType.Client; }
        public ClientException(string message) : base(message)
        {
        }

        public ClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ClientException()
        {
        }
    }
}
