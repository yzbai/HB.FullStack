using Microsoft.Extensions.Logging;

namespace System
{
    public class ErrorCode : IEquatable<ErrorCode>
    {
        public static readonly ErrorCode Empty = new ErrorCode(nameof(Empty), "Empty ErrorCode");

        public string Code { get; } = null!;

        public string Message { get; } = null!;

        public ErrorCode(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public override string ToString()
        {
            return Code;
        }

        public bool Equals(ErrorCode? other)
        {
            if (other is null)
            {
                return false;
            }

            return Code == other.Code;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ErrorCode eventCode)
            {
                return Equals(eventCode);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code);
        }

        public EventId ToEventId()
        {
            return new EventId(GetHashCode(), Code);
        }

        public static bool operator ==(ErrorCode? left, ErrorCode? right)
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ErrorCode? left, ErrorCode? right)
        {
            return !(left == right);
        }
    }

    public static class ErrorCodeExtensions
    {
        public static ErrorCode WithMessage(this ErrorCode errorCode, string? messsage)
        {
            return new ErrorCode(errorCode.Code, messsage ?? errorCode.Message);
        }
    }
}