namespace System
{
    public class ErrorCodeException : Exception
    {
        public ErrorCodeException(ErrorCode errorCode, string cause, Exception? innerException = null, object? context = null) : base(cause, innerException)
        {
            ErrorCode = errorCode;

            if (context != null)
            {
                Context = context;
            }
        }

        public ErrorCode ErrorCode
        {
            get
            {
                return (ErrorCode?)Data[nameof(ErrorCode)]!;
            }
            private set
            {
                Data[nameof(ErrorCode)] = value;
            }
        }

        //TODO: 这会引起Memory Leack吗？
        //TODO: 有必要吗？还是直接用Data就行

        /// <summary>
        /// 异常发生时，上下文数据
        /// </summary>
        public object? Context
        {
            get { return Data[nameof(Context)]; }
            set { Data[nameof(Context)] = value; }
        }

        [Obsolete("不要用")]
        public ErrorCodeException()
        {
        }

        [Obsolete("不要用")]
        public ErrorCodeException(string? cause) : base(cause)
        {
        }

        [Obsolete("不要用")]
        public ErrorCodeException(string? cause, Exception innerException) : base(cause, innerException)
        {
        }
    }
}