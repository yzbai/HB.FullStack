namespace System
{
    public class ClientException : Exception
    {
        public ClientException(ClientErrorCode errorCode, string? message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ClientException(ClientErrorCode errorCode, string? message, Exception? innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public ClientException(ClientErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }

        public ClientErrorCode ErrorCode { get; set; }

        public ClientException()
        {
        }

        public ClientException(string message) : base(message)
        {
        }

        public ClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}