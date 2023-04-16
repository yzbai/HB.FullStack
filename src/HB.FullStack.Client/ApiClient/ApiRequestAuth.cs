using System;

namespace HB.FullStack.Client.ApiClient
{
    public class ApiRequestAuth : IEquatable<ApiRequestAuth>
    {
        public static ApiRequestAuth JWT = new ApiRequestAuth { AuthType = ApiAuthType.Jwt };
        public static ApiRequestAuth NONE = new ApiRequestAuth { AuthType = ApiAuthType.None };
        public static ApiRequestAuth APIKEY(string apiKeyName) => new ApiRequestAuth { AuthType = ApiAuthType.ApiKey, ApiKeyName = apiKeyName };

        public ApiAuthType AuthType { get; private set; }

        public string? ApiKeyName { get; private set; }

        public override bool Equals(object? obj)
        {
            if (obj is ApiRequestAuth other)
            {
                return other.AuthType == AuthType && other.ApiKeyName == ApiKeyName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AuthType, ApiKeyName);
        }

        public static bool operator ==(ApiRequestAuth? left, ApiRequestAuth? right)
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

        public static bool operator !=(ApiRequestAuth? left, ApiRequestAuth? right)
        {
            return !(left == right);
        }

        public bool Equals(ApiRequestAuth? other)
        {
            if (other is null)
            {
                return false;
            }

            return other.AuthType == AuthType && other.ApiKeyName == ApiKeyName;
        }
    }
}