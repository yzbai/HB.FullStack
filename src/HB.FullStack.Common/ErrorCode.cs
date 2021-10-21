using Microsoft.Extensions.Logging;

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System
{
    public static partial class ErrorCodeStartIds
    {
        public const int COMMON = 0;
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
        public const int API = 12000;
    }

    public class ErrorCode : IEquatable<ErrorCode>
    {
        public int Id { get; } = -1;

        public string Name { get; } = null!;

        public string Message { get; } = null!;

        public ErrorCode(int id, string name, string message)
        {
            Id = id;
            Name = name;
            Message = message;
        }

        public override string ToString()
        {
            return Name ?? Id.ToString(CultureInfo.InvariantCulture);
        }

        public bool Equals(ErrorCode? other)
        {
            if(other is null)
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

        public EventId ToEventId()
        {
            return new EventId(Id, Name);
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

        //public static explicit operator EventId(ErrorCode errorCode)
        //{
        //    return errorCode.ToEventId();
        //}
    }
}