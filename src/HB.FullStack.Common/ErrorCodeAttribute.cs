namespace System
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ErrorCodeAttribute : Attribute
    {

        public ErrorCodeAttribute(params ErrorCode[] errorCodes)
        {
            ErrorCodes = errorCodes;
        }

        public ErrorCode[] ErrorCodes { get; }
    }
}
