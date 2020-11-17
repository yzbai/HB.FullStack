#nullable enable

using System;

namespace HB.Framework.Common
{
    public class ValidateErrorException : FrameworkException
    {
        public ValidateErrorException()
        {
        }

        public ValidateErrorException(ISupportValidate supportValidate) : this(supportValidate?.GetValidateErrorMessage())
        {
        }

        public ValidateErrorException(string? message) : base(message)
        {
        }

        public ValidateErrorException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public ValidateErrorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public ValidateErrorException(ErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public ValidateErrorException(ErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }
    }
}

#nullable restore