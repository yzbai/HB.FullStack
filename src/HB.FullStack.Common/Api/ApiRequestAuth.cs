using System;

namespace HB.FullStack.Common.Api
{
    public class ApiRequestAuth2 : IEquatable<ApiRequestAuth2>
    {
        public static ApiRequestAuth2 JWT = new ApiRequestAuth2 { AuthType = ApiAuthType.Jwt };
        public static ApiRequestAuth2 NONE = new ApiRequestAuth2 { AuthType = ApiAuthType.None };
        public static ApiRequestAuth2 APIKEY(string apiKeyName) => new ApiRequestAuth2 { AuthType = ApiAuthType.ApiKey, ApiKeyName = apiKeyName };

        public ApiAuthType AuthType { get; private set; }

        public string? ApiKeyName { get; private set; }

        public override bool Equals(object? obj)
        {
            if (obj is ApiRequestAuth2 other)
            {
                return other.AuthType == AuthType && other.ApiKeyName == ApiKeyName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AuthType, ApiKeyName);
        }

        public static bool operator ==(ApiRequestAuth2? left, ApiRequestAuth2? right)
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

        public static bool operator !=(ApiRequestAuth2? left, ApiRequestAuth2? right)
        {
            return !(left == right);
        }

        public bool Equals(ApiRequestAuth2? other)
        {
            if (other is null)
            {
                return false;
            }

            return other.AuthType == AuthType && other.ApiKeyName == ApiKeyName;
        }
    }
}