using System;
using HB.FullStack.Common.Api.Requests;

namespace HB.FullStack.Common.Api
{
    public struct ApiRequestAuth : IEquatable<ApiRequestAuth>
    {
        public static ApiRequestAuth JWT = new ApiRequestAuth { AuthType = ApiAuthType.Jwt };

        public static ApiRequestAuth NONE = new ApiRequestAuth { AuthType = ApiAuthType.None };

        public ApiAuthType AuthType { get; set; }

        public string? ApiKeyName { get; set; }

        public override bool Equals(object? obj)
        {
            if(obj == null)
            {
                return false;
            }

            if(obj is ApiRequestAuth other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AuthType, ApiKeyName);
        }

        public static bool operator ==(ApiRequestAuth left, ApiRequestAuth right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ApiRequestAuth left, ApiRequestAuth right)
        {
            return !(left == right);
        }

        public bool Equals(ApiRequestAuth other)
        {
            return other.AuthType == AuthType && other.ApiKeyName == ApiKeyName;
        }
    }
}