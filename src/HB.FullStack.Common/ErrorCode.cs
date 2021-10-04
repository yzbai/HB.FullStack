using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System
{
    public static class ErrorCodeStartIds
    {
        public const int DATABASE = 1000;
        public const int CACHE = 2000;
        public const int EVENT_BUS = 3000;
        public const int KV_STORE = 4000;
        public const int LOCK = 5000;
        public const int REPOSITORY = 6000;
        public const int ALIYUN = 7000;
        public const int IDENTITY = 8000;
        public const int TENCENT = 9000;
        public const int WEB_API = 10000;
        public const int MOBILE = 11000;
        public const int USER_DOMAIN = 12000;
        public const int ROUTINE_DOMAIN = 12000;
        public const int THEME_DOMAIN = 12000;
    }

    //TODO: 使用Record改写
    public class ErrorCode : IEquatable<ErrorCode>
    {
        public static readonly ErrorCode Empty = new ErrorCode(0);

        public int Id { get; }

        public string? Name { get; }

        public string? Message { get; private set; }

        public string? Detail { get; set; }


        public static implicit operator ErrorCode(int i)
        {
            return new ErrorCode(i);
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

        public ErrorCode(int id, string? name = null, string? message = null)
        {
            Id = id;
            Name = name;
            Message = message;
        }

        public ErrorCode(ErrorCode other)
        {
            Id = other.Id;
            Name = other.Name;
            Message = other.Message;
        }

        public override string ToString()
        {
            return Name ?? Id.ToString(CultureInfo.InvariantCulture);
        }

        public bool Equals(ErrorCode? other)
        {
            if(ReferenceEquals(other,null))
            {
                return false;
            }

            return Id == other.Id;
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
            return Id;
        }

        public static ErrorCode FromInt32(int code)
        {
            return new ErrorCode(code);
        }

        public Microsoft.Extensions.Logging.EventId ToEventId()
        {
            return new Microsoft.Extensions.Logging.EventId(Id, Name);
        }

        public ErrorCode AppendDetail(string? detail)
        {
            Detail += detail;
            return this;
        }
    }
}